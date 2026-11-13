#!/usr/bin/env python3
"""Stil kilidi araçları: ana palet, palet uyumu denetimi ve referanslarla yan yana karşılaştırma.

Tutarlılığın üç ayağı (art/style/README.md):
  1. Referanslar  art/style/references.json  (aile başına kabul edilmiş görseller; PixelLab stil eşlemesi bunları kullanır)
  2. Ana palet    art/style/palette.json     (referanslardan çıkarılır; yeni görsel bu palete yakın olmalı)
  3. Yan yana     art/results/NNN-*-yanyana.png (yeni çıktı, ailesinin referanslarıyla aynı ölçekte)

Kullanım:
  python tools/art/style.py palette [--png]              # referanslardan art/style/palette.json üret (--png: önizleme)
  python tools/art/style.py check <png>... [--family F]  # palet uyumu; eşik altıysa çıkış 1
  python tools/art/style.py sheet --family F --out X.png <png>...   # referanslar + yeni görseller, 4x, aynı taban çizgisi
"""
import argparse
import json
import math
import os
import sys

from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
REFS = os.path.join(ROOT, "art", "style", "references.json")
PALETTE_JSON = os.path.join(ROOT, "art", "style", "palette.json")
PALETTE_PNG = os.path.join(ROOT, "art", "style", "palette.png")

PALETTE_SIZE = 64          # ana paletteki renk sayısı
NEAR = 20.0                # bir pikselin palete "yakın" sayıldığı en büyük uzaklık (algısal ağırlıklı RGB)
# Eşikler 2026-11-13'te ölçüldü: referanslar ortalama 1-6, aynı aileden kabul görmüş adaylar 4-12,
# doygunluğu 1,8-2,2 kat artırılmış ya da tonu kaydırılmış kopyalar 11-12 uzaklık ve 1,4-1,9 kat doygunluk.
# Bu denetim kaba sapmayı yakalar; ince stil farkı insan gözüyle (yan yana sayfası) ve pilot kabulüyle çözülür.
MEAN_WARN, MEAN_FAIL = 10.0, 16.0          # palete ortalama uzaklık
NEAR_FAIL = 0.80                            # palete yakın piksel oranı bunun altındaysa kalır
SAT_WARN, SAT_FAIL = 1.25, 1.50            # ortalama doygunluk / aile referanslarının ortalaması
SAT_LOW_WARN = 0.60


def load_refs():
    with open(REFS, encoding="utf-8") as f:
        return json.load(f)["aileler"]


def ref_files(families=None, only_accepted=False):
    out = []
    for name, fam in load_refs().items():
        if families and name not in families:
            continue
        for r in fam.get("referanslar", []):
            if only_accepted and r.get("durum") != "kabul":
                continue
            p = os.path.join(ROOT, r["dosya"])
            if os.path.exists(p):
                out.append(p)
    return out


def opaque_pixels(path):
    im = Image.open(path).convert("RGBA")
    return [(r, g, b) for (r, g, b, a) in (im.get_flattened_data() if hasattr(im, "get_flattened_data") else im.getdata()) if a >= 128]


def dist(c1, c2):
    # "redmean" yaklaşımı: düz RGB'den algıya daha yakın, bağımlılık gerektirmez
    rm = (c1[0] + c2[0]) / 2.0
    dr, dg, db = c1[0] - c2[0], c1[1] - c2[1], c1[2] - c2[2]
    return math.sqrt((2 + rm / 256) * dr * dr + 4 * dg * dg + (2 + (255 - rm) / 256) * db * db) / 3.0


