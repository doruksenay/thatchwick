#!/usr/bin/env python3
"""Stil kilidi araçları: ana palet, palet uyumu denetimi ve referanslarla yan yana karşılaştırma.

Tutarlılığın üç ayağı (art/style/README.md):
  1. Referanslar  art/style/references.json  (aile başına kabul edilmiş görseller; PixelLab stil eşlemesi bunları kullanır)
  2. Paletler     art/style/palette.json (dünya) ve art/style/palette_arayuz.json (ikon ve arayüz); ikisi de DONDURULMUŞ
                  (kullanıcı kararı 2026-11-20, Project belgesi 64c). Kaynak: art/queue'da "accepted" işlerin çıktıları.
  3. Yan yana     art/results/NNN-*-yanyana.png (yeni çıktı, ailesinin referanslarıyla aynı ölçekte)

Palet seçimi: dosya adı icon_ ya da ui_ ile başlıyorsa ya da aile "arayuz" ise arayüz paleti, değilse dünya paleti.
Karakter katmanlarının asıl kilidi rampalardır (tools/art/ramps.py); burada dünya paletiyle kaba denetim yapılır.

Kullanım:
  python tools/art/style.py snap [--set dunya|arayuz] <png>...      # donmuş palete oturt (yerinde, 0 gen; kullanıcı kararıyla)
  python tools/art/style.py palette --set dunya|arayuz --yeniden "karar notu" [--png]
                                                         # donmuş paleti yeniden üretir; yalnızca kullanıcı kararıyla
  python tools/art/style.py check <png>... [--family F] [--light]  # palet uyumu; eşik altıysa çıkış 1
                                                         # --light: alev/ışık pikselleri denetim dışı (ışık kaynağı kuralı)
  python tools/art/style.py sheet --family F --out X.png <png>...   # referanslar + yeni görseller, 4x, aynı taban çizgisi
  python tools/art/style.py fit --family F <girdi.png> <çıktı.png>  # yalnızca doygunluğu aileye indirir (renk/ton aynı)

Kilidi kurulmamış aile (KILIT_ESIGI'nden az "kabul" referans): palet uyumu kalma sebebi değil UYARI'dır,
çünkü palet o ailenin renklerini henüz içermez (2026-11-18: bitki ailesinde palet 3 yeşil içeriyordu, 310/311/320/322
pilot dahil hiç teslim çıkaramadı). Doygunluk denetimi her ailede aynen geçerli.
"""
import argparse
import json
import math
import os
import sys

from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
REFS = os.path.join(ROOT, "art", "style", "references.json")
QUEUE = os.path.join(ROOT, "art", "queue")
PALETTE_JSON = os.path.join(ROOT, "art", "style", "palette.json")
PALETTE_UI_JSON = os.path.join(ROOT, "art", "style", "palette_arayuz.json")

# Palet takımları (kullanıcı kararı 2026-11-20, Project belgesi 64c §1): dünya "yaklaşık 96" → 112 (96 renkte kabul
# edilmiş kaynakların 16'sı kendi paletinden kalıyordu, 112'de 13'ü); ikon ve arayüz ayrı, daha doygun ve daha kalabalık.
PALETTE_SETS = {
    "dunya": {"json": PALETTE_JSON, "size": 112, "families": ("zemin", "yapi", "esya", "bitki", "hayvan")},
    "arayuz": {"json": PALETTE_UI_JSON, "size": 128, "families": None},   # None: bütün ailelerin icon_/ui_ çıktıları
}
UI_PREFIXES = ("icon_", "ui_")
SAMPLE_PER_IMAGE = 400     # palet üretiminde görsel başına en çok örnek piksel (büyük görsel paleti ezmesin)
SAMPLE_SEED = 7            # aynı kabul listesi aynı paleti verir
PALETTE_MIN_PER_SECTOR = 3 # ton dilimi başına en az renk (_stratified_palette)
NEAR = 20.0                # bir pikselin palete "yakın" sayıldığı en büyük uzaklık (algısal ağırlıklı RGB)
# Eşikler 2026-11-13'te ölçüldü: referanslar ortalama 1-6, aynı aileden kabul görmüş adaylar 4-12,
# doygunluğu 1,8-2,2 kat artırılmış ya da tonu kaydırılmış kopyalar 11-12 uzaklık ve 1,4-1,9 kat doygunluk.
# Bu denetim kaba sapmayı yakalar; ince stil farkı insan gözüyle (yan yana sayfası) ve pilot kabulüyle çözülür.
MEAN_WARN, MEAN_FAIL = 10.0, 16.0          # palete ortalama uzaklık
NEAR_FAIL = 0.80                            # palete yakın piksel oranı bunun altındaysa kalır
LIGHT_MAX = 0.35                            # --light: ışık pikseli bu oranı aşarsa KALDI (eşyanın kendisi denetlenebilmeli)
SAT_WARN, SAT_FAIL = 1.25, 1.50            # ortalama doygunluk / aile referanslarının ortalaması
SAT_LOW_WARN = 0.60
KILIT_ESIGI = 3                             # ailede en az bu kadar "kabul" referans yoksa palet denetimi uyarıdır
FIT_TARGET = 1.15                           # fit: hedef doygunluk / aile ortalaması


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


