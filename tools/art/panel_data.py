#!/usr/bin/env python3
"""PixelLab üretim panosu için veri dosyaları (kullanıcı isteği 2026-11-21).

Repodaki iş dosyalarından (art/queue) ve defterden (art/budget.md tablosu + art/ledger/*.md) üç JSON çıkarır.
Her dosya {"rows": [...], "meta": [...]} biçimindedir; pano bunları gösterir, zamanlanmış görev bunları yeniden
üretip yükler.

  ozet.json    tek satır: son bilinen PixelLab bakiyesi, aylık hak, bugün ve son 7 günde harcanan,
               iş durumlarının sayıları, uygun (başlayabilir) iş sayısı, Claude sınırı beklemesi
  gunluk.json  gün başına harcanan generation ve iş sayısı (son 30 gün)
  isler.json   son değişen 60 iş: numara, başlık, durum, tahmini/üst maliyet, harcanan, son değişiklik

Kullanım:
  python tools/art/panel_data.py --out <klasör>
Bakiye defterdeki en son "kalan (get_balance)" okumasıdır; işçiler her iş sonunda yazar. PixelLab'in
kendi sayacıyla birkaç generation fark edebilir (aynı anda çalışan işler).
"""
import argparse
import datetime
import glob
import json
import os
import re
import subprocess
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
sys.path.insert(0, os.path.dirname(__file__))
import queue_ready  # noqa: E402

QUEUE = os.path.join(ROOT, "art", "queue")
LEDGER = os.path.join(ROOT, "art", "ledger")
BUDGET = os.path.join(ROOT, "art", "budget.md")
MONTHLY = 10000          # Pixel Architect (art/budget.md "Plan")
STATUSES = ["pending", "running", "review", "accepted", "failed", "blocked", "rejected", "hold"]
NUM = re.compile(r"-?\d+(?:[.,]\d+)?")


def num(text):
    m = NUM.search(text or "")
    return float(m.group(0).replace(",", ".")) if m else None


def git_times(paths_glob):
    """Her dosyanın son commit zamanı (unix), tek git log çağrısıyla."""
    out = subprocess.run(["git", "log", "--format=@%ct", "--name-only", "--", paths_glob],
                         cwd=ROOT, capture_output=True, text=True).stdout
    times, cur = {}, None
    for line in out.splitlines():
        if line.startswith("@"):
            cur = int(line[1:])
        elif line.strip() and cur is not None:
            times.setdefault(line.strip(), cur)   # git log yeniden eskiye: ilk görülen en yeni
    return times


def ledger_rows():
    """(tarih, iş, harcanan, kalan, dosya) satırları: budget.md tablosu ve iş başına defter dosyaları."""
    rows = []
    files = [BUDGET] + sorted(glob.glob(os.path.join(LEDGER, "*.md")))
    for path in files:
        if not os.path.exists(path) or path.endswith("README.md"):
            continue
        with open(path, encoding="utf-8") as f:
            for line in f:
                cells = [c.strip() for c in line.strip().strip("|").split("|")]
                if len(cells) < 5 or not re.match(r"\d{4}-\d{2}-\d{2}$", cells[0]):
                    continue
                rows.append({"date": cells[0], "job": cells[1].split(" ")[0], "spent": num(cells[3]) or 0.0,
                             "balance": num(cells[4]), "file": os.path.relpath(path, ROOT).replace(os.sep, "/")})
    return rows


def front(path):
    with open(path, encoding="utf-8") as f:
        text = f.read()
    m = re.match(r"^---\n(.*?)\n---", text, re.S)
    out = {}
    for line in (m.group(1) if m else "").splitlines():
        km = re.match(r"^(\w+):\s*(.*)$", line)
        if km:
            out[km.group(1)] = km.group(2).split("  #")[0].split(" #")[0].strip().strip('"')
    return out


