#!/usr/bin/env python3
"""Manken araçları: anahtar renkli PixelLab karelerinden bölge haritası, baş çapası ve betikle beden (belge 43 §4.3).

Akış (P1, P1b, P2 pilotları):
  1. PixelLab mankeni anahtar renklerde üretir (ramps.py KEYS): kaynak kareler lay_src_<gövde>_<anim>_<yön>_<NN>.png
  2. regions: her kare bölge haritasına çevrilir -> lay_body_<gövde>_<anim>_<yön>_<NN>.png
     (R = bölge kodu, G = ton 0-3, A = örtü; LayerTypes.cs Region ile aynı kodlar).
     Çapalar görselin içinde: B = 1 baş çapası (kel kafanın tepesi), B = 2 göz merkezleri, diğerleri 0.
     Ayrı çapa dosyası yok; oyun (RegionFrame.Decode) aynı pikselleri okur.
  3. anchors: bölge haritalarının çapalarını raporlar (denetim için; --out ile JSON)
  4. build: Orta mankenden İnce, Gürbüz, Dolgun bölge haritaları (B1, betikle beden; P1b pilotu)
  5. extract: mankenin karesi + PixelLab'in saç/sakal/başlık eklediği hâli -> baş parçası (64x96, çapa 32,24)

Kullanım:
  python tools/art/manken.py regions <lay_src_*.png>... --out-dir Assets/Art/Iso/katman
  python tools/art/manken.py anchors <lay_body_*.png>... [--out art/results/NNN-capalar.json]
  python tools/art/manken.py build <lay_body_m_orta_*.png>... --build dolgun --out-dir Assets/Art/Iso/katman
  python tools/art/manken.py extract --base <lay_body_*.png> --edited <inpaint.png> --out-dir D --name lay_mass_short_se
  python tools/art/manken.py preview <lay_body_*.png> --out X.png   # bölge haritasını okunur renklerle gösterir

Kural: var olan dosyanın üzerine yazılmaz (--force verilmedikçe). regions, eşleşmeyen piksel oranı %3'ü aşarsa
o kareyi KALDI sayar (çıkış 1): manken anahtar renklere oturmamış demektir, yeniden üretilmeli.
"""
import argparse
import json
import os
import sys

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import ramps  # noqa: E402

UNKNOWN_FAIL = 0.03
MATCH_TOLERANCE = 30.0
BUILDS = ("ince", "orta", "gurbuz", "dolgun")
REGION = {name: code for name, (_hx, code) in ramps.KEYS.items()}
TORSO = {REGION["main"], REGION["belt"], REGION["trim"]}
FACE = {REGION["eye_white"], REGION["iris"]}


# ---------------- bölge haritası ----------------

def key_table():
    """(renk, bölge kodu, ton) listesi; tek renkli anahtarların tonu taban (2)."""
    table = []
    for name, cols in ramps.key_ramps().items():
        code = REGION[name]
        if len(cols) == 1:
            table.append((cols[0], code, ramps.BASE_SHADE))
        else:
            for shade, c in enumerate(cols):
                table.append((c, code, shade))
    return table


def to_regions(img):
    """RGBA kaynak -> (bölge haritası görseli, eşleşmeyen opak piksel oranı)."""
    src = img.convert("RGBA")
    w, h = src.size
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    table = key_table()
    cache = {}
    opaque = unknown = 0
    sp, op = src.load(), out.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = sp[x, y]
            if a < 128:
                continue
            opaque += 1
            c = (r, g, b)
            if c not in cache:
                best = min(table, key=lambda t: ramps.dist(t[0], c))
                cache[c] = (best[1], best[2], ramps.dist(best[0], c))
            code, shade, d = cache[c]
            if d > MATCH_TOLERANCE:
                unknown += 1
            op[x, y] = (code, shade, 0, 255)
    mark_anchors(out)
    return out, (unknown / opaque if opaque else 1.0)


