"""314 işi: ham kareleri PixelLab yükleme adresine gönderir, upload:<id> listesini yazar.

Kullanım: python tools/art/upload314.py <yukleme_url>
"""
import json
import sys
import urllib.request
import uuid
from pathlib import Path

U = sys.argv[1]
out = {}
for n in ["hatice", "zeynep", "murat", "kerem", "ayse", "orhan"]:
    out[n] = []
    for d in ["s", "se", "e", "ne", "n", "nw", "w", "sw"]:
        data = Path(f"Assets/Art/Iso/_aday/_ham/iso_char_{n}_56_cand_{d}.png").read_bytes()
        b = uuid.uuid4().hex
        head = (f'--{b}\r\nContent-Disposition: form-data; name="file"; '
                f'filename="{n}_{d}.png"\r\nContent-Type: image/png\r\n\r\n').encode()
        body = head + data + f"\r\n--{b}--\r\n".encode()
        req = urllib.request.Request(U, body, {"Content-Type": f"multipart/form-data; boundary={b}"})
        out[n].append(json.load(urllib.request.urlopen(req))["ref"])
Path("art/tmp314/refs.json").write_text(json.dumps(out))
for n, v in out.items():
    print(n, json.dumps(v))
