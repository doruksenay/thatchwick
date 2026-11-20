#!/usr/bin/env python3
"""Yerini ölçek seti (_v2) alan eski hayvan görsellerini Assets/Art/Iso/_arsiv/ klasörüne taşır.

Kullanıcı kararı 2026-11-21: eski görseller silinmez, "_arsiv" klasöründe saklanır; oyun ve görsel kütüphanesi
bu klasörü okumaz (IsoArtLibraryBuilder, art_link.py).

Hangi dosyaların taşınacağını iş dosyaları söyler: frontmatter'daki `supersedes:` listesi (glob, depo köküne göre).
Yalnızca `status: accepted` olan işlerin listesi uygulanır; reddedilen ya da incelemedeki iş eski görsele dokunmaz.

Taşıma `git mv` ile yapılır ve her PNG'nin .meta dosyası da birlikte taşınır (GUID korunur; .meta yazılmaz).
Unity açıkken çalıştırma: önce Unity'yi kapat ya da taşımadan sonra Unity'de Assets > Refresh yap.

Kullanım:
  python tools/art/archive_superseded.py            # yalnızca rapor (ne taşınacak)
  python tools/art/archive_superseded.py --apply    # taşı (git mv)
  python tools/art/archive_superseded.py --job 781  # yalnızca bu iş
"""
import argparse
import glob
import os
import re
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
QUEUE = os.path.join(ROOT, "art", "queue")
ARCHIVE = "Assets/Art/Iso/_arsiv"


def frontmatter(path):
    with open(path, encoding="utf-8") as f:
        text = f.read()
    m = re.match(r"^---\n(.*?)\n---", text, re.S)
    if not m:
        return None
    fm = {"supersedes": []}
    key = None
    for line in m.group(1).splitlines():
        if re.match(r"^\s+-\s", line) and key == "supersedes":
            fm["supersedes"].append(line.strip()[2:].split("#")[0].strip())
            continue
        km = re.match(r"^([a-z_]+):\s*(.*)$", line)
        if km:
            key = km.group(1)
            if key != "supersedes":
                fm[key] = km.group(2).split("#")[0].strip().strip('"')
    return fm


def expand(pattern):
    """{a,b} kalıplarını açar, sonra glob uygular."""
    m = re.search(r"\{([^{}]*)\}", pattern)
    if m:
        out = []
        for part in m.group(1).split(","):
            out += expand(pattern[:m.start()] + part + pattern[m.end():])
        return out
    return sorted(glob.glob(os.path.join(ROOT, pattern), recursive=True))


def plan(job_filter=None):
    moves = []
    for path in sorted(glob.glob(os.path.join(QUEUE, "*.md"))):
        fm = frontmatter(path)
        if not fm or not fm["supersedes"]:
            continue
        if job_filter and fm.get("id") != job_filter:
            continue
        if fm.get("status") != "accepted":
            print(f"atla  {os.path.basename(path)}: status {fm.get('status')} (yalnızca accepted taşınır)")
            continue
        for pat in fm["supersedes"]:
            for src in expand(pat):
                if not src.endswith(".png") or "/_arsiv/" in src.replace("\\", "/"):
                    continue
                rel = os.path.relpath(src, ROOT).replace("\\", "/")
                sub = rel[len("Assets/Art/Iso/"):] if rel.startswith("Assets/Art/Iso/") else os.path.basename(rel)
                moves.append((rel, f"{ARCHIVE}/{sub}"))
    return moves


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--apply", action="store_true")
    ap.add_argument("--job")
    a = ap.parse_args()
    moves = plan(a.job)
    if not moves:
        print("taşınacak dosya yok")
        return 0
    for src, dst in moves:
        print(f"{'taşı' if a.apply else 'plan'}  {src} -> {dst}")
        if not a.apply:
            continue
        os.makedirs(os.path.join(ROOT, os.path.dirname(dst)), exist_ok=True)
        subprocess.run(["git", "mv", "-k", src, dst], cwd=ROOT, check=True)
        if os.path.exists(os.path.join(ROOT, src + ".meta")):
            subprocess.run(["git", "mv", "-k", src + ".meta", dst + ".meta"], cwd=ROOT, check=True)
    print(f"{len(moves)} görsel {'taşındı' if a.apply else 'taşınacak'}")
    if a.apply:
        print("Klasörün .meta dosyasını Unity üretir: Unity'yi açıp Assets > Refresh, sonra _arsiv.meta'yı da commit et.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