def mark_anchors(im):
    """Bölge haritasına çapaları yazar: B = 1 baş, B = 2 gözler. Çapa her zaman opak bir piksele oturur."""
    w, h = im.size
    px = im.load()
    grid = [[(px[x, y][0], px[x, y][1]) if px[x, y][3] >= 128 else (0, 0) for x in range(w)] for y in range(h)]
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if b:
                px[x, y] = (r, g, 0, a)
    a = anchor_of(grid, w, h)
    if a is None:
        return None
    r, g, _b, al = px[a["hx"], a["hy"]]
    px[a["hx"], a["hy"]] = (r, g, 1, al)
    e = a["eyes"]
    for i in range(0, len(e), 2):
        r, g, _b, al = px[e[i], e[i + 1]]
        px[e[i], e[i + 1]] = (r, g, 2, al)
    return a


def body_name(src_path):
    stem = os.path.splitext(os.path.basename(src_path))[0]
    return ("lay_body_" + stem[len("lay_src_"):] if stem.startswith("lay_src_") else "lay_body_" + stem) + ".png"


def save_new(img, path, force):
    if os.path.exists(path) and not force:
        sys.exit(f"hedef zaten var, üzerine yazılmaz: {path}")
    img.save(path)


def cmd_regions(args):
    os.makedirs(args.out_dir, exist_ok=True)
    failed = False
    for p in args.files:
        out, unk = to_regions(Image.open(p))
        ok = unk <= UNKNOWN_FAIL
        failed |= not ok
        dst = os.path.join(args.out_dir, body_name(p))
        if ok or args.keep_failed:
            save_new(out, dst, args.force)
        print(f"{'GEÇTİ' if ok else 'KALDI'}  {p} -> {dst}: eşleşmeyen %{unk * 100:.1f}")
    sys.exit(1 if failed else 0)


# ---------------- çapalar ----------------

def read_regions(path):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    px = im.load()
    grid = [[(px[x, y][0], px[x, y][1]) if px[x, y][3] >= 128 else (0, 0) for x in range(w)] for y in range(h)]
    return grid, w, h


def components(cells):
    """4-komşu bağlı bileşenler; her biri piksel listesi."""
    cells = set(cells)
    out = []
    while cells:
        stack = [cells.pop()]
        comp = []
        while stack:
            x, y = stack.pop()
            comp.append((x, y))
            for n in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                if n in cells:
                    cells.remove(n)
                    stack.append(n)
        out.append(comp)
    return out


def anchor_of(grid, w, h):
    """Baş çapası: en üst opak satır (kel kafanın tepesi), x = üst 4 satırdaki opak piksellerin ortası.
    Gözler: iris (yoksa göz akı) bileşenlerinin merkezleri, soldan sağa."""
    top = next((y for y in range(h) if any(grid[y][x][0] for x in range(w))), None)
    if top is None:
        return None
    xs = [x for y in range(top, min(h, top + 4)) for x in range(w) if grid[y][x][0]]
    hx = int(round(sum(xs) / len(xs)))
    top_row = [x for x in range(w) if grid[top][x][0]]
    hx = min(top_row, key=lambda x: (abs(x - hx), x))   # çapa opak piksele oturur
    eyes = []
    iris = [(x, y) for y in range(h) for x in range(w) if grid[y][x][0] == REGION["iris"]]
    if not iris:
        iris = [(x, y) for y in range(h) for x in range(w) if grid[y][x][0] == REGION["eye_white"]]
    for comp in components(iris):
        if len(comp) > 12:   # göz değil (manken hatası); yok say
            continue
        cx = sum(p[0] for p in comp) / len(comp)
        cy = sum(p[1] for p in comp) / len(comp)
        eyes.append(min(comp, key=lambda p: ((p[0] - cx) ** 2 + (p[1] - cy) ** 2, p)))
    eyes.sort()
    return {"hx": hx, "hy": top, "eyes": [v for e in eyes[:2] for v in e]}


def read_markers(path):
    """Görselin içindeki çapalar (B kanalı): {'hx','hy','eyes'} ya da None."""
    return read_markers_im(Image.open(path).convert("RGBA"))


def cmd_anchors(args):
    out = []
    for p in args.files:
        a = read_markers(p)
        name = os.path.splitext(os.path.basename(p))[0]
        if a is None:
            print(f"ÇAPASIZ  {p}")
            continue
        out.append({"name": name, **a})
        print(f"{name}: baş ({a['hx']},{a['hy']}), göz {a['eyes']}")
    if args.out:
        with open(args.out, "w", encoding="utf-8", newline="\n") as f:
            json.dump(out, f, ensure_ascii=False, indent=1)
            f.write("\n")