def build(today):
    ledger = ledger_rows()
    lt = git_times("art/ledger")
    bt = git_times("art/budget.md").get("art/budget.md", 0)
    with_balance = [r for r in ledger if r["balance"] is not None]
    # En güncel okuma: en son commit edilen defter dosyası; aynı dosyada son satır.
    latest = None
    if with_balance:
        latest = max(with_balance, key=lambda r: (r["date"], lt.get(r["file"], bt if r["file"] == "art/budget.md" else 0)))
    balance = latest["balance"] if latest else None
    balance_at = None
    if latest:
        t = lt.get(latest["file"]) or bt
        balance_at = datetime.datetime.fromtimestamp(t, datetime.timezone.utc).strftime("%Y-%m-%d %H:%M UTC") if t else latest["date"]

    by_day = {}
    for r in ledger:
        d = by_day.setdefault(r["date"], {"date": r["date"], "spent": 0.0, "jobs": set()})
        d["spent"] += r["spent"]
        if r["spent"]:
            d["jobs"].add(r["job"])
    t = datetime.date.fromisoformat(today)
    days = []
    for i in range(29, -1, -1):
        day = (t - datetime.timedelta(days=i)).isoformat()
        d = by_day.get(day)
        days.append({"date": day, "spent": round(d["spent"], 1) if d else 0.0, "jobs": len(d["jobs"]) if d else 0})
    week = sum(x["spent"] for x in days[-7:])

    qt = git_times("art/queue")
    spent_by_job = {}
    for r in ledger:
        spent_by_job[r["job"]] = spent_by_job.get(r["job"], 0.0) + r["spent"]
    jobs = []
    for path in glob.glob(os.path.join(QUEUE, "*.md")):
        fm = front(path)
        if not fm.get("id"):
            continue
        rel = os.path.relpath(path, ROOT).replace(os.sep, "/")
        ts = qt.get(rel, 0)
        jobs.append({
            "id": fm["id"], "title": fm.get("title", ""), "status": fm.get("status", "?").split()[0],
            "cost_est": num(fm.get("cost_est")), "cost_max": num(fm.get("cost_max")),
            "spent": round(spent_by_job.get(fm["id"], 0.0), 1) if fm["id"] in spent_by_job else None,
            "changed": datetime.datetime.fromtimestamp(ts, datetime.timezone.utc).strftime("%Y-%m-%d %H:%M") if ts else "",
            "_ts": ts,
        })
    counts = {s: 0 for s in STATUSES}
    for j in jobs:
        counts[j["status"]] = counts.get(j["status"], 0) + 1

    qjobs = queue_ready.load_jobs()
    spent_q, bal_q = queue_ready.read_budget(today)
    ready, _, _ = queue_ready.evaluate(qjobs, spent_q, bal_q, False, queue_ready.daily_limit(today), "bulut")
    cool = queue_ready.claude_cooldown() if hasattr(queue_ready, "claude_cooldown") else 0.0

    now = datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%d %H:%M UTC")
    sha = subprocess.run(["git", "rev-parse", "--short", "HEAD"], cwd=ROOT, capture_output=True, text=True).stdout.strip()
    with open(os.path.abspath(__file__), encoding="utf-8") as f:
        code = f.read()
    meta = [{"label": "Üretildi", "value": now}, {"label": "Repo", "value": f"main @ {sha}"},
            {"label": "Girdi", "value": "art/queue/*.md, art/ledger/*.md, art/budget.md"},
            {"label": "panel_data.py", "value": code, "type": "code", "lang": "python"}]

    ozet = [{
        "balance": balance, "balance_at": balance_at or "", "monthly": MONTHLY,
        "used_pct": round(100 * (MONTHLY - balance) / MONTHLY, 1) if balance is not None else None,
        "spent_today": round(by_day.get(today, {"spent": 0.0})["spent"], 1), "spent_7d": round(week, 1),
        "running": counts.get("running", 0), "pending": counts.get("pending", 0), "ready": len(ready),
        "review": counts.get("review", 0), "accepted": counts.get("accepted", 0),
        "failed": counts.get("failed", 0) + counts.get("blocked", 0), "rejected": counts.get("rejected", 0),
        "total_jobs": len(jobs), "claude_wait_min": round(cool),
        "generated_at": now,
    }]
    recent = sorted(jobs, key=lambda j: j["_ts"], reverse=True)[:60]
    for j in recent:
        j.pop("_ts")
    return {
        "ozet.json": {"rows": ozet, "meta": meta + [{"label": "Bakiye kaynağı", "value": (latest or {}).get("file", "-")}]},
        "gunluk.json": {"rows": days, "meta": meta},
        "isler.json": {"rows": recent, "meta": meta},
    }


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", required=True)
    ap.add_argument("--today", default=datetime.datetime.now(datetime.timezone.utc).date().isoformat())
    a = ap.parse_args()
    os.makedirs(a.out, exist_ok=True)
    for name, data in build(a.today).items():
        with open(os.path.join(a.out, name), "w", encoding="utf-8") as f:
            json.dump(data, f, ensure_ascii=False, indent=1)
        print(os.path.join(a.out, name), len(data["rows"]))


if __name__ == "__main__":
    main()
