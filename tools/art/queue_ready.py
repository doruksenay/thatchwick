#!/usr/bin/env python3
"""Asset kuyruğunda şu an çalışabilecek işleri listeler (PixelLab'e dokunmaz, generation harcamaz).

Kurallar art/budget.md ve art/README.md ile aynıdır; işçi (pixellab-artist) iş seçerken,
yerel yoklayıcı (tools/art-poll.cmd) ve bulut işçisi (.github/workflows/art-worker.yml)
Claude'u boşuna başlatmamak için bunu kullanır.

Bir iş uygundur, eğer:
  - status: pending
  - approved_by: user, ya da approved_by: auto ve cost_max <= 25
  - depends_on içindeki her iş `accepted` (art/budget.md kuralı).
    --allow-review-deps verilirse stil pilotu olmayan (`style_gate: true` DEĞİL) bağımlılıklar `review` iken de
    karşılanmış sayılır. Bu, kullanıcı kararı bekleyen bir öneridir; budget.md'ye yazılmadan işçiler kullanmaz.
  - cost_max bugünkü kalan günlük tavana (150) ve rezerve (200) sığıyor
  - status: running olan ama iş dosyası STALE_HOURS (6) saattir değişmemiş iş takılı sahiplenme sayılır
    (çöken işçi); pending gibi yeniden uygun olur. İşçiler en çok 90 dakika çalışır.

Kullanım:
  python tools/art/queue_ready.py            # uygun işleri ve nedenleri yazar; uygun iş varsa çıkış 0, yoksa 1
  python tools/art/queue_ready.py --quiet    # yalnızca çıkış kodu
  python tools/art/queue_ready.py --json     # makine için
  python tools/art/queue_ready.py --all      # her işin neden bekledigini de yazar
  python tools/art/queue_ready.py --claim    # ilk uygun işi `status: running` yapar, kimliğini yazar (bulut işçisi)
"""
import argparse
import datetime
import glob
import json
import os
import re
import subprocess
import sys
import time

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
QUEUE = os.path.join(ROOT, "art", "queue")
BUDGET = os.path.join(ROOT, "art", "budget.md")

AUTO_LIMIT = 25.0
DAILY_LIMIT = 150.0
RESERVE = 200.0
STALE_HOURS = 6.0


def _num(text, default=0.0):
    m = re.search(r"-?\d+(?:[.,]\d+)?", text or "")
    return float(m.group(0).replace(",", ".")) if m else default


def front_matter(path):
    with open(path, encoding="utf-8") as f:
        text = f.read()
    if not text.startswith("---"):
        return {}
    block = text.split("---", 2)[1]
    data = {}
    for line in block.splitlines():
        m = re.match(r"^([A-Za-z_]+):\s*(.*)$", line)
        if not m:
            continue
        key, val = m.group(1), m.group(2)
        val = re.sub(r"\s+#.*$", "", val).strip()     # satır sonu yorumu
        data[key] = val
    return data


def last_change(path):
    """İş dosyasının son commit zamanı (epoch saniye); commit edilmemiş değişiklik varsa ya da git yoksa None."""
    try:
        rel = os.path.relpath(path, ROOT)
        dirty = subprocess.run(["git", "status", "--porcelain", "--", rel], cwd=ROOT, capture_output=True, text=True, timeout=30)
        if dirty.returncode != 0 or dirty.stdout.strip():
            return None
        out = subprocess.run(["git", "log", "-1", "--format=%ct", "--", rel], cwd=ROOT, capture_output=True, text=True, timeout=30)
        return float(out.stdout.strip()) if out.returncode == 0 and out.stdout.strip() else None
    except (OSError, ValueError, subprocess.SubprocessError):
        return None


def job_id(path, fm):
    raw = fm.get("id", "").strip().strip('"').strip("'")
    if raw:
        return raw
    return os.path.basename(path).split("-", 1)[0]


def parse_list(val):
    return [x.strip().strip('"').strip("'") for x in re.findall(r'"[^"]*"|\'[^\']*\'|[^\[\],\s]+', val or "")
            if x.strip().strip('"').strip("'")]


def read_budget(today):
    """Bugün harcanan ve defterdeki son kalan bakiye (defter; işçi get_balance ile doğrular)."""
    spent_today, last_balance = 0.0, None
    if not os.path.exists(BUDGET):
        return spent_today, last_balance
    with open(BUDGET, encoding="utf-8") as f:
        for line in f:
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            if len(cells) < 5 or not re.match(r"\d{4}-\d{2}-\d{2}", cells[0]):
                continue
            if cells[0] == today:
                spent_today += _num(cells[3])
            bal = _num(cells[4], None)
            if bal is not None:
                last_balance = bal
    return spent_today, last_balance


