#!/usr/bin/env python3
"""Duvar eşyası kılavuzu (sw_ne, "/" yüz): tuvalde 2:1 eğimli paralelkenar (duvar yüzü) + eşya maskesi.
Kullanım: python tools/art/wall_guide.py --w 48 --h 64 --height 40 --out art/tmp306/torch
Çıktı: <out>_guide.png, <out>_mask.png
"""
import argparse
from PIL import Image, ImageFilter

COLOR = (0xB8, 0xA0, 0x7A, 255)
ap = argparse.ArgumentParser()
ap.add_argument("--w", type=int, required=True)
ap.add_argument("--h", type=int, required=True)
ap.add_argument("--height", type=int, required=True)
ap.add_argument("--out", required=True)
a = ap.parse_args()

fw = min(32, a.w - 8)
x0 = (a.w - fw) // 2
fh = a.height + 14


def ftop(x):
    return a.h - 6 - fh - (x - x0) // 2  # sağa doğru yükselir


g = Image.new("RGBA", (a.w, a.h), (0, 0, 0, 0))
m = Image.new("L", (a.w, a.h), 0)
gp, mp = g.load(), m.load()
for y in range(a.h):
    for x in range(a.w):
        if not (x0 <= x < x0 + fw):
            continue
        t = ftop(x)
        if t <= y < t + fh:
            gp[x, y] = COLOR
            if x0 + 4 <= x < x0 + fw - 4 and t + 3 <= y < t + 3 + a.height:
                mp[x, y] = 255
m = m.filter(ImageFilter.MaxFilter(3))
g.save(a.out + "_guide.png")
m.save(a.out + "_mask.png")
print(a.out, "ok")