# ---------------- betikle beden (B1) ----------------

def torso_band(grid, w, h):
    rows = [y for y in range(h) if any(grid[y][x][0] in TORSO for x in range(w))]
    return (rows[0], rows[-1]) if rows else (None, None)


def widths_for(build, top, bottom):
    """Satır başına her iki yana eklenecek (+) ya da çıkarılacak (-) piksel."""
    n = bottom - top + 1
    out = {}
    for i, y in enumerate(range(top, bottom + 1)):
        t = i / max(1, n - 1)          # 0 omuz, 1 kalça
        if build == "ince":
            k = -1
        elif build == "gurbuz":
            k = 1 if t < 0.55 else 0   # omuz ve göğüs
        elif build == "dolgun":
            k = 2 if 0.35 <= t <= 0.85 else 1   # bel ve karın en geniş
        else:
            k = 0
        out[y] = k
    return out


def stretch_row(row, k):
    """Opak aralığı (kontur uçlar dahil) her yandan k piksel genişletir/daraltır; en yakın komşu örnekleme.
    Uçlardaki kontur pikselleri korunur."""
    idx = [x for x, c in enumerate(row) if c[0]]
    if not idx or k == 0:
        return row
    left, right = idx[0], idx[-1]
    seg = row[left:right + 1]
    n = len(seg)
    m = n + 2 * k
    if m < 3:
        return row
    inner, inner_m = seg[1:-1], m - 2
    new_inner = [inner[min(len(inner) - 1, int(i * len(inner) / inner_m))] for i in range(inner_m)] if inner else []
    new_seg = [seg[0]] + new_inner + [seg[-1]]
    out = [(0, 0)] * len(row)
    start = left - k
    for i, c in enumerate(new_seg):
        x = start + i
        if 0 <= x < len(row):
            out[x] = c
    return out


def derive_build(grid, w, h, build):
    top, bottom = torso_band(grid, w, h)
    if top is None:
        return grid
    ks = widths_for(build, top, bottom)
    return [stretch_row(grid[y], ks[y]) if y in ks else grid[y] for y in range(h)]


def write_regions(grid, w, h, path, force):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = im.load()
    for y in range(h):
        for x in range(w):
            code, shade = grid[y][x]
            if code:
                px[x, y] = (code, shade, 0, 255)
    mark_anchors(im)
    save_new(im, path, force)


# ---------------- baş parçası çıkarma (P2) ----------------

PART_W, PART_H, PART_AX, PART_AY = 64, 96, 32, 24   # LayerTypes.cs PartCanvas ile aynı
DIFF_TOLERANCE = 18.0


def extract_part(base_regions, edited, src=None):
    """Mankenin bölge haritası (çapalı) + PixelLab'in parça eklediği kare -> (ön, arka) parça tuvalleri.
    Değişen pikseller parçadır. Gövdenin üstüne binen ya da çene satırının üstündekiler ön (fr);
    gövdenin dışında ve çenenin altında kalanlar (sırta dökülen saç) arka (bk) katmandır.
    src: mankenin anahtar renkli kaynağı (verilirse fark ona göre ölçülür; yoksa bölge haritasının önizlemesi)."""
    a = read_markers_im(base_regions)
    if a is None:
        raise SystemExit("bölge haritasında çapa yok (manken.py regions ile üretilmeli)")
    w, h = base_regions.size
    bp, ep = base_regions.load(), edited.convert("RGBA").load()
    sp = src.convert("RGBA").load() if src is not None else None
    chin = a["eyes"][1] + 6 if a["eyes"] else a["hy"] + 14
    front = Image.new("RGBA", (PART_W, PART_H), (0, 0, 0, 0))
    back = Image.new("RGBA", (PART_W, PART_H), (0, 0, 0, 0))
    fp, kp = front.load(), back.load()
    for y in range(h):
        for x in range(w):
            e = ep[x, y]
            if e[3] < 128:
                continue
            on_body = bp[x, y][3] >= 128
            if on_body and sp is not None and ramps.dist(e[:3], sp[x, y][:3]) <= DIFF_TOLERANCE:
                continue   # değişmemiş gövde pikseli
            if on_body and sp is None:
                continue   # kaynak yoksa yalnızca gövdenin dışındaki eklemeler alınır
            tx, ty = x - a["hx"] + PART_AX, y - a["hy"] + PART_AY
            if not (0 <= tx < PART_W and 0 <= ty < PART_H):
                continue
            target = kp if (not on_body and y > chin) else fp
            target[tx, ty] = e
    return front, back


