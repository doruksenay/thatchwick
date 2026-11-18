#!/usr/bin/env python3
"""Asset kuyruğunda şu an çalışabilecek işleri listeler (PixelLab'e dokunmaz, generation harcamaz).

Kurallar art/budget.md ve art/README.md ile aynıdır; işçi (pixellab-artist) iş seçerken,
yerel yoklayıcı (tools/art-poll.cmd) ve bulut işçisi (.github/workflows/art-worker.yml)
Claude'u boşuna başlatmamak için bunu kullanır.

Bir iş uygundur, eğer (art/budget.md "Otomatik akış", kullanıcı kararı 2026-11-17):
  - status: pending
  - approved_by: user ya da auto (pending = açık soru, çalışmaz)
  - depends_on içindeki her iş `accepted` ya da `review` (stil pilotları dahil)
  - cost_max <= 60 (çalışma tavanı; büyük iş bölünmeli)
  - cost_max bugünkü kalan günlük tavana (150; art/budget.md "Günlük tavan istisnaları" o günü
    değiştirebilir, `sınırsız` = tavan yok) ve rezerve (200) sığıyor
  --strict: eski kural (yalnızca accepted bağımlılık, auto için cost_max <= 25).

Kullanım:
  python tools/art/queue_ready.py            # uygun işleri ve nedenleri yazar; uygun iş varsa çıkış 0, yoksa 1
  python tools/art/queue_ready.py --quiet    # yalnızca çıkış kodu
  python tools/art/queue_ready.py --json     # makine için
  python tools/art/queue_ready.py --all      # her işin neden bekledigini de yazar
"""
import argparse
import datetime
import glob
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
QUEUE = os.path.join(ROOT, "art", "queue")
BUDGET = os.path.join(ROOT, "art", "budget.md")

AUTO_LIMIT = 25.0   # yalnızca --strict
RUN_LIMIT = 60.0
DAILY_LIMIT = 150.0
RESERVE = 200.0


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


def job_id(path, fm):
    raw = fm.get("id", "").strip().strip('"').strip("'")
    if raw:
        return raw
    return os.path.basename(path).split("-", 1)[0]


def parse_list(val):
    return [x.strip().strip('"').strip("'") for x in re.findall(r'"[^"]*"|\'[^\']*\'|[^\[\],\s]+', val or "")
            if x.strip().strip('"').strip("'")]


def daily_limit(today):
    """Bugünün günlük tavanı: art/budget.md "Günlük tavan istisnaları" listesinde varsa o değer."""
    if not os.path.exists(BUDGET):
        return DAILY_LIMIT
    with open(BUDGET, encoding="utf-8") as f:
        for line in f:
            m = re.match(r"^\s*-\s*(\d{4}-\d{2}-\d{2})\s*:\s*(\S+)", line)
            if not m or m.group(1) != today:
                continue
            val = m.group(2).lower()
            if val.startswith("sınırsız") or val.startswith("sinirsiz") or val.startswith("unlimited"):
                return float("inf")
            return _num(val, DAILY_LIMIT)
    return DAILY_LIMIT


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
        jobs[jid] = {
            "id": jid,
            "file": os.path.relpath(path, ROOT).replace("\\", "/"),
            "status": fm.get("status", ""),
            "approved_by": fm.get("approved_by", ""),
            "cost_max": _num(fm.get("cost_max"), 0.0),
            "depends_on": parse_list(fm.get("depends_on", "")),
            "style_gate": fm.get("style_gate", "").lower() == "true",
            "family": fm.get("family", ""),
        }
    return jobs


def evaluate(jobs, spent_today, balance, strict=False, limit=DAILY_LIMIT):
    remaining_today = max(0.0, limit - spent_today)
    ready, waiting = [], []
    for j in jobs.values():
        why = []
        if j["status"] != "pending":
            continue
        if j["approved_by"] == "auto" and strict:
            if j["cost_max"] > AUTO_LIMIT:
                why.append(f"auto ama cost_max {j['cost_max']:g} > {AUTO_LIMIT:g} (approved_by: user gerekir)")
        elif j["approved_by"] not in ("user", "auto"):
            why.append(f"onay yok (approved_by: {j['approved_by'] or '-'})")
        for d in j["depends_on"]:
            dep = jobs.get(d)
            if dep is None:
                why.append(f"bağımlılık {d} bulunamadı")
            elif dep["status"] == "accepted":
                continue
            elif dep["status"] == "review" and not strict:
                continue
            elif dep["status"] == "review":
                why.append(f"{d} review, kabulünü bekliyor" + (" (stil pilotu)" if dep["style_gate"] else ""))
            else:
                why.append(f"{d} {dep['status'] or '?'}")
        if j["cost_max"] > RUN_LIMIT:
            why.append(f"cost_max {j['cost_max']:g} > çalışma tavanı {RUN_LIMIT:g}: iş bölünmeli")
        if j["cost_max"] > remaining_today:
            why.append(f"günlük tavan: bugün kalan {remaining_today:g}")
        if balance is not None and balance - j["cost_max"] < RESERVE:
            why.append(f"rezerv: defter bakiyesi {balance:g}")
        (waiting if why else ready).append((j, why))
    ready.sort(key=lambda t: t[0]["id"])
    waiting.sort(key=lambda t: t[0]["id"])
    return ready, waiting, remaining_today


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--quiet", action="store_true")
    ap.add_argument("--json", action="store_true")
    ap.add_argument("--all", action="store_true")
    ap.add_argument("--today", default=datetime.date.today().isoformat())
    ap.add_argument("--strict", action="store_true", help="eski kural: yalnızca accepted bağımlılık, auto için cost_max <= 25")
    ap.add_argument("--allow-review-deps", action="store_true", help="artık varsayılan; geriye uyumluluk için kabul edilir")
    a = ap.parse_args()

    jobs = load_jobs()
    spent, balance = read_budget(a.today)
    limit = daily_limit(a.today)
    ready, waiting, remaining = evaluate(jobs, spent, balance, a.strict, limit)

    if a.json:
        print(json.dumps({
            "today": a.today, "daily_limit": None if limit == float("inf") else limit,
            "spent_today": spent, "remaining_today": None if remaining == float("inf") else remaining, "ledger_balance": balance,
            "ready": [j for j, _ in ready],
            "waiting": [dict(j, why=w) for j, w in waiting],
        }, ensure_ascii=False, indent=2))
    elif not a.quiet:
        print(f"Bugün harcanan {spent:g}, günlük kalan {'sınırsız' if remaining == float('inf') else f'{remaining:g}'}, defter bakiyesi {balance if balance is not None else '?'}")
        print(f"Uygun iş: {len(ready)}")
        for j, _ in ready:
            print(f"  {j['id']:>5}  {j['cost_max']:>4g}  {j['file']}")
        if a.all and waiting:
            print(f"Bekleyen: {len(waiting)}")
            for j, w in waiting:
                print(f"  {j['id']:>5}  {'; '.join(w)}")
    sys.exit(0 if ready else 1)


if __name__ == "__main__":
    main()
