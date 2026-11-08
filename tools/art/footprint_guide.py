#!/usr/bin/env python3
"""Ayak izi kılavuzu ve maskesi üretir (create_map_object stil eşleme modu için).

Tur 3'te çalışan yöntem: kılavuz = saydam tuvalde düz renkli taban elmas(lar)ı,
maske = taban elmasının eşya yüksekliği kadar yukarı uzatılmış, 1 px genişletilmiş hali.
Kenar boşlukları: sol 8 px, alt 8 px (yatak 112x84 ve ocak 80x56 ile aynı).

Kullanım:
  python tools/art/footprint_guide.py --w 112 --h 84 --n 2 --axis x --height 22 --out <klasör>/bed
  -> bed_guide.png, bed_mask.png, bed_info.txt (ayak izi sınırları ve pivot)

--axis x : uzun eksen SE-NW (adım +32,+16). --axis y : uzun eksen SW-NE (adım -32,+16).
"""
import argparse
from PIL import Image, ImageFilter

TILE_W, TILE_H = 64, 32
GUIDE_COLOR = (0x78, 0x60, 0x46, 255)


def in_diamond(px, py, cx, cy):
    return abs(px + 0.5 - cx) / (TILE_W / 2) + abs(py + 0.5 - cy) / (TILE_H / 2) <= 1.0


def centers(n, axis, w, h, margin=8):
    step = (32, 16) if axis == "x" else (-32, 16)
    raw = [(i * step[0], i * step[1]) for i in range(n)]
    left = min(cx - 32 for cx, _ in raw)
    bottom = max(cy + 16 for _, cy in raw)
    dx = margin - left
    dy = (h - margin) - bottom
    return [(cx + dx, cy + dy) for cx, cy in raw]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--w", type=int, required=True)
    ap.add_argument("--h", type=int, required=True)
    ap.add_argument("--n", type=int, default=1, help="uzun eksendeki karo sayısı")
    ap.add_argument("--axis", choices=["x", "y"], default="x")
    ap.add_argument("--height", type=int, required=True, help="eşya yüksekliği (px)")
    ap.add_argument("--out", required=True, help="çıktı öneki (klasör/ad)")
    a = ap.parse_args()

    cs = centers(a.n, a.axis, a.w, a.h)
    guide = Image.new("RGBA", (a.w, a.h), (0, 0, 0, 0))
    mask = Image.new("L", (a.w, a.h), 0)
    gp, mp = guide.load(), mask.load()
    for y in range(a.h):
        for x in range(a.w):
            if any(in_diamond(x, y, cx, cy) for cx, cy in cs):
                gp[x, y] = GUIDE_COLOR
            if any(in_diamond(x, y + t, cx, cy) for cx, cy in cs for t in range(a.height + 1)):
                mp[x, y] = 255
    mask = mask.filter(ImageFilter.MaxFilter(3))

    xs0 = min(cx - 32 for cx, _ in cs)
    xs1 = max(cx + 32 for cx, _ in cs)
    ys0 = min(cy - 16 for _, cy in cs)
    ys1 = max(cy + 16 for _, cy in cs)
    pivot_x = (xs0 + xs1) / 2
    pivot_y_bottom = a.h - (ys0 + ys1) / 2

    guide.save(a.out + "_guide.png")
    mask.save(a.out + "_mask.png")
    info = (
        f"canvas {a.w}x{a.h}, n={a.n}, axis={a.axis}, height={a.height}\n"
        f"footprint x {xs0}-{xs1 - 1}, y {ys0}-{ys1 - 1}\n"
        f"pivot x={pivot_x:g}, pivot y (alttan)={pivot_y_bottom:g}\n"
    )
    with open(a.out + "_info.txt", "w", encoding="utf-8") as f:
        f.write(info)
    print(info, end="")


if __name__ == "__main__":
    main()