def _expand(pattern):
    """`a_{x,y}_b.png` → [`a_x_b.png`, `a_y_b.png`] (iş dosyası çıktı listesi biçimi)."""
    import re
    m = re.search(r"\{([^{}]*)\}", pattern)
    if not m:
        return [pattern]
    out = []
    for part in m.group(1).split(","):
        out.extend(_expand(pattern[: m.start()] + part + pattern[m.end():]))
    return out


def accepted_job_files():
    """art/queue'da status: accepted işlerin var olan PNG çıktıları, aileye göre."""
    import glob
    import re
    out = {}
    for path in sorted(glob.glob(os.path.join(QUEUE, "*.md"))):
        with open(path, encoding="utf-8") as f:
            parts = f.read().split("---")
        if len(parts) < 3:
            continue
        head = parts[1]
        st = re.search(r"^status:\s*(\w+)", head, re.M)
        fam = re.search(r"^family:\s*(\w+)", head, re.M)
        if not st or st.group(1) != "accepted" or not fam:
            continue
        for line in re.findall(r"^\s*-\s*(\S+\.png)", head, re.M):
            for rel in _expand(line):
                p = os.path.join(ROOT, rel)
                if os.path.exists(p):
                    out.setdefault(fam.group(1), set()).add(p)
    return {k: sorted(v) for k, v in out.items()}


def palette_sources(set_name):
    jobs = accepted_job_files()
    conf = PALETTE_SETS[set_name]
    files = []
    for fam, paths in jobs.items():
        for p in paths:
            ui = os.path.basename(p).startswith(UI_PREFIXES) or fam == "arayuz"
            if set_name == "arayuz" and ui:
                files.append(p)
            elif set_name == "dunya" and not ui and fam in conf["families"]:
                files.append(p)
    return sorted(set(files))


