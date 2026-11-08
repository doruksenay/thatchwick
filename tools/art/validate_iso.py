#!/usr/bin/env python3
"""İzometrik PNG doğrulaması (boyut, saydamlık, renk sayısı, sınırlar, pivot, zemin maskesi).

Kullanım:
  python tools/art/validate_iso.py <dosya.png> [--kind ground|object]
                                   [--footprint X,Y,W,H] [--max-colors N]

--kind ground : 64x32 olmalı; tam 2:1 elmas maskesi dışında opak piksel ve maske içinde delik sayar.
--footprint   : ayak izi taban elmasının tuvaldeki kutusu (x,y,w,h). Verilirse pivotu (x, y ALTTAN) hesaplar.
Çıkış kodu 0: kritik hata yok, 1: en az bir kontrol başarısız. Işık sırası ve döşeme testi bu betikte yok;
onları işçi ayrıca ölçer ve raporlar.
"""
import argparse
import sys
from PIL import Image


def in_diamond(px, py, cx, cy, w=64, h=32):
    return abs(px + 0.5 - cx) / (w / 2) + abs(py + 0.5 - cy) / (h / 2) <= 1.0


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("--kind", choices=["ground", "object"], default="object")
    ap.add_argument("--footprint", help="x,y,w,h")
    ap.add_argument("--max-colors", type=int, default=0, help="0 = sınır yok (yalnızca bildir)")
    a = ap.parse_args()

    im = Image.open(a.file).convert("RGBA")
    w, h = im.size
    px = im.load()
    transparent = opaque = semi = 0
    colors = set()
    minx, miny, maxx, maxy = w, h, -1, -1
    for y in range(h):
        for x in range(w):
            r, g, b, al = px[x, y]
            if al == 0:
                transparent += 1
            elif al == 255:
                opaque += 1
                colors.add((r, g, b))
                minx, miny, maxx, maxy = min(minx, x), min(miny, y), max(maxx, x), max(maxy, y)
            else:
                semi += 1

    fails = []
    print(f"dosya: {a.file}")
    print(f"boyut: {w}x{h}")
    print(f"piksel: saydam {transparent}, opak {opaque}, yarı saydam {semi}")
    print(f"renk sayısı (opak): {len(colors)}")
    if opaque:
        print(f"opak sınırlar: x {minx}-{maxx}, y {miny}-{maxy}; en alt opak satır y={maxy}")
        print(f"alt boşluk: {h - 1 - maxy} satır")
    else:
        fails.append("hiç opak piksel yok")

    if semi:
        fails.append(f"yarı saydam piksel var: {semi}")
    if a.max_colors and len(colors) > a.max_colors:
        fails.append(f"renk sayısı {len(colors)} > {a.max_colors}")

    if a.kind == "ground":
        if (w, h) != (64, 32):
            fails.append(f"zemin 64x32 olmalı, bulunan {w}x{h}")
        else:
            outside = holes = 0
            for y in range(h):
                for x in range(w):
                    inside = in_diamond(x, y, 32, 16)
                    opq = px[x, y][3] == 255
                    if opq and not inside:
                        outside += 1
                    if inside and not opq:
                        holes += 1
            print(f"2:1 elmas maskesi: maske dışı opak {outside}, maske içi boş {holes}")
            if outside:
                fails.append(f"maske dışında {outside} opak piksel (yalnızca maskeyle kırp, boyama)")
            if holes:
                fails.append(f"maske içinde {holes} boş piksel")
            print("pivot (zemin): x=32, y=16 (elmas merkezi)")

    if a.footprint:
        fx, fy, fw, fh = (int(v) for v in a.footprint.split(","))
        print(f"pivot: x={fx + fw / 2:g}, y (alttan)={h - (fy + fh / 2):g}")

    if fails:
        print("SONUÇ: BAŞARISIZ")
        for f in fails:
            print(" -", f)
        sys.exit(1)
    print("SONUÇ: geçti")


if __name__ == "__main__":
    main()
