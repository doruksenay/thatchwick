"""Eşya ↔ görsel eşleme denetimi ve onarımı (tester raporu 66 §1.3).

Depoda görseli olduğu halde items.json'daki adı tutmadığı ya da hiç eşlenmediği için yer tutucu kutuyla çizilen eşyaları bulur.
  python tools/art/art_link.py            # rapor (değişiklik yok)
  python tools/art/art_link.py --apply    # items.json'a yazar

Kurallar (docs/gorsel-yonerge.md "Yön kuralı"):
- Görsel adındaki ölçü (_2x1 vb.) ayak iziyle uyuşmuyorsa eşlenmez.
- Sürüm eki varsa en yeni sürüm (_v3 > _v2 > ek yok) seçilir; eşlenen ad depoda yoksa aynı adın başka sürümü aranır.
- Ön yüzlü eşyada (front) se görseli n, sw görseli w yüzü olur; diğerlerinde alongX (se, se_nw) / alongY (sw, sw_ne).
- Pivot: yer eşyasında yatay orta, dikeyde alt boşluk + ayak izi elmasının yarısı; duvar ve tavan eşyasında tuvalin ortası;
  yüzey süsünde (placement Surface) boş bırakılır (oyunun varsayılanı).
"""
import json
import os
import re
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ITEMS = os.path.join(ROOT, "Assets", "Resources", "Content", "items.json")
ART = os.path.join(ROOT, "Assets", "Art", "Iso")
SKIP_CATEGORIES = {"resource", "clothing", "plant"}   # kaynak/giysi dünyada çizilmez; bitki evreleri IsoArt sözlüğünde
PREFIXES = ["iso_item_", "iso_forage_", "iso_decor_", "iso_ceiling_item_", "iso_wall_item_", "iso_deco_"]


def library():
    files = {}
    for root, _, names in os.walk(ART):
        rel = os.path.relpath(root, ART).replace("\\", "/")
        if "anim" in rel.split("/") or "_ham" in rel or "katman" in rel:
            continue
        for n in names:
            if n.endswith(".png"):
                files.setdefault(n[:-4], os.path.join(root, n))
    return files


VER = re.compile(r"_v(\d+)(?=_|$)")


def version(name):
    m = VER.search(name)
    return int(m.group(1)) if m else 1


def base(name):
    return VER.sub("", name)


_SIZES = {}


def size_ok(name, w, h, files=None):
    """Adda ölçü varsa ayak iziyle aynı olmalı; yoksa tuval genişliği ayak izine uymalı (1x1 ≤ 80 px, 2x1 ~112, 2x2 ~144)."""
    m = re.search(r"_(\d)x(\d)(?=_|$)", name)
    if m:
        return {int(m.group(1)), int(m.group(2))} == {w, h}
    if name.startswith(("iso_wall_item_", "iso_ceiling_item_", "iso_decor_", "iso_deco_")) or w == h == 1:
        if w == h == 1 and files is not None and name in files:
            return _width(files[name]) <= 96
        return w == h == 1
    if files is None or name not in files:
        return False
    return _width(files[name]) >= 32 * (w + h) + 8


def _width(path):
    if path not in _SIZES:
        _SIZES[path] = Image.open(path).size[0]
    return _SIZES[path]


def direction(name):
    n = base(name)
    for d in ("_se_nw", "_sw_ne", "_se", "_sw", "_ne", "_nw"):
        if n.endswith(d):
            return d.strip("_").split("_")[0]
    return "se"


def best(cands):
    return max(cands, key=lambda n: (version(n), -len(n))) if cands else None


def candidates(short, w, h, files):
    out = []
    keys = {short}
    if short.startswith("forage_"):
        keys.add(short[len("forage_"):])
    for n in files:
        if not n.startswith(tuple(PREFIXES)):
            continue
        b = base(n)
        for k in keys:
            for p in PREFIXES:
                if b.startswith(p + k):
                    rest = b[len(p + k):]
                    if re.fullmatch(r"(_\dx\d)?(_se|_sw|_se_nw|_sw_ne)?", rest) and size_ok(n, w, h, files) and direction(n) in ("se", "sw"):
                        out.append(n)
    return out


