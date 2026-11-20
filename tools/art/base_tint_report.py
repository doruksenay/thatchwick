"""Taban karakter boyamasının (Assets/Scripts/Game/BaseSpriteTint.cs) önizlemesi: setin durma karelerini örnek saç ve giysi
renkleriyle boyar, yan yana PNG yazar. Kural tablosu C# ile aynı tutulmalı.
  python tools/art/base_tint_report.py çıktı.png"""
import colorsys, glob, sys
from PIL import Image

RULES = {
    "iso_char_man_r84": {"hair": (345, 32, .38, .62, .15, .62, 0, .32, .48, .38), "outfit": (36, 62, .30, 1, .20, .62, .28, 1, .42, .45)},
    "iso_char_woman_r84": {"hair": (350, 32, .62, 1, .20, .90, 0, .60, .72, .55), "outfit": (25, 45, .45, .60, .28, .55, .30, 1, .53, .42)},
    "iso_char_elder_m_r84": {"outfit": (24, 40, .50, 1, .30, .70, .30, 1, .58, .52)},
    "iso_char_elder_f_r84": {"outfit": (100, 230, .18, 1, .20, .40, .30, 1, .27, .30)},
    "iso_char_boy_r64": {"hair": (350, 32, .40, 1, .15, .70, 0, .35, .55, .42), "outfit": (30, 45, .20, .32, .25, .78, .30, 1, .27, .55)},
    "iso_char_girl_r64": {"hair": (350, 32, .60, 1, .15, .85, 0, .40, .77, .47), "outfit": (20, 40, .60, 1, .20, .60, .40, 1, .68, .36)},
}
LOOKS = [("#1C1A19", "#2F4E7A"), ("#D9B45A", "#8C322D"), ("#A5471F", "#3E7C59"), ("#8C8C8C", "#E6DCC3")]


def has(b, h, s, v, row):
    hmin, hmax, smin, smax, vmin, vmax, rmin, rmax, _, _ = b
    if not (rmin <= row < rmax and smin <= s <= smax and vmin <= v <= vmax):
        return False
    return hmin <= h <= hmax if hmin <= hmax else (h >= hmin or h <= hmax)


def hexrgb(x):
    return tuple(int(x[i:i + 2], 16) / 255 for i in (1, 3, 5))


def tint(im, rule, hair, outfit):
    im = im.convert("RGBA")
    W, H = im.size
    px = im.load()
    ys = [y for y in range(H) for x in range(W) if px[x, y][3] > 10]
    top, bot = min(ys), max(ys)
    span = max(1, bot - top + 1)
    th = colorsys.rgb_to_hsv(*hexrgb(hair)); to = colorsys.rgb_to_hsv(*hexrgb(outfit))
    for y in range(H):
        row = (y - top) / span
        for x in range(W):
            p = px[x, y]
            if p[3] <= 10:
                continue
            h, s, v = colorsys.rgb_to_hsv(*(c / 255 for c in p[:3]))
            for name, t in (("hair", th), ("outfit", to)):
                b = rule.get(name)
                if b and has(b, h * 360, s, v, row):
                    ns = min(1, t[1] * s / b[8]); nv = min(1, t[2] * v / b[9])
                    r, g, bb = colorsys.hsv_to_rgb(t[0], ns, nv)
                    px[x, y] = (int(r * 255), int(g * 255), int(bb * 255), p[3])
                    break
    return im


def main(out):
    rows = []
    for prefix, rule in RULES.items():
        fs = sorted(glob.glob(f"Assets/Art/Iso/**/{prefix}_idle_se_00.png", recursive=True)) or sorted(glob.glob(f"Assets/Art/Iso/**/{prefix}_idle_se*.png", recursive=True))
        if not fs:
            continue
        base = Image.open(fs[0]).convert("RGBA")
        rows.append([base] + [tint(base.copy(), rule, hh, oo) for hh, oo in LOOKS])
    S = 3
    W = max(len(r) for r in rows) * 84 * S
    sheet = Image.new("RGBA", (W, len(rows) * 84 * S), (235, 230, 215, 255))
    for j, r in enumerate(rows):
        for i, im in enumerate(r):
            big = im.resize((im.size[0] * S, im.size[1] * S), Image.NEAREST)
            sheet.alpha_composite(big, (i * 84 * S, j * 84 * S + (84 - im.size[1]) * S))
    sheet.save(out)


if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else "base_tint.png")
