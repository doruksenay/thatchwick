"""Eksik sesler: koddaki ses anahtarları (SoundKeys.cs) ile soundbank ve kuyruğu karşılaştırır.

Kullanım:
  python tools/audio/gaps.py            eksikleri listele
  python tools/audio/gaps.py --draft    eksikler için taslak iş dosyası aç (approved_by: pending; prompt'u Claude yazar)
Kurallar (45a bölüm 4): A sınıfı bir anahtarda 6'dan az varyasyon varsa da eksik sayılır.
"""
from __future__ import annotations

import argparse
import re
import sys

from common import BANK_FILE, QUEUE, ROOT, all_jobs, read_json, write_job

KEYS_FILE = ROOT / "Assets" / "Scripts" / "Core" / "Audio" / "SoundKeys.cs"
# public const string WorkChop = "work.chop"; // A tools
PAT = re.compile(r'const\s+string\s+\w+\s*=\s*"([^"]+)"\s*;\s*(?://\s*([ABC])?\s*(\w+)?)?')
MIN_A = 6


def code_keys() -> dict[str, tuple[str, str]]:
    out = {}
    for m in PAT.finditer(KEYS_FILE.read_text(encoding="utf-8")):
        out[m.group(1)] = (m.group(2) or "B", m.group(3) or "")
    return out


def main(argv=None) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--draft", action="store_true")
    a = ap.parse_args(argv)
    bank = {e["key"]: e for e in read_json(BANK_FILE, {"entries": []})["entries"]}
    queued = {}
    for j in all_jobs():
        if j.get("status") in ("rejected",):
            continue
        k = j.get("key")
        if k:
            queued[str(k)] = j.id
    missing, thin = [], []
    for key, (freq, fam) in sorted(code_keys().items()):
        if key.startswith("voice."):  # sesler voice işlerinden gelir
            continue
        e = bank.get(key)
        if e is None and key not in queued:
            missing.append((key, freq, fam))
        elif e is not None and freq == "A":
            n = len(e.get("clips", [])) or min((len(L["clips"]) for L in e.get("layers", [])), default=0)
            if n < MIN_A and key not in queued:
                thin.append((key, n))
    for key, freq, fam in missing:
        print(f"EKSİK  {key:32} sıklık {freq} aile {fam or '?'}")
    for key, n in thin:
        print(f"AZ     {key:32} {n} varyasyon (A sınıfında en az {MIN_A})")
    if not missing and not thin:
        print("Eksik ses yok.")
    if a.draft:
        for i, (key, freq, fam) in enumerate(missing):
            jid = "g" + re.sub(r"[^a-z0-9]+", "-", key.lower())
            p = QUEUE / "taslak" / f"{jid}.md"
            if p.exists():
                continue
            p.parent.mkdir(parents=True, exist_ok=True)
            meta = {"id": jid, "title": key, "kind": "sfx", "family": fam or "foley", "freq": freq,
                    "status": "ready", "approved_by": "pending", "key": key, "cost_max": 1500,
                    "duration_s": 1.0}
            write_job(p, meta, "TODO: prompt yazılacak (Claude). İngilizce, tek olay, aile ön eki otomatik eklenir.\n")
            print("taslak:", p.relative_to(ROOT))
    return 0


if __name__ == "__main__":
    sys.exit(main())
