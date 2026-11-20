#!/usr/bin/env python3
"""Karakter renk rampaları ve palet kilidi (belge 43 §4.5, P0).

Her taban renkten 4 tonluk rampa: 0 en koyu, 1 gölge, 2 taban, 3 ışık. Formül
Assets/Scripts/Core/Customization/Layers/RampMath.cs ile birebir aynıdır; ikisi
character_ramps.json üzerinden testle karşılaştırılır. Formül değişirse ikisi birlikte değişir.

Kullanım:
  python tools/art/ramps.py build                 # appearance.json renklerinden character_ramps.json üret
  python tools/art/ramps.py check --ref hair <png>...    # baş parçası referans saç rampasında mı (KALDI: çıkış 1)
  python tools/art/ramps.py check --ref cloth <png>...   # başlık/süs referans gri rampada mı
  python tools/art/ramps.py check --ref body <png>...    # manken anahtar renklerinde mi
  python tools/art/ramps.py sheet --out X.png            # bütün rampaların önizlemesi (commit edilmez)

Kurallar (belge 43 §4.5): karakter işi (manken, baş parçası, portre parçası) teslimden önce bu denetimden geçer;
oturmayan aday kabul edilmez. Baş parçaları oyunda saç rengine bu rampalar üzerinden çevrilir.
"""
import argparse
import json
import math
import os
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
APPEARANCE = os.path.join(ROOT, "Assets", "Resources", "Content", "appearance.json")
RAMPS_JSON = os.path.join(ROOT, "Assets", "Resources", "Content", "character_ramps.json")

SIZE = 4
BASE_SHADE = 2
SHADOW_HUE, LIGHT_HUE = 250.0, 55.0
MIN_SAT_FOR_HUE_SHIFT = 0.08

# LayerTypes.cs KeyColors ile aynı. Bölge kodları LayerTypes.cs Region ile aynı.
KEYS = {
    "outline": ("#2A1E1E", 1),
    "skin": ("#D9A877", 2),
    "main": ("#D83030", 3),
    "trim": ("#3050D8", 4),
    "bottom": ("#30A848", 5),
    "shoes": ("#D8D800", 6),
    "belt": ("#28C0C0", 7),
    "eye_white": ("#F0F0F0", 8),
    "iris": ("#E040E0", 9),
    "brow": ("#7030C0", 10),
}
# Tek renkli anahtarlar (rampa değil): kontur, göz akı, iris, kaş.
SINGLE_KEYS = {"outline", "eye_white", "iris", "brow"}
REFERENCE = {"hair": "#7A4B25", "cloth": "#9A9A9A", "leather": "#5E4630"}

KEY_MIN_SEPARATION = 25.0   # iki farklı bölgenin anahtar tonları arasında en az uzaklık
CHECK_TOLERANCE = 28.0      # LayerComposer.RecolorTolerance ile aynı
CHECK_PASS = 0.85           # opak piksellerin en az bu oranı rampada (ya da konturda) olmalı


def hex_to_rgb(h):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16))


def rgb_to_hex(c):
    return "#%02X%02X%02X" % c


def to_hsv(c):
    r, g, b = c[0] / 255.0, c[1] / 255.0, c[2] / 255.0
    mx, mn = max(r, g, b), min(r, g, b)
    d = mx - mn
    v = mx
    s = 0.0 if mx <= 0.0 else d / mx
    if d <= 0.0:
        h = 0.0
    elif mx == r:
        h = 60.0 * ((g - b) / d)
    elif mx == g:
        h = 60.0 * ((b - r) / d + 2.0)
    else:
        h = 60.0 * ((r - g) / d + 4.0)
    if h < 0.0:
        h += 360.0
    return h, s, v


def channel(x):
    i = int(math.floor(x * 255.0 + 0.5))
    return 0 if i < 0 else 255 if i > 255 else i


def from_hsv(h, s, v):
    c = v * s
    hp = h / 60.0
    mod2 = hp - 2.0 * math.floor(hp / 2.0)
    x = c * (1.0 - abs(mod2 - 1.0))
    if hp < 1.0:
        r1, g1, b1 = c, x, 0.0
    elif hp < 2.0:
        r1, g1, b1 = x, c, 0.0
    elif hp < 3.0:
        r1, g1, b1 = 0.0, c, x
    elif hp < 4.0:
        r1, g1, b1 = 0.0, x, c
    elif hp < 5.0:
        r1, g1, b1 = x, 0.0, c
    else:
        r1, g1, b1 = c, 0.0, x
    m = v - c
    return (channel(r1 + m), channel(g1 + m), channel(b1 + m))


def sign(x):
    return (x > 0) - (x < 0)


def shift_hue(h, target, amount):
    diff = math.fmod(target - h + 540.0, 360.0) - 180.0
    step = sign(diff) * min(abs(diff), amount)
    return math.fmod(h + step + 360.0, 360.0)


def _shade(h, s, v, target, amount, sat_mul, val_mul, val_add):
    nh = shift_hue(h, target, amount) if s >= MIN_SAT_FOR_HUE_SHIFT else h
    ns = min(1.0, s * sat_mul)
    nv = min(1.0, v * val_mul + val_add)
    return from_hsv(nh, ns, nv)


