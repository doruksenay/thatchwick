#!/usr/bin/env python3
"""İzometrik PNG doğrulaması (boyut, saydamlık, renk sayısı, sınırlar, pivot, zemin maskesi).

Kullanım:
  python tools/art/validate_iso.py <dosya.png> [--kind ground|object]
                                   [--footprint X,Y,W,H] [--max-colors N]

--kind ground : 64x32 olmalı; tam 2:1 elmas maskesi dışında opak piksel ve maske içinde delik sayar.
--footprint   : ayak izi taban elmasının tuvaldeki kutusu (x,y,w,h). Verilirse pivotu (x, y ALTTAN) hesaplar.
Çıkış kodu 0: kritik hata yok, 1: en az bir kontrol başarısız. Işık sırası ve döşeme testi bu betikte yok;
onları işçi ayrıca ölçer ve raporlar.

Toplu kip (ad kuralına göre tür seçer, PASS/FAIL tablosu yazar, hiçbir PNG'yi değiştirmez):
  uv run --with pillow python tools/art/validate_iso.py --all Assets/Art/Iso [--out art/results/validate-<tarih>.md]

  iso_ground_*                 zemin: 64x32, maske dışı 0, delik 0, her satır tam 2:1 elmas sınırında, yarı saydam 0
  iso_char_*, iso_animal_*     renk <= 32
  iso_item_*, iso_wall_*, ui_* renk <= 48
  hepsi (zemin dışı)           yarı saydam 0, opak piksel var, en alt satıra değmiyor (alt satıra değen = kırpılmış)
  diğer adlar                  yalnızca ölçülür, PASS/FAIL verilmez
"""
import argparse
import datetime
import os
import sys
from PIL import Image


def in_diamond(px, py, cx, cy, w=64, h=32):
    return abs(px + 0.5 - cx) / (w / 2) + abs(py + 0.5 - cy) / (h / 2) <= 1.0


COLOR_LIMITS = (("iso_char_", 32), ("iso_animal_", 32), ("iso_item_", 48), ("iso_wall_", 48), ("ui_", 48))


def kind_of(name):
    if name.startswith("iso_ground_"):
        return "ground", 0
    for prefix, limit in COLOR_LIMITS:
        if name.startswith(prefix):
            return prefix.rstrip("_"), limit
    return "other", 0


def measure(path):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    px = im.load()
    semi = 0
    colors = set()
    rows = {}
    for y in range(h):
        for x in range(w):
            r, g, b, al = px[x, y]
            if al == 255:
                colors.add((r, g, b))
                lo, hi = rows.get(y, (x, x))
                rows[y] = (min(lo, x), max(hi, x))
            elif al:
                semi += 1
    m = {"w": w, "h": h, "semi": semi, "colors": len(colors), "rows": rows, "px": px}
    if rows:
        ys = sorted(rows)
        m["bbox"] = (min(v[0] for v in rows.values()), ys[0], max(v[1] for v in rows.values()), ys[-1])
    return m


def check_ground(m):
    fails, notes = [], []
    if (m["w"], m["h"]) != (64, 32):
        return [f"boyut {m['w']}x{m['h']} (64x32 olmalı)"], notes
    px = m["px"]
    outside = holes = 0
    for y in range(32):
        for x in range(64):
            inside = in_diamond(x, y, 32, 16)
            opq = px[x, y][3] == 255
            outside += opq and not inside
            holes += inside and not opq
    bad_rows = []
    for y in range(32):
        k = y if y < 16 else 31 - y
        want = (31 - 2 * k, 32 + 2 * k)
        if m["rows"].get(y) != want:
            bad_rows.append(y)
    if outside:
        fails.append(f"maske dışı {outside}")
    if holes:
        fails.append(f"delik {holes}")
    if bad_rows:
        fails.append(f"2:1 kenar dışı satır {len(bad_rows)} (ilk: y={bad_rows[0]})")
    if m["semi"]:
        fails.append(f"yarı saydam {m['semi']}")
    return fails, notes