def read_markers_im(im):
    w, h = im.size
    px = im.load()
    head, eyes = None, []
    for y in range(h):
        for x in range(w):
            b = px[x, y][2]
            if b == 1:
                head = (x, y)
            elif b == 2:
                eyes.append((x, y))
    if head is None:
        return None
    eyes.sort()
    return {"hx": head[0], "hy": head[1], "eyes": [v for e in eyes for v in e]}


def cmd_extract(args):
    base = Image.open(args.base).convert("RGBA")
    src = Image.open(args.src) if args.src else None
    front, back = extract_part(base, Image.open(args.edited), src)
    os.makedirs(args.out_dir, exist_ok=True)
    for img, suffix in ((front, "_fr"), (back, "_bk")):
        if img.getbbox() is None and suffix == "_bk":
            continue
        dst = os.path.join(args.out_dir, args.name + suffix + ".png")
        save_new(img, dst, args.force)
        print(dst)


def cmd_build(args):
    os.makedirs(args.out_dir, exist_ok=True)
    for p in args.files:
        name = os.path.basename(p)
        if "_orta_" not in name:
            sys.exit(f"kaynak Orta beden olmalı (adında _orta_): {name}")
        grid, w, h = read_regions(p)
        out = derive_build(grid, w, h, args.build)
        dst = os.path.join(args.out_dir, name.replace("_orta_", f"_{args.build}_"))
        write_regions(out, w, h, dst, args.force)
        print(f"{p} -> {dst}")


# ---------------- önizleme ----------------

PREVIEW = {1: (42, 30, 30), 2: (217, 168, 119), 3: (160, 110, 70), 4: (70, 50, 35), 5: (80, 70, 60),
           6: (94, 70, 48), 7: (94, 70, 48), 8: (240, 236, 228), 9: (60, 90, 140), 10: (70, 45, 30)}


def cmd_preview(args):
    grid, w, h = read_regions(args.file)
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = im.load()
    for y in range(h):
        for x in range(w):
            code, shade = grid[y][x]
            if code in PREVIEW:
                base = PREVIEW[code]
                ramp = ramps.make_ramp(base) if code not in (1, 8, 9, 10) else [base] * 4
                px[x, y] = tuple(ramp[min(3, shade)]) + (255,)
    im = im.resize((w * args.scale, h * args.scale), Image.NEAREST)
    im.save(args.out)
    print(args.out)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)
    r = sub.add_parser("regions")
    r.add_argument("files", nargs="+")
    r.add_argument("--out-dir", required=True)
    r.add_argument("--force", action="store_true")
    r.add_argument("--keep-failed", action="store_true")
    a = sub.add_parser("anchors")
    a.add_argument("files", nargs="+")
    a.add_argument("--out")
    e = sub.add_parser("extract")
    e.add_argument("--base", required=True, help="mankenin bölge haritası (lay_body_*)")
    e.add_argument("--edited", required=True, help="PixelLab'in parça eklediği kare")
    e.add_argument("--src", help="mankenin anahtar renkli kaynağı (lay_src_*); gövde üstündeki eklemeler için")
    e.add_argument("--out-dir", required=True)
    e.add_argument("--name", required=True, help="ör. lay_mass_short_se")
    e.add_argument("--force", action="store_true")
    b = sub.add_parser("build")
    b.add_argument("files", nargs="+")
    b.add_argument("--build", choices=["ince", "gurbuz", "dolgun"], required=True)
    b.add_argument("--out-dir", required=True)
    b.add_argument("--force", action="store_true")
    v = sub.add_parser("preview")
    v.add_argument("file")
    v.add_argument("--out", required=True)
    v.add_argument("--scale", type=int, default=4)
    args = ap.parse_args()
    {"regions": cmd_regions, "anchors": cmd_anchors, "build": cmd_build, "extract": cmd_extract, "preview": cmd_preview}[args.cmd](args)


if __name__ == "__main__":
    main()
