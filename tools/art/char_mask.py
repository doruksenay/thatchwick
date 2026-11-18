"""016: karakter SE karesinden saç/tunik bölgesi için HLS maskesi üretir (PixelLab'e dokunmaz).

Kullanım: python tools/art/char_mask.py <png> <out_mask.png>
"""
import colorsys
import sys

from PIL import Image


def main(src, out):
    im = Image.open(src).convert("RGBA")
    w, h = im.size
    px = im.load()
    ys = [y for y in range(h) for x in range(w) if px[x, y][3] > 0]
    top = min(ys)
    mask = Image.new("RGB", (w, h), (0, 0, 0))
    mp = mask.load()
    # kafa: en üstten ~ 11 satır; saç = koyu/kahverengi, düşük parlaklık
    for y in range(top, top + 11):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            hh, l, s = colorsys.rgb_to_hls(r / 255, g / 255, b / 255)
            if l < 0.30:
                mp[x, y] = (255, 255, 255)
    print("top", top, "white", sum(1 for y in range(h) for x in range(w) if mp[x, y][0] == 255))
    mask.save(out)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
