"""Duvar malzemesi: 96x96 inpaint çıktısını maskeyle 102x115 kaynağa geri koyar.
Kullanım: wall_compose.py <kaynak_tam.png> <cikti_kirpim.png> <maske.png> <ox> <oy> <hedef.png>
Ek kullanım (toplu): wall_compose.py --batch  (art/tmp325 yapısı için)
"""
import os
import sys

from PIL import Image


def compose(src, crop, mask, ox, oy, dest):
    s = Image.open(src).convert("RGBA")
    c = Image.open(crop).convert("RGBA")
    m = Image.open(mask).convert("L")
    out = s.copy()
    cp, mp, op = c.load(), m.load(), out.load()
    for y in range(96):
        for x in range(96):
            if mp[x, y] > 127:
                op[x + ox, y + oy] = cp[x, y]
    out.save(dest)


if __name__ == "__main__":
    if sys.argv[1] == "--batch":
        t = "art/tmp325/"
        os.makedirs(t + "out", exist_ok=True)
        for p in ("plank", "stone"):
            for n, o in (("sw_ne", 0), ("se_nw", 6)):
                compose("Assets/Art/Iso/iso_wall_wall_%s.png" % n,
                        t + "o_%s_%s.png" % (p, n), t + "mask_%s.png" % n, o, 19,
                        t + "out/iso_wall_wall_%s_%s.png" % (p, n))
    else:
        a = sys.argv[1:7]
        compose(a[0], a[1], a[2], int(a[3]), int(a[4]), a[5])
