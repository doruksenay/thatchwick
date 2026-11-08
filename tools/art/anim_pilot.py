#!/usr/bin/env python3
"""Yürüme animasyonu pilotu (013): kare işleme, ölçüm, karşılaştırma görseli ve GIF (gen yok).

Kullanım:
  uv run --with pillow python tools/art/anim_pilot.py ingest <yontem> <kaynak_klasor> [--stem iso_char_farmer_a2_56_walk] [--dir se]
      kaynak_klasordeki frame_NNN.png dosyalarını _ham adlarıyla kopyalar (üzerine yazmaz).
  uv run --with pillow python tools/art/anim_pilot.py process <yontem> [<yontem> ...]
      işlenmiş kareleri yazar (üzerine yazmaz), ölçüm tablosunu basar,
      art/results/013-yurume-pilot-karsilastirma.png ve 013-yurume-<yontem>-x4.gif üretir.

İşleme (iş 013 tarifi):
  1. 60x60 tuval, ölçekleme yok. Yatay: yöntemin tüm karelerinin birleşik opak kutusunun ortası,
     durma karesinin opak kutu ortasına gelir (yöntem başına tek tamsayı ofset; kareler arası
     göreli hareket korunur).
  2. Dikey: her karenin en alt opak satırı durma karesinin en alt opak satırına (tamsayı kaydırma).
  3. Palet: iso_char_farmer_a2_56_cand_*.png opak renklerinin birleşimi (<=32), en yakın RGB.
     Alfa ikili: a>=128 -> 255, aksi 0.
"""
import glob
import os
import shutil
import sys
from PIL import Image

ISO = "Assets/Art/Iso"
ADAY = f"{ISO}/_aday"
ANIM = f"{ADAY}/anim"
HAM = f"{ANIM}/_ham"
STAND = f"{ADAY}/iso_char_farmer_a2_56_cand_se.png"
VILLAGER = f"{ISO}/iso_char_villager_se.png"
STEM = "iso_char_farmer_a2_56_walk"
DIR = "se"
C = 60

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import char_preview  # noqa: E402


def load(p):
    return Image.open(p).convert("RGBA")


def opaque_mask(im, thr=255):
    a = im.split()[3]
    return a.point(lambda v: 255 if v >= thr else 0)


def bbox(im, thr=255):
    return opaque_mask(im, thr).getbbox()


def palette():
    cols = set()
    for f in sorted(glob.glob(f"{ADAY}/iso_char_farmer_a2_56_cand_*.png")):
        im = load(f)
        for r, g, b, a in im.getdata():
            if a == 255:
                cols.add((r, g, b))
    cols = sorted(cols)
    if len(cols) > 32:
        sys.exit(f"palet {len(cols)} renk > 32")
    return cols


def nearest(c, pal, cache):
    if c not in cache:
        cache[c] = min(pal, key=lambda p: (p[0] - c[0]) ** 2 + (p[1] - c[1]) ** 2 + (p[2] - c[2]) ** 2)
    return cache[c]


def raw_files(m):
    return sorted(glob.glob(f"{HAM}/{STEM}_{m}_{DIR}_[0-9][0-9].png"))


def ingest(m, src):
    os.makedirs(HAM, exist_ok=True)
    frames = sorted(glob.glob(os.path.join(src, "frame_*.png")))
    if not frames:
        sys.exit("kare yok")
    for i, f in enumerate(frames):
        dst = f"{HAM}/{STEM}_{m}_{DIR}_{i:02d}.png"
        if os.path.exists(dst):
            sys.exit(f"var: {dst}")
        shutil.copyfile(f, dst)
        print(dst, load(dst).size)


def iou(a, b):
    pa = set(i for i, v in enumerate(a.getdata()) if v)
    pb = set(i for i, v in enumerate(b.getdata()) if v)
    u = len(pa | pb)
    return len(pa & pb) / u if u else 1.0


def same_canvas_masks(ims):
    """Kareleri ortak tuvale (en büyük w,h) sol-üst hizalı koyup opak maske döndürür."""
    w = max(i.width for i in ims); h = max(i.height for i in ims)
    out = []
    for i in ims:
        c = Image.new("L", (w, h), 0)
        c.paste(opaque_mask(i, 128), (0, 0))
        out.append(c)
    return out


def leg_stats(im, palset):
    bb = bbox(im, 128)
    x0, y0, x1, y1 = bb
    top = y1 - round((y1 - y0) * 0.35)
    px = im.load()
    cols, n, off = set(), 0, 0
    for y in range(top, y1):
        for x in range(x0, x1):
            r, g, b, a = px[x, y]
            if a >= 128:
                n += 1
                cols.add((r, g, b))
                if (r, g, b) not in palset:
                    off += 1
    return len(cols), (off / n if n else 0.0)


