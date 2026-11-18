#!/usr/bin/env python3
"""PixelLab çıktısını PNG olarak diske yazar (bulut ve yerel işçi; curl gerekmez).

Kaynak üç biçimde olabilir:
  - http(s) URL: indirilir. ZIP gelirse içindeki PNG'ler hedef klasöre açılır.
  - Dosya yolu (ör. Claude Code'un tool-results kopyası): PNG ise kopyalanır; metin/JSON ise içindeki
    base64 PNG'ler bulunur ve --index ile seçilen (varsayılan 0) yazılır. --all hepsini yazar (hedef klasör olur).
  - data:image/png;base64,... metni.

Kullanım:
  python tools/art/save_png.py <kaynak> <hedef.png> [--index N]
  python tools/art/save_png.py <kaynak> <hedef_klasör> --all
Üzerine yazmaz: hedef varsa hata verir (CLAUDE.md: var olan PNG'nin üzerine yazılmaz).
"""
import argparse
import base64
import io
import os
import re
import sys
import urllib.request
import zipfile

PNG_MAGIC = b"\x89PNG\r\n\x1a\n"
B64_PNG = re.compile(rb"iVBORw0KGgo[A-Za-z0-9+/=\\n\\r]+")


def fetch(src):
    if src.startswith(("http://", "https://")):
        req = urllib.request.Request(src, headers={"User-Agent": "thatchwick-art-worker"})
        with urllib.request.urlopen(req, timeout=120) as r:
            return r.read()
    if src.startswith("data:image/png;base64,"):
        return base64.b64decode(src.split(",", 1)[1])
    with open(os.path.expanduser(src), "rb") as f:
        return f.read()


def pngs_in(data):
    if data.startswith(PNG_MAGIC):
        return [data]
    if data.startswith(b"PK"):
        out = []
        with zipfile.ZipFile(io.BytesIO(data)) as z:
            for n in sorted(z.namelist()):
                if n.lower().endswith(".png"):
                    out.append((n, z.read(n)))
        return out
    found = []
    for m in B64_PNG.finditer(data):
        raw = m.group(0).replace(b"\\n", b"").replace(b"\\r", b"").replace(b"\n", b"").replace(b"\r", b"")
        raw = raw[: len(raw) - len(raw) % 4]
        try:
            b = base64.b64decode(raw)
        except Exception:
            continue
        if b.startswith(PNG_MAGIC):
            found.append(b)
    return found


def write_new(path, data):
    if os.path.exists(path):
        raise SystemExit(f"hedef zaten var, üzerine yazılmaz: {path}")
    os.makedirs(os.path.dirname(os.path.abspath(path)), exist_ok=True)
    with open(path, "wb") as f:
        f.write(data)
    w = int.from_bytes(data[16:20], "big") if data.startswith(PNG_MAGIC) else 0
    h = int.from_bytes(data[20:24], "big") if data.startswith(PNG_MAGIC) else 0
    print(f"yazıldı: {path} ({w}x{h}, {len(data)} bayt)")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("source")
    ap.add_argument("dest")
    ap.add_argument("--index", type=int, default=0)
    ap.add_argument("--all", action="store_true")
    a = ap.parse_args()
    items = pngs_in(fetch(a.source))
    if not items:
        raise SystemExit("kaynakta PNG bulunamadı")
    if a.all:
        for i, it in enumerate(items):
            name, data = it if isinstance(it, tuple) else (f"{i:02d}.png", it)
            write_new(os.path.join(a.dest, os.path.basename(name)), data)
        return
    it = items[a.index]
    write_new(a.dest, it[1] if isinstance(it, tuple) else it)


if __name__ == "__main__":
    main()