def load_jobs():
    jobs = {}
    for path in sorted(glob.glob(os.path.join(QUEUE, "*.md"))):
        fm = front_matter(path)
        if not fm:
            continue
        jid = job_id(path, fm)
        status = fm.get("status", "")
        stale = False
        if status == "running":
            changed = last_change(path)
            stale = changed is not None and time.time() - changed > STALE_HOURS * 3600
        jobs[jid] = {
            "id": jid,
            "file": os.path.relpath(path, ROOT).replace("\\", "/"),
            "status": status,
            "stale_claim": stale,
            "approved_by": fm.get("approved_by", ""),
            "cost_max": _num(fm.get("cost_max"), 0.0),
            "depends_on": parse_list(fm.get("depends_on", "")),
            "style_gate": fm.get("style_gate", "").lower() == "true",
            "family": fm.get("family", ""),
        }
    return jobs


def evaluate(jobs, spent_today, balance, allow_review_deps=False):
    remaining_today = max(0.0, DAILY_LIMIT - spent_today)
    ready, waiting = [], []
    for j in jobs.values():
        why = []
        if j["status"] != "pending" and not j["stale_claim"]:
            continue
        if j["approved_by"] == "auto":
            if j["cost_max"] > AUTO_LIMIT:
                why.append(f"auto ama cost_max {j['cost_max']:g} > {AUTO_LIMIT:g} (approved_by: user gerekir)")
        elif j["approved_by"] != "user":
            why.append(f"onay yok (approved_by: {j['approved_by'] or '-'})")
        for d in j["depends_on"]:
            dep = jobs.get(d)
            if dep is None:
                why.append(f"bağımlılık {d} bulunamadı")
            elif dep["status"] == "accepted":
                continue
            elif dep["status"] == "review" and allow_review_deps and not dep["style_gate"]:
                continue
            elif dep["status"] == "review":
                why.append(f"{d} review, kabulünü bekliyor" + (" (stil pilotu)" if dep["style_gate"] else ""))
            else:
                why.append(f"{d} {dep['status'] or '?'}")
        if j["cost_max"] > remaining_today:
            why.append(f"günlük tavan: bugün kalan {remaining_today:g}")
        if balance is not None and balance - j["cost_max"] < RESERVE:
            why.append(f"rezerv: defter bakiyesi {balance:g}")
        (waiting if why else ready).append((j, why))
    ready.sort(key=lambda t: t[0]["id"])
    waiting.sort(key=lambda t: t[0]["id"])
    return ready, waiting, remaining_today


def claim(job):
    """İş dosyasının ön bilgisindeki status satırını running yapar (yalnızca ilk --- bloğunda)."""
    path = os.path.join(ROOT, job["file"])
    with open(path, encoding="utf-8") as f:
        text = f.read()
    head, sep, rest = text.partition("---")
    block, sep2, body = rest.partition("---")
    new_block, n = re.subn(r"(?m)^status:[^\n]*$", "status: running", block, count=1)
    if not sep or not sep2 or n != 1:
        raise SystemExit(f"{job['file']}: status satırı bulunamadı")
    # Sahiplenme zamanı: takılı sahiplenme yeniden alınırken de dosya değişir (commit edilir, yaşı sıfırlanır).
    stamp = "claimed_at: " + datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%MZ")
    new_block, m = re.subn(r"(?m)^claimed_at:[^\n]*$", stamp, new_block, count=1)
    if m == 0:
        new_block = re.sub(r"(?m)^(status: running)$", "\\1\n" + stamp, new_block, count=1)
    with open(path, "w", encoding="utf-8", newline="") as f:
        f.write(head + sep + new_block + sep2 + body)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--quiet", action="store_true")
    ap.add_argument("--json", action="store_true")
    ap.add_argument("--all", action="store_true")
    ap.add_argument("--today", default=datetime.date.today().isoformat())
    ap.add_argument("--allow-review-deps", action="store_true", help="öneri: pilot olmayan review bağımlılıkları yeterli say")
    ap.add_argument("--claim", action="store_true", help="ilk uygun işi running yap; kimliğini ve dosyasını yaz")
    a = ap.parse_args()

    jobs = load_jobs()
    spent, balance = read_budget(a.today)
    ready, waiting, remaining = evaluate(jobs, spent, balance, a.allow_review_deps)

    if a.claim:
        if not ready:
            sys.exit(1)
        job = ready[0][0]
        claim(job)
        print(f"{job['id']} {job['file']}")
        sys.exit(0)

    if a.json:
        print(json.dumps({
            "today": a.today, "spent_today": spent, "remaining_today": remaining, "ledger_balance": balance,
            "ready": [j for j, _ in ready],
            "waiting": [dict(j, why=w) for j, w in waiting],
        }, ensure_ascii=False, indent=2))
    elif not a.quiet:
        print(f"Bugün harcanan {spent:g}, günlük kalan {remaining:g}, defter bakiyesi {balance if balance is not None else '?'}")
        print(f"Uygun iş: {len(ready)}")
        for j, _ in ready:
            note = "  (takılı sahiplenme: yeniden alınabilir)" if j["stale_claim"] else ""
            print(f"  {j['id']:>5}  {j['cost_max']:>4g}  {j['file']}{note}")
        if a.all and waiting:
            print(f"Bekleyen: {len(waiting)}")
            for j, w in waiting:
                print(f"  {j['id']:>5}  {'; '.join(w)}")
    sys.exit(0 if ready else 1)


if __name__ == "__main__":
    main()