def build_palette(args):
    conf = PALETTE_SETS[args.set]
    if os.path.exists(conf["json"]):
        with open(conf["json"], encoding="utf-8") as f:
            if json.load(f).get("donduruldu") and not args.yeniden:
                sys.exit(f"{os.path.relpath(conf['json'], ROOT)} donduruldu. Yeniden üretim yalnızca kullanıcı kararıyla: "
                         f"--yeniden \"<tarih, karar ve gerekçe>\"")
    files = palette_sources(args.set)
    if not files:
        sys.exit("Kabul edilmiş kaynak görsel bulunamadı (art/queue, status: accepted). PNG'ler Git LFS'te: git lfs pull.")
    colors = _stratified_palette(files, conf["size"])
    data = {
        "_aciklama": "DONDURULDU. Elle düzenleme; yeni renk yalnızca kullanıcı kararıyla "
                     "(tools/art/style.py palette --set " + args.set + " --yeniden \"karar\"). Bkz. docs/gorsel-yonerge.md.",
        "takim": args.set,
        "donduruldu": True,
        "karar": args.yeniden or "",
        "kaynak_sayisi": len(files),
        "yakinlik_esigi": NEAR,
        "renkler": ["#%02x%02x%02x" % c for c in colors],
    }
    with open(conf["json"], "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")
    if args.png:   # önizleme; repoya commit edilmez (.gitignore)
        sw = Image.new("RGB", (16 * 16, 16 * int(math.ceil(len(colors) / 16))), (51, 51, 51))
        d = ImageDraw.Draw(sw)
        for i, c in enumerate(colors):
            x, y = (i % 16) * 16, (i // 16) * 16
            d.rectangle([x, y, x + 15, y + 15], fill=c)
        sw.save(conf["json"][:-5] + ".png")
    print(f"{len(colors)} renk, {len(files)} kabul edilmiş görsel -> {os.path.relpath(conf['json'], ROOT)}")


def _hue_sector(c):
    import colorsys
    h, sat, v = colorsys.rgb_to_hsv(*(x / 255 for x in c))
    if sat < 0.15 or v < 0.18:
        return "n%d" % min(3, int(v * 4))            # nötr: dört açıklık basamağı
    return "h%d" % (int(h * 360 + 15) % 360 // 30)   # renkli: 12 ton dilimi (30°)


def _stratified_palette(files, size):
    """Ton dilimli median-cut. Düz median-cut azınlık tonları (su mavisi, çiçek pembesi) baskın kahvelerin içinde
    eritiyordu: kabul edilmiş su karoları kendi paletinden kalıyordu (2026-11-20). Her dilime nüfusunun karekökü oranında
    renk ayrılır, dolu her dilim en az PALETTE_MIN_PER_SECTOR renk alır."""
    import random
    rnd = random.Random(SAMPLE_SEED)
    groups = {}
    for p in files:
        px = opaque_pixels(p)
        for c in (rnd.sample(px, min(len(px), SAMPLE_PER_IMAGE)) if px else []):
            groups.setdefault(_hue_sector(c), []).append(c)
    weight = {k: math.sqrt(len(v)) for k, v in groups.items()}
    total = sum(weight.values())
    alloc = {k: max(PALETTE_MIN_PER_SECTOR, round(size * weight[k] / total)) for k in groups}
    while sum(alloc.values()) > size:
        alloc[max(alloc, key=lambda k: (alloc[k], k))] -= 1
    colors = set()
    for k in sorted(groups):
        v = groups[k]
        n = min(alloc[k], len(set(v)))
        im = Image.new("RGB", (len(v), 1))
        im.putdata(v)
        pal = im.quantize(colors=n, method=Image.Quantize.MEDIANCUT).getpalette()[: n * 3]
        colors.update(tuple(pal[i: i + 3]) for i in range(0, n * 3, 3))
    return sorted(colors, key=lambda c: (0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2], c))


def snap_to_palette(path, set_name=None, out=None):
    """Her opak pikseli donmuş paletteki en yakın renge çeker (saydamlık korunur). 0 gen.
    Kullanıcı kararıyla kilitten önce kabul edilmiş görselleri oturtmak için (Project belgesi 64c §0.1)."""
    set_name = set_name or palette_set_for(path)
    pal = load_palette(set_name)
    im = Image.open(path).convert("RGBA")
    px = im.load()
    cache = {}
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            key = (r, g, b)
            if key not in cache:
                cache[key] = min(pal, key=lambda p: (p[0] - r) ** 2 + (p[1] - g) ** 2 + (p[2] - b) ** 2)
            px[x, y] = cache[key] + (a,)
    im.save(out or path)
    return set_name


def snap(args):
    for p in args.files:
        used = snap_to_palette(p, args.set)
        print(f"oturtuldu ({used}): {os.path.relpath(os.path.abspath(p), ROOT)}")


def palette_set_for(path, family=""):
    if family == "arayuz" or os.path.basename(path).startswith(UI_PREFIXES):
        return "arayuz"
    return "dunya"


def load_palette(set_name="dunya"):
    with open(PALETTE_SETS[set_name]["json"], encoding="utf-8") as f:
        d = json.load(f)
    return [tuple(int(h[i: i + 2], 16) for i in (1, 3, 5)) for h in d["renkler"]]


def _is_frozen(set_name):
    path = PALETTE_SETS[set_name]["json"]
    if not os.path.exists(path):
        return False
    with open(path, encoding="utf-8") as f:
        return bool(json.load(f).get("donduruldu"))


def is_light_pixel(c):
    """Alev ve ışık pikseli: sıcak ton (kırmızı-turuncu-sarı), parlak ve doygun ya da alevin beyaza yakın sıcak çekirdeği.
    Ahşap ve pirinç gibi koyu sıcak tonlar (değer < 0,75) dışarıda kalır."""
    import colorsys
    h, sat, v = colorsys.rgb_to_hsv(c[0] / 255, c[1] / 255, c[2] / 255)
    hue = h * 360
    warm = hue <= 60 or hue >= 345
    if warm and v >= 0.75 and sat >= 0.45:
        return True
    return v >= 0.92 and sat < 0.35 and c[0] >= c[1] >= c[2]


def light_split(path):
    """(ışık dışı pikseller, ışık oranı)."""
    px = opaque_pixels(path)
    rest = [c for c in px if not is_light_pixel(c)]
    return rest, (1 - len(rest) / len(px)) if px else 0.0


def conformity(path, palette, cache, px=None):
    px = opaque_pixels(path) if px is None else px
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


def mean_saturation(path, px=None):
    import colorsys
    px = opaque_pixels(path) if px is None else px
    if not px:
        return 0.0
    return sum(colorsys.rgb_to_hls(r / 255, g / 255, b / 255)[2] for r, g, b in px) / len(px)


def check(args):
    palettes, caches = {}, {}
    refs = ref_files([args.family]) if args.family else []
    ref_sat = sum(mean_saturation(r) for r in refs) / len(refs) if refs else None
    # Donmuş paletler bütün ailelerin kabul edilmiş görsellerinden üretildiği için palet uyumu her ailede bağlayıcıdır
    # (2026-11-20). Eski paletle (donmamış) aile kilidi kuralı sürer.
    frozen = all(_is_frozen(n) for n in PALETTE_SETS)
    locked = frozen or (not args.family) or len(ref_files([args.family], only_accepted=True)) >= KILIT_ESIGI
    if not locked:
        print(f"Not: '{args.family}' ailesinin kilidi kurulmadı (kabul referans < {KILIT_ESIGI}); palet uyumu yalnızca uyarı.")
    failed = False
    print("| Dosya | Palete yakın | Ortalama uzaklık | Doygunluk / aile | Sonuç | Bulgu |")
    print("|---|---|---|---|---|---|")
    for p in args.files:
        px, light = (light_split(p) if args.light else (None, 0.0))
        pset = palette_set_for(p, args.family)
        if pset not in palettes:
            palettes[pset], caches[pset] = load_palette(pset), {}
        ratio, mean = conformity(p, palettes[pset], caches[pset], px)
        sat = mean_saturation(p, px) / ref_sat if ref_sat else None
        notes, level = [], 0
        if args.light:
            notes.append(f"ışık pikseli %{light * 100:.0f} denetim dışı")
            if light > LIGHT_MAX:
                notes.append(f"ışık payı %{LIGHT_MAX * 100:.0f}'i aşıyor"); level = 2
        if mean > MEAN_FAIL or ratio < NEAR_FAIL:
            if locked:
                notes.append("palet dışı renkler"); level = 2
            else:
                notes.append("palet dışı renkler (kilit kurulmadı: uyarı)"); level = max(level, 1)
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


def fit(args):
    """Yalnızca doygunluğu aile ortalamasının FIT_TARGET katına indirir; ton, açıklık ve saydamlık aynı kalır.
    Renkleri palete zorlamaz (palete eşleme gölgelemeyi bozuyor; 2026-11-18 denemesi). Üzerine yazmaz."""
    import colorsys
    refs = ref_files([args.family])
    if not refs:
        sys.exit(f"'{args.family}' ailesinin referansı yok.")
    if os.path.exists(args.out):
        sys.exit(f"hedef zaten var, üzerine yazılmaz: {args.out}")
    target = args.target * sum(mean_saturation(r) for r in refs) / len(refs)
    im = Image.open(args.src).convert("RGBA")
    px = im.load()
    pts = [(x, y) for y in range(im.height) for x in range(im.width) if px[x, y][3] >= 128]
    if not pts:
        sys.exit("opak piksel yok")
    hls = {p: colorsys.rgb_to_hls(*(c / 255 for c in px[p][:3])) for p in pts}
    mean = sum(v[2] for v in hls.values()) / len(hls)
    k = min(1.0, target / mean) if mean else 1.0
    for p, (h, l, s) in hls.items():
        r, g, b = colorsys.hls_to_rgb(h, l, s * k)
        px[p] = (round(r * 255), round(g * 255), round(b * 255), px[p][3])
    os.makedirs(os.path.dirname(os.path.abspath(args.out)), exist_ok=True)
    im.save(args.out)
    print(f"{os.path.basename(args.src)} -> {args.out}: doygunluk x{k:.2f} (aile hedefi {target:.3f})")


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
    sn = sub.add_parser("snap", help="görseli donmuş palete oturt (yerinde yazar)")
    sn.add_argument("--set", choices=sorted(PALETTE_SETS), default=None, help="boşsa dosya adından seçilir")
    sn.add_argument("files", nargs="+")
    sn.set_defaults(fn=snap)

    pp = sub.add_parser("palette")
    pp.add_argument("--set", choices=sorted(PALETTE_SETS), required=True)
    pp.add_argument("--yeniden", default="", help="donmuş paleti yeniden üret; kullanıcı kararının notu (tarih, gerekçe)")
    pp.add_argument("--png", action="store_true", help="art/style/palette*.png önizlemesini de yaz")
    pp.set_defaults(fn=build_palette)
    c = sub.add_parser("check")
    c.add_argument("files", nargs="+")
    c.add_argument("--family", default="")
    c.add_argument("--light", action="store_true",
                   help="ışık kaynağı kuralı: alev ve ışık pikselleri palet ve doygunluk denetiminin dışında (art/TEMPLATE.md)")
    c.set_defaults(fn=check)
    f = sub.add_parser("fit")
    f.add_argument("--family", required=True)
    f.add_argument("--target", type=float, default=FIT_TARGET)
    f.add_argument("src")
    f.add_argument("out")
    f.set_defaults(fn=fit)
    s = sub.add_parser("sheet")
    s.add_argument("files", nargs="*")
    s.add_argument("--family", default="")
    s.add_argument("--out", required=True)
    s.set_defaults(fn=sheet)
    a = ap.parse_args()
    a.fn(a)


if __name__ == "__main__":
    main()