def build_palette(_args):
    files = ref_files()
    if not files:
        sys.exit("Referans bulunamadı (art/style/references.json).")
    pixels = []
    for p in files:
        pixels.extend(opaque_pixels(p))
    side = int(math.ceil(math.sqrt(len(pixels))))
    canvas = Image.new("RGB", (side, side), pixels[0])
    canvas.putdata(pixels + [pixels[-1]] * (side * side - len(pixels)))
    q = canvas.quantize(colors=PALETTE_SIZE, method=Image.Quantize.MEDIANCUT)
    pal = q.getpalette()[: PALETTE_SIZE * 3]
    used = sorted(set(q.get_flattened_data() if hasattr(q, "get_flattened_data") else q.getdata()))
    colors = sorted({tuple(pal[i * 3: i * 3 + 3]) for i in used}, key=lambda c: (0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2]))
    data = {
        "_aciklama": "art/style/references.json'daki görsellerden üretildi (tools/art/style.py palette). Elle düzenleme; referans değişince yeniden üret.",
        "kaynak": [os.path.relpath(p, ROOT).replace("\\", "/") for p in files],
        "yakinlik_esigi": NEAR,
        "renkler": ["#%02x%02x%02x" % c for c in colors],
    }
    with open(PALETTE_JSON, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    sw = Image.new("RGB", (16 * 8, 16 * int(math.ceil(len(colors) / 8))), (51, 51, 51))
    d = ImageDraw.Draw(sw)
    for i, c in enumerate(colors):
        x, y = (i % 8) * 16, (i // 8) * 16
        d.rectangle([x, y, x + 15, y + 15], fill=c)
    if getattr(_args, "png", False):
        sw.save(PALETTE_PNG)   # önizleme; repoya commit edilmez (.gitignore)
    print(f"{len(colors)} renk, {len(files)} referans -> {os.path.relpath(PALETTE_JSON, ROOT)}")


def load_palette():
    with open(PALETTE_JSON, encoding="utf-8") as f:
        d = json.load(f)
    return [tuple(int(h[i: i + 2], 16) for i in (1, 3, 5)) for h in d["renkler"]]


def conformity(path, palette, cache):
    px = opaque_pixels(path)
    if not px:
        return 0.0, 0.0
    near, total_d = 0, 0.0
    for c in px:
        if c not in cache:
            cache[c] = min(dist(c, p) for p in palette)
        d = cache[c]
        total_d += d
        if d <= NEAR:
            near += 1
    return near / len(px), total_d / len(px)


def mean_saturation(path):
    import colorsys
    px = opaque_pixels(path)
    if not px:
        return 0.0
    return sum(colorsys.rgb_to_hls(r / 255, g / 255, b / 255)[2] for r, g, b in px) / len(px)


def check(args):
    palette = load_palette()
    refs = ref_files([args.family]) if args.family else []
    ref_sat = sum(mean_saturation(r) for r in refs) / len(refs) if refs else None
    cache, failed = {}, False
    print("| Dosya | Palete yakın | Ortalama uzaklık | Doygunluk / aile | Sonuç | Bulgu |")
    print("|---|---|---|---|---|---|")
    for p in args.files:
        ratio, mean = conformity(p, palette, cache)
        sat = mean_saturation(p) / ref_sat if ref_sat else None
        notes, level = [], 0
        if mean > MEAN_FAIL or ratio < NEAR_FAIL:
            notes.append("palet dışı renkler"); level = 2
        elif mean > MEAN_WARN:
            notes.append("palete uzak"); level = max(level, 1)
        if sat is not None:
            if sat > SAT_FAIL:
                notes.append("ailesinden çok doygun"); level = 2
            elif sat > SAT_WARN:
                notes.append("ailesinden doygun"); level = max(level, 1)
            elif sat < SAT_LOW_WARN:
                notes.append("ailesinden soluk"); level = max(level, 1)
        failed = failed or level == 2
        res = ("GEÇTİ", "UYARI", "KALDI")[level]
        sat_txt = f"x{sat:.2f}" if sat is not None else "-"
        print(f"| {os.path.basename(p)} | %{ratio * 100:.0f} | {mean:.1f} | {sat_txt} | {res} | {', '.join(notes)} |")
    sys.exit(1 if failed else 0)


def sheet(args):
    refs = ref_files([args.family]) if args.family else []
    imgs = [Image.open(p).convert("RGBA") for p in refs + args.files]
    if not imgs:
        sys.exit("Görsel yok.")
    scale, gap, label_h = 4, 8, 14
    w = sum(im.width * scale for im in imgs) + gap * (len(imgs) + 1) + 24
    h = max(im.height for im in imgs) * scale + gap * 2 + label_h
    out = Image.new("RGBA", (w, h), (51, 51, 51, 255))
    d = ImageDraw.Draw(out)
    x = gap
    for i, im in enumerate(imgs):
        if i == len(refs) and refs:
            d.line([(x + 4, 0), (x + 4, h)], fill=(200, 200, 200, 255), width=2)   # referans | yeni
            x += 24
        big = im.resize((im.width * scale, im.height * scale), Image.NEAREST)
        y = h - gap - big.height                       # alt kenarlar aynı çizgide
        out.alpha_composite(big, (x, y))
        d.text((x, 2), ("R " if i < len(refs) else "Y ") + os.path.basename((refs + args.files)[i])[:28], fill=(230, 230, 230, 255))
        x += big.width + gap
    out.save(args.out)
    print(f"{len(refs)} referans + {len(args.files)} yeni -> {args.out}")


def main():
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)
    pp = sub.add_parser("palette")
    pp.add_argument("--png", action="store_true", help="art/style/palette.png önizlemesini de yaz")
    pp.set_defaults(fn=build_palette)
    c = sub.add_parser("check")
    c.add_argument("files", nargs="+")
    c.add_argument("--family", default="")
    c.set_defaults(fn=check)
    s = sub.add_parser("sheet")
    s.add_argument("files", nargs="*")
    s.add_argument("--family", default="")
    s.add_argument("--out", required=True)
    s.set_defaults(fn=sheet)
    a = ap.parse_args()
    a.fn(a)


if __name__ == "__main__":
    main()