def make_ramp(base):
    """base: (r, g, b) ya da '#RRGGBB'. 4 renk döner (koyudan açığa)."""
    if isinstance(base, str):
        base = hex_to_rgb(base)
    h, s, v = to_hsv(base)
    return [
        _shade(h, s, v, SHADOW_HUE, 14.0, 1.12, 0.52, 0.0),
        _shade(h, s, v, SHADOW_HUE, 7.0, 1.06, 0.74, 0.0),
        tuple(base),
        _shade(h, s, v, LIGHT_HUE, 8.0, 0.80, 1.10, 0.06),
    ]


def dist(c1, c2):
    rm = (c1[0] + c2[0]) / 2.0
    dr, dg, db = c1[0] - c2[0], c1[1] - c2[1], c1[2] - c2[2]
    return math.sqrt((2 + rm / 256) * dr * dr + 4 * dg * dg + (2 + (255 - rm) / 256) * db * db) / 3.0


def key_ramps():
    out = {}
    for name, (hx, _code) in KEYS.items():
        out[name] = [hex_to_rgb(hx)] if name in SINGLE_KEYS else make_ramp(hx)
    return out


def key_separation():
    """Farklı bölgelerin anahtar tonları arasındaki en küçük uzaklık ve o çift."""
    ramps = key_ramps()
    best = (1e9, None)
    names = list(ramps)
    for i, a in enumerate(names):
        for b in names[i + 1:]:
            for ca in ramps[a]:
                for cb in ramps[b]:
                    d = dist(ca, cb)
                    if d < best[0]:
                        best = (d, (a, b))
    return best


def build(_args):
    with open(APPEARANCE, encoding="utf-8") as f:
        app = json.load(f)

    def group(colors):
        return [{"base": c.upper(), "ramp": [rgb_to_hex(x) for x in make_ramp(c)]} for c in colors]

    sep, pair = key_separation()
    if sep < KEY_MIN_SEPARATION:
        sys.exit(f"anahtar renkler birbirine çok yakın: {pair} uzaklık {sep:.1f} < {KEY_MIN_SEPARATION}")
    data = {
        "_aciklama": "Karakter rampaları (belge 43 §4.5). tools/art/ramps.py build üretir; elle düzenleme. "
                     "RampMath.cs ile aynı formül, CharacterRampsTests karşılaştırır.",
        "version": 1,
        "size": SIZE,
        "baseShade": BASE_SHADE,
        "skin": group(app["skinTones"]),
        "hair": group(app["hairColors"]),
        "eye": group(app["eyeColors"]),
        "cloth": group(app["clothColors"]),
        "reference": {k: {"base": v, "ramp": [rgb_to_hex(x) for x in make_ramp(v)]} for k, v in REFERENCE.items()},
        "keys": {k: {"region": code, "colors": [rgb_to_hex(x) for x in key_ramps()[k]]} for k, (hx, code) in KEYS.items()},
        "keySeparation": round(sep, 1),
    }
    with open(RAMPS_JSON, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")
    print(f"{os.path.relpath(RAMPS_JSON, ROOT)}: ten {len(data['skin'])}, saç {len(data['hair'])}, göz {len(data['eye'])}, "
          f"kumaş {len(data['cloth'])} rampa; anahtar ayrımı {sep:.1f} ({pair[0]}/{pair[1]})")


def conformity(path, palette):
    from PIL import Image
    im = Image.open(path).convert("RGBA")
    px = [(r, g, b) for (r, g, b, a) in im.getdata() if a >= 128]
    if not px:
        return 0, 0.0
    near = sum(1 for c in px if min(dist(c, p) for p in palette) <= CHECK_TOLERANCE)
    return len(px), near / len(px)


def check(args):
    outline = [hex_to_rgb(KEYS["outline"][0])]
    if args.ref == "body":
        palette = [c for r in key_ramps().values() for c in r]
    else:
        palette = make_ramp(REFERENCE[args.ref]) + outline
    failed = False
    for p in args.files:
        n, ratio = conformity(p, palette)
        ok = n > 0 and ratio >= CHECK_PASS
        failed |= not ok
        print(f"{'GEÇTİ' if ok else 'KALDI'}  {p}: {n} piksel, rampada %{ratio * 100:.0f} (eşik %{CHECK_PASS * 100:.0f})")
    sys.exit(1 if failed else 0)


def sheet(args):
    from PIL import Image, ImageDraw
    with open(RAMPS_JSON, encoding="utf-8") as f:
        data = json.load(f)
    rows = []
    for group in ("skin", "hair", "eye", "cloth"):
        rows += [[hex_to_rgb(c) for c in r["ramp"]] for r in data[group]]
    rows += [[hex_to_rgb(c) for c in v["colors"]] for v in data["keys"].values()]
    cell = 12
    im = Image.new("RGBA", (SIZE * cell, len(rows) * cell), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for y, row in enumerate(rows):
        for x, c in enumerate(row):
            d.rectangle([x * cell, y * cell, x * cell + cell - 1, y * cell + cell - 1], fill=c + (255,))
    im.save(args.out)
    print(args.out)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)
    sub.add_parser("build")
    c = sub.add_parser("check")
    c.add_argument("--ref", choices=["hair", "cloth", "body"], required=True)
    c.add_argument("files", nargs="+")
    s = sub.add_parser("sheet")
    s.add_argument("--out", required=True)
    args = ap.parse_args()
    {"build": build, "check": check, "sheet": sheet}[args.cmd](args)


if __name__ == "__main__":
    main()