def check_object(m, limit):
    fails, notes = [], []
    if "bbox" not in m:
        return ["opak piksel yok"], notes
    x0, y0, x1, y1 = m["bbox"]
    if m["semi"]:
        fails.append(f"yarı saydam {m['semi']}")
    if limit and m["colors"] > limit:
        fails.append(f"renk {m['colors']} > {limit}")
    if y1 >= m["h"] - 1:
        fails.append("en alt satıra değiyor")
    edges = [n for n, t in (("üst", y0 == 0), ("sol", x0 == 0), ("sağ", x1 == m["w"] - 1)) if t]
    if edges:
        notes.append("kenara değiyor: " + ", ".join(edges))
    return fails, notes


def batch(root, out):
    files = []
    for d, _, names in os.walk(root):
        for n in names:
            if n.lower().endswith(".png"):
                files.append(os.path.join(d, n))
    files.sort(key=lambda p: (os.path.relpath(p, root).count(os.sep), p.lower()))
    rows, counts = [], {"PASS": 0, "FAIL": 0, "—": 0}
    for p in files:
        name = os.path.splitext(os.path.basename(p))[0]
        kind, limit = kind_of(name)
        m = measure(p)
        if kind == "ground":
            fails, notes = check_ground(m)
        elif kind == "other":
            fails, notes = [], ["ad kuralı dışı, yalnızca ölçüldü"]
        else:
            fails, notes = check_object(m, limit)
        verdict = "—" if kind == "other" else ("FAIL" if fails else "PASS")
        counts[verdict] += 1
        if "bbox" in m:
            x0, y0, x1, y1 = m["bbox"]
            hw = f"{y1 - y0 + 1} / {x1 - x0 + 1}"
            bottom = f"{m['h'] - 1 - y1}"
        else:
            hw, bottom = "—", "—"
        rel = os.path.relpath(p, root).replace(os.sep, "/")
        rows.append(f"| {rel} | {kind} | {m['w']}x{m['h']} | {hw} | {m['colors']}"
                    f"{' / ' + str(limit) if limit else ''} | {m['semi']} | {bottom} | **{verdict}** | "
                    f"{'; '.join(fails + notes)} |")
    today = datetime.date.today().isoformat()
    head = [f"# Doğrulama: {root} ({today})", "",
            "Betik: `tools/art/validate_iso.py --all` (yalnızca okur, düzeltme yapmaz). "
            "Boy/genişlik = opak sınır kutusu; alt boşluk = en alt opak satırın altındaki saydam satır sayısı. "
            "Zemin kuralı: 64x32, maske dışı 0, delik 0, her satırın iki ucu tam 2:1 elmas sınırında "
            "(2 px uç, en geniş satır 62 px, x=1-62), yarı saydam 0. "
            "Renk sınırı: karakter ve hayvan 32, eşya, duvar ve arayüz 48.", "",
            f"Toplam {len(files)} PNG: PASS {counts['PASS']}, FAIL {counts['FAIL']}, ölçüldü {counts['—']}.", "",
            "| Dosya | Tür | Tuval | Boy / genişlik | Renk / sınır | Yarı saydam | Alt boşluk | Sonuç | Bulgu |",
            "|---|---|---|---|---|---|---|---|---|"]
    text = "\n".join(head + rows) + "\n"
    if out:
        if os.path.exists(out):
            base, ext = os.path.splitext(out)
            i = 2
            while os.path.exists(f"{base}-{i}{ext}"):
                i += 1
            out = f"{base}-{i}{ext}"
        with open(out, "w", encoding="utf-8") as f:
            f.write(text)
        print(f"yazıldı: {out}")
    print(f"PASS {counts['PASS']}, FAIL {counts['FAIL']}, ölçüldü {counts['—']}")
    return counts["FAIL"]


def main():
    if "--all" in sys.argv:
        ap = argparse.ArgumentParser()
        ap.add_argument("--all", required=True, metavar="KLASOR")
        ap.add_argument("--out")
        a = ap.parse_args()
        sys.exit(1 if batch(a.all, a.out) else 0)
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
