#!/usr/bin/env python3
"""Karakter rotasyon görüntülerini indirir ve get_upload_url adresine yükler (gen yok).
Kullanım: python tools/art/fetch_rot.py <upload_url> <cikti_klasor>
"""
import json
import subprocess
import sys
import urllib.request

BASE = ("https://backblaze.pixellab.ai/file/pixellab-characters/aac48078-9292-4106-bb52-5708e6df86c7/"
        "37cebe59-b761-4dd8-b23c-b11f60f58294/rotations/%s.png?t=1794136056")
DIRS = "south-east south east north-east north north-west west south-west".split()

url, out = sys.argv[1], sys.argv[2]
for d in DIRS:
    p = f"{out}/{d}.png"
    try:
        req = urllib.request.Request(BASE % d, headers={"User-Agent": "Mozilla/5.0"})
        open(p, "wb").write(urllib.request.urlopen(req).read())
    except Exception as e:
        print(d, "indirme hata", e)
        continue
    try:
        r = subprocess.run(["curl", "-sS", "-F", "file=@" + p, url], capture_output=True, text=True)
        print(d, r.stdout.strip(), r.stderr.strip())
    except Exception as e:
        print(d, "yükleme hata", e)
