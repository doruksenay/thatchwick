#!/usr/bin/env python3
"""Bulut işçisi bir işi bitirince kullanıcıya bildirim yazar (kullanıcı kararı 2026-11-21).

Bildirim, depodaki "art-bildirim" etiketli açık konuya (issue) yorum olarak düşer ve depo sahibini
anar; GitHub uygulaması ve e-postası bunu anında iletir. Konu yoksa oluşturulur.

Kullanım (iş akışında, GH_TOKEN ve GITHUB_REPOSITORY ortamda):
  python tools/art/notify.py <iş> <push_sonucu> [--dry-run]
push_sonucu: art-worker.yml "Denetle ve push et" adımının outcome değeri (success, failure, skipped...).
--dry-run: yorumu göndermez, yalnızca metni yazdırır.
"""
import json
import os
import re
import subprocess
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
QUEUE = os.path.join(ROOT, "art", "queue")
LEDGER = os.path.join(ROOT, "art", "ledger")
RESULTS = os.path.join(ROOT, "art", "results")
LABEL = "art-bildirim"
TITLE = "Asset bildirimleri (bulut işçisi)"

STATUS_TEXT = {
    "review": "bitti, incelemeni bekliyor",
    "failed": "başarısız",
    "blocked": "durdu, kararını bekliyor",
    "running": "bitmedi (işçi sonuç yazmadı)",
}


def job_file(job):
    for name in sorted(os.listdir(QUEUE)):
        if name.startswith(job + "-") and name.endswith(".md"):
            return os.path.join(QUEUE, name)
    return None


def front(path):
    """İş dosyasının ön bilgisinden title ve status."""
    with open(path, encoding="utf-8") as f:
        text = f.read()
    m = re.match(r"^---\n(.*?)\n---", text, re.S)
    block = m.group(1) if m else ""
    out = {}
    for key in ("title", "status"):
        km = re.search(rf"^{key}:\s*(.*)$", block, re.M)
        if km:
            out[key] = km.group(1).split("#")[0].strip().strip('"')
    qm = re.search(r"^## Sorular\s*\n(.*?)(?=^## |^<!--|\Z)", text, re.S | re.M)
    out["sorular"] = qm.group(1).strip() if qm else ""
    return out


def spent(job):
    """art/ledger/<iş>.md satırlarındaki harcanan toplamı ve son bakiye."""
    path = os.path.join(LEDGER, f"{job}.md")
    if not os.path.exists(path):
        return None, None
    total, balance = 0.0, None
    with open(path, encoding="utf-8") as f:
        for line in f:
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            if len(cells) < 5 or not re.match(r"\d{4}-\d{2}-\d{2}", cells[0]):
                continue
            n = re.search(r"-?\d+(?:[.,]\d+)?", cells[3])
            if n:
                total += float(n.group(0).replace(",", "."))
            b = re.search(r"-?\d+(?:[.,]\d+)?", cells[4])
            if b:
                balance = b.group(0)
    return total, balance


def results(job):
    if not os.path.isdir(RESULTS):
        return []
    return sorted(n for n in os.listdir(RESULTS) if n.startswith(job + "-") or n == f"{job}.md")


def body(job, push_outcome, repo, owner):
    path = job_file(job)
    fm = front(path) if path else {}
    status = fm.get("status", "?")
    if push_outcome not in ("success", ""):
        line = f"push edilemedi ({push_outcome}); iş `running` kalır, 6 saat sonra yeniden alınabilir"
    else:
        line = STATUS_TEXT.get(status, status)
    total, balance = spent(job)
    base = f"https://github.com/{repo}/blob/main"
    parts = [f"@{owner} **{job}** {fm.get('title', '')}: {line}."]
    if total is not None:
        parts.append(f"Harcanan: {total:g} gen" + (f", kalan bakiye: {balance}" if balance else "") + ".")
    files = results(job)
    if files:
        parts.append("Sonuç: " + ", ".join(f"[{n}]({base}/art/results/{n})" for n in files[:6]))
    if path:
        parts.append(f"İş dosyası: [{os.path.basename(path)}]({base}/art/queue/{os.path.basename(path)})")
    if fm.get("sorular"):
        parts.append("**Sorular:**\n" + fm["sorular"][:1500])
    return "\n\n".join(parts)


def gh(*args, data=None):
    cmd = ["gh", "api", *args]
    res = subprocess.run(cmd, input=json.dumps(data) if data else None, capture_output=True, text=True)
    if res.returncode != 0:
        raise RuntimeError(res.stderr.strip())
    return json.loads(res.stdout or "null")


def issue_number(repo):
    found = gh(f"repos/{repo}/issues?labels={LABEL}&state=open&per_page=1")
    if found:
        return found[0]["number"]
    try:
        gh(f"repos/{repo}/labels", "--input", "-", data={"name": LABEL, "color": "8a5a2b"})
    except RuntimeError:
        pass   # etiket zaten var
    made = gh(f"repos/{repo}/issues", "--input", "-", data={
        "title": TITLE, "labels": [LABEL],
        "body": "Bulut işçisi (art-worker.yml) her işi bitirince buraya yorum yazar ve depo sahibini anar. "
                "Bu konuyu kapatma; kapatılırsa işçi yenisini açar."})
    return made["number"]


def main(argv):
    if len(argv) < 3:
        print(__doc__)
        return 2
    job, outcome = argv[1], argv[2]
    repo = os.environ.get("GITHUB_REPOSITORY", "")
    owner = os.environ.get("NOTIFY_USER") or repo.split("/")[0]
    text = body(job, outcome, repo, owner)
    if "--dry-run" in argv:
        print(text)
        return 0
    num = issue_number(repo)
    gh(f"repos/{repo}/issues/{num}/comments", "--input", "-", data={"body": text})
    print(f"bildirim: konu #{num}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
