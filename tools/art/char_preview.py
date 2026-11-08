#!/usr/bin/env python3
"""Karakter adayı ölçümü ve önizlemesi (gen yok).

Kullanım:
  python tools/art/char_preview.py measure <dosya.png> [...]
  python tools/art/char_preview.py sheet <cikti.png> --top a.png b.png c.png --bottom d1.png ... [--scale 4]
  python tools/art/char_preview.py row <cikti.png> a.png b.png ... [--scale 4]

Ölçüm (010 yöntemi):
  boy = opak bbox yüksekliği, genişlik = opak bbox genişliği
  pivot y (ALTTAN) = tuval yüksekliği - 1 - en alt opak satır
  pivot x = en alt opak satırdan yukarı, boyun %10'u kadar satırlık bantta opak sütunların ortası
Önizleme: #333333 zemin, her sprite kendi 64x32 elmas konturunun (1 px açık gri) üzerinde,
pivot elmas merkezine oturur, sprite konturun önünde çizilir; en yakın komşu büyütme.
"""
import sys
from PIL import Image

BG = (0x33, 0x33, 0x33, 255)
LINE = (0xB0, 0xB0, 0xB0, 255)
DW, DH = 64, 32


def measure(path):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    px = im.load()
    cols, semi = set(), 0
    minx, miny, maxx, maxy = w, h, -1, -1
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 255:
                cols.add((r, g, b))
                minx, miny, maxx, maxy = min(minx, x), min(miny, y), max(maxx, x), max(maxy, y)
            elif a:
                semi += 1
    bh, bw = maxy - miny + 1, maxx - minx + 1
    band = max(1, round(bh * 0.10))
    xs = [x for y in range(maxy - band + 1, maxy + 1) for x in range(w) if px[x, y][3] == 255]
    pvx = (min(xs) + max(xs) + 1) / 2
    return dict(file=path, size=(w, h), height=bh, width=bw, colors=len(cols), semi=semi,
                pivot_x=pvx, pivot_y=h - 1 - maxy, maxy=maxy, band=band)


def diamond(img, ox, oy):
    px = img.load()
    for x in range(DW):
        d = abs(x + 0.5 - DW / 2)
        half = DH / 2 - d / 2
        for y in (int(DH / 2 - half), int(DH / 2 + half) - 1):
            y = min(max(y, 0), DH - 1)
            px[ox + x, oy + y] = LINE


def compose(rows, scale):
    """rows: liste[liste[dosya]]; her hücre bir elmas + pivotu merkezde sprite."""
    ms = [[(f, measure(f)) for f in r] for r in rows]
    above = max(m["maxy"] for r in ms for _, m in r) + 2 + DH // 2  # merkezden yukarı
    cellw = max(max(DW, m["size"][0]) for r in ms for _, m in r) + 8
    cellh = above + DH // 2 + 4
    ncol = max(len(r) for r in rows)
    out = Image.new("RGBA", (cellw * ncol, cellh * len(rows)), BG)
    for ri, r in enumerate(ms):
        for ci, (f, m) in enumerate(r):
            cx, cy = ci * cellw + cellw // 2, ri * cellh + above
            diamond(out, cx - DW // 2, cy - DH // 2)
            sp = Image.open(f).convert("RGBA")
            ox = cx - int(round(m["pivot_x"]))
            oy = cy - m["maxy"]
            out.alpha_composite(sp, (ox, oy))
    return out.resize((out.width * scale, out.height * scale), Image.NEAREST)


def main():
    a = sys.argv[1:]
    scale = 4
    if "--scale" in a:
        i = a.index("--scale"); scale = int(a[i + 1]); del a[i:i + 2]
    cmd = a[0]
    if cmd == "measure":
        for f in a[1:]:
            m = measure(f)
            print(f"{f}\t{m['size'][0]}x{m['size'][1]}\tboy {m['height']}\tgen {m['width']}\trenk {m['colors']}"
                  f"\tyari {m['semi']}\tpivot ({m['pivot_x']}, {m['pivot_y']} alttan)")
    elif cmd == "sheet":
        out = a[1]
        ti, bi = a.index("--top"), a.index("--bottom")
        top, bottom = a[ti + 1:bi], a[bi + 1:]
        compose([top, bottom], scale).save(out)
    elif cmd == "row":
        compose([a[2:]], scale).save(a[1])
    else:
        sys.exit("bilinmeyen komut")


if __name__ == "__main__":
    main()