def stats(ims, palset=None, legs=False):
    hs, feet, cols_all, semi, perframe_cols = [], [], set(), 0, []
    for im in ims:
        bb = bbox(im, 128)
        hs.append(bb[3] - bb[1])
        feet.append(bb[3] - 1)
        fc = set()
        for r, g, b, a in im.getdata():
            if 0 < a < 255:
                semi += 1
            if a >= 128:
                fc.add((r, g, b))
        cols_all |= fc
        perframe_cols.append(len(fc))
    masks = same_canvas_masks(ims)
    ious = [iou(masks[i], masks[i + 1]) for i in range(len(masks) - 1)]
    close = iou(masks[-1], masks[0])
    d = dict(n=len(ims), heights=hs, feet=feet, ious=ious, close=close,
             colors=len(cols_all), colors_frame_max=max(perframe_cols), semi=semi,
             sizes=sorted(set(i.size for i in ims)))
    if legs:
        ls = [leg_stats(im, palset) for im in ims]
        d["leg_colors"] = [c for c, _ in ls]
        d["leg_off"] = [o for _, o in ls]
    return d


def process(methods):
    pal = palette()
    palset = set(pal)
    stand = load(STAND)
    sbb = bbox(stand)
    s_cx = (sbb[0] + sbb[2]) / 2
    s_foot = sbb[3] - 1
    print(f"palet {len(pal)} renk; durma karesi bbox {sbb} boy {sbb[3]-sbb[1]} ayak satırı {s_foot}")
    report = {}
    for m in methods:
        files = raw_files(m)
        raws = [load(f) for f in files]
        # birleşik kutu (ham tuval koordinatı; tüm kareler aynı tuvalde varsayılır)
        bbs = [bbox(r, 128) for r in raws]
        ux0 = min(b[0] for b in bbs); ux1 = max(b[2] for b in bbs)
        dx = int(round(s_cx - (ux0 + ux1) / 2))
        procs, clipped = [], 0
        cache = {}
        for f, r, bb in zip(files, raws, bbs):
            dy = s_foot - (bb[3] - 1)
            out = Image.new("RGBA", (C, C), (0, 0, 0, 0))
            src = r.load(); dst = out.load()
            for y in range(r.height):
                for x in range(r.width):
                    pr, pg, pb, pa = src[x, y]
                    if pa < 128:
                        continue
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < C and 0 <= ny < C:
                        q = nearest((pr, pg, pb), pal, cache)
                        dst[nx, ny] = (q[0], q[1], q[2], 255)
                    else:
                        clipped += 1
            dstp = f"{ANIM}/{os.path.basename(f)}"
            if os.path.exists(dstp):
                sys.exit(f"var: {dstp}")
            out.save(dstp)
            procs.append(out)
        report[m] = dict(raw=stats(raws, palset, legs=True), proc=stats(procs, palset, legs=True),
                         dx=dx, clipped=clipped)
    for m, r in report.items():
        R, P = r["raw"], r["proc"]
        f2 = lambda xs: ", ".join(f"{v:.2f}" for v in xs)
        print(f"\n== {m}  ham tuval {R['sizes']}  dx {r['dx']}  tuval dışı kalan piksel {r['clipped']}")
        print(f" kare {R['n']}")
        print(f" boy ham {R['heights']} aralık {min(R['heights'])}-{max(R['heights'])}")
        print(f" boy işlenmiş {P['heights']} aralık {min(P['heights'])}-{max(P['heights'])}")
        print(f" ayak satırı ham {R['feet']} aralık {min(R['feet'])}-{max(R['feet'])}")
        print(f" IoU ham ardışık [{f2(R['ious'])}] min {min(R['ious']):.2f} ort {sum(R['ious'])/len(R['ious']):.2f} kapanış {R['close']:.2f}")
        print(f" IoU işl ardışık [{f2(P['ious'])}] min {min(P['ious']):.2f} ort {sum(P['ious'])/len(P['ious']):.2f} kapanış {P['close']:.2f}")
        print(f" bacak renk ham {R['leg_colors']} palet dışı oran ham [{f2(R['leg_off'])}] ort {sum(R['leg_off'])/len(R['leg_off']):.2f}")
        print(f" bacak renk işl {P['leg_colors']}")
        print(f" renk ham {R['colors']} (kare max {R['colors_frame_max']}) -> işl {P['colors']} (kare max {P['colors_frame_max']})")
        print(f" yarı saydam ham {R['semi']} işl {P['semi']}")
    return report


def visuals(methods):
    proc = {m: sorted(glob.glob(f"{ANIM}/{STEM}_{m}_{DIR}_[0-9][0-9].png")) for m in methods}
    out = "art/results/013-yurume-pilot-karsilastirma.png"
    if os.path.exists(out):
        sys.exit(f"var: {out}")
    rows = [[VILLAGER, STAND] + proc[m] for m in methods]
    char_preview.compose(rows, 4).save(out)
    print(out)
    for m in methods:
        g = f"art/results/013-yurume-{m}-x4.gif"
        if os.path.exists(g):
            sys.exit(f"var: {g}")
        frames = [char_preview.compose([[VILLAGER, STAND, f]], 4).convert("RGB") for f in proc[m]]
        pf = [fr.quantize(colors=256, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE) for fr in frames]
        pf[0].save(g, save_all=True, append_images=pf[1:], duration=125, loop=0, disposal=1, optimize=False)
        print(g, len(pf))


if __name__ == "__main__":
    a = sys.argv[1:]
    if a[0] == "ingest":
        ingest(a[1], a[2])
    elif a[0] == "process":
        process(a[1:])
        visuals(a[1:])
    else:
        sys.exit("bilinmeyen komut")