def bottom_pad(path):
    im = Image.open(path).convert("RGBA")
    a = im.getchannel("A")
    bbox = a.point(lambda v: 255 if v > 10 else 0).getbbox()
    if not bbox:
        return im.size, 0
    return im.size, im.size[1] - bbox[3]


def make_art(it, name, files, old):
    w, h = it.get("width", 1) or 1, it.get("height", 1) or 1
    (W, H), pad = bottom_pad(files[name])
    art = {k: v for k, v in (old or {}).items() if k not in ("alongX", "alongY", "n", "e", "s", "w", "pending", "pivotX", "pivotY", "pxW", "pxH")}
    d = direction(name)
    if it.get("front"):
        art["n" if d == "se" else "w"] = name
    else:
        art["alongX" if d == "se" else "alongY"] = name
    placement = it.get("placement")
    cat = it.get("category")
    if placement == "Surface":
        pass
    elif placement == "Ceiling" or cat in ("decor_wall",) or name.startswith("iso_wall_item_"):
        art["pivotX"], art["pivotY"] = float(W // 2), float(H // 2)
    else:
        art["pivotX"], art["pivotY"] = float(W // 2), float(pad + 8 * (w + h))
    art["pxW"], art["pxH"] = W, H
    art.setdefault("mirror", True)
    return art


def main(apply):
    files = library()
    text = open(ITEMS, encoding="utf-8").read()
    data = json.loads(text)
    fixed, linked, left = [], [], []
    for it in data["items"]:
        if it.get("deleted"):
            continue
        w, h = it.get("width", 1) or 1, it.get("height", 1) or 1
        art = it.get("art") or {}
        refs = [art.get(k) for k in ("alongX", "alongY", "n", "e", "s", "w", "lit", "young", "grown", "fruit") if art.get(k)]
        missing = [r for r in refs if r not in files]
        short = it["id"].split(".", 1)[1]
        if refs and not missing:
            if art.get("pending"):
                if apply:
                    del art["pending"]
                fixed.append((it["id"], "pending kalktı (görsel depoda)"))
            continue
        if it.get("category") in SKIP_CATEGORIES:
            continue
        if missing:
            # Aynı adın başka sürümü varsa ona geç (ör. shrub_v2 yok, shrub var).
            repl = {}
            for r in missing:
                alt = best([n for n in files if base(n) == base(r) and size_ok(n, w, h, files)])
                if alt:
                    repl[r] = alt
            if len(repl) == len(missing):
                if apply:
                    for k in list(art):
                        if art[k] in repl:
                            art[k] = repl[art[k]]
                    art.pop("pending", None)
                fixed.append((it["id"], ", ".join(f"{a} → {b}" for a, b in repl.items())))
                continue
            c = best(candidates(short, w, h, files))
            if c:
                if apply:
                    it["art"] = make_art(it, c, files, art)
                fixed.append((it["id"], f"{', '.join(missing)} yok → {c}"))
            else:
                left.append((it["id"], it.get("category"), f"{w}x{h}", "eşlenen görsel yok: " + ", ".join(missing)))
            continue
        c = best(candidates(short, w, h, files))
        if c:
            if apply:
                it["art"] = make_art(it, c, files, art)
            linked.append((it["id"], c))
        else:
            left.append((it["id"], it.get("category"), f"{w}x{h}", "görsel yok"))
    print(f"# Görsel eşleme: {len(fixed)} düzeltme, {len(linked)} yeni eşleme, {len(left)} eşya görselsiz")
    for i, m in fixed:
        print(f"düzelt  {i}: {m}")
    for i, m in linked:
        print(f"eşle    {i}: {m}")
    for row in left:
        print("eksik   " + " | ".join(row))
    if apply:
        out = json.dumps(data, ensure_ascii=False, indent=2) + "\n"
        open(ITEMS, "w", encoding="utf-8", newline="\n").write(out)


if __name__ == "__main__":
    main("--apply" in sys.argv)
