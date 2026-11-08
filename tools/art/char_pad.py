#!/usr/bin/env python3
"""Karakter karesini ölçeklemeden, yalnızca saydam piksel ekleyerek kare tuvale getirir (gen yok).

Kullanım:
  python tools/art/char_pad.py <girdi.png> <cikti.png> --ref <ayni_yon_referans.png> [--canvas 60]

- Ölçekleme yok; piksel değerleri ve alfa olduğu gibi kopyalanır.
- Yatay: girdi tuvali hedef tuvalde ortalanır (ofset (C - w) // 2), böylece üreticinin
  yön başına yatay yerleşimi korunur.
- Dikey: girdinin en alt opak satırı, referans karenin (aynı yön) en alt opak satırına gelir.
- Çıktı varsa üzerine yazmaz, hata verir.
"""
import os
import sys
from PIL import Image


def bottom_row(im):
    a = im.split()[3]
    bb = a.point(lambda v: 255 if v == 255 else 0).getbbox()
    if bb is None:
        sys.exit("opak piksel yok")
    return bb[3] - 1, bb


def main():
    a = sys.argv[1:]
    canvas = 60
    if "--canvas" in a:
        i = a.index("--canvas"); canvas = int(a[i + 1]); del a[i:i + 2]
    i = a.index("--ref"); ref = a[i + 1]; del a[i:i + 2]
    src, dst = a
    if os.path.exists(dst):
        sys.exit(f"var, üzerine yazılmaz: {dst}")
    im = Image.open(src).convert("RGBA")
    w, h = im.size
    if w > canvas or h > canvas:
        sys.exit(f"girdi tuvalden büyük: {w}x{h}")
    src_bottom, bb = bottom_row(im)
    ref_bottom, _ = bottom_row(Image.open(ref).convert("RGBA"))
    ox = (canvas - w) // 2
    oy = ref_bottom - src_bottom
    if bb[1] + oy < 0 or bb[0] + ox < 0 or bb[2] + ox > canvas:
        sys.exit(f"hizalama sonrası taşma: ox={ox} oy={oy} bbox={bb}")
    out = Image.new("RGBA", (canvas, canvas), (0, 0, 0, 0))
    out.paste(im, (ox, oy))  # paste: alfa dahil birebir kopya, karışım yok
    out.save(dst)
    print(f"{src} -> {dst}\tofset ({ox},{oy})\talt satır {src_bottom}->{ref_bottom}")


if __name__ == "__main__":
    main()
