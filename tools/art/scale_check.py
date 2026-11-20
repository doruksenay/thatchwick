#!/usr/bin/env python3
"""Ölçek denetimi (docs/gorsel-yonerge.md "Ölçek referansı", tester raporu 66 §1.2).

Ölçü: görselin **ayak noktası (pivot) üstündeki** dolu yüksekliği, piksel. Yetişkin karakter 76 px (≈ 1,7 m; 1 m ≈ 45 px).
Eşyada pivot ayak izi elmasının ortasıdır: elmasın arka yarısı (1x1'de 16 px) ölçüye dahildir, tabloda hesaba katıldı.

  python tools/art/scale_check.py <png>...            # tabloda sınıfı olan her görsel; aralık dışıysa çıkış 1
  python tools/art/scale_check.py --pivot 24 <png>    # pivot yüksekliği (alttan) elle; yoksa sınıfın varsayılanı

Sınıf dosya adından bulunur (önek ve tür). Tabloda olmayan görsel "sınıf yok" diye geçer (hata değil).
"""
import argparse
import os
import re
import sys

from PIL import Image

# (desen, sınıf adı, en az, en çok, pivot alttan px). Pivot None: alt dolu satır (hayvan ve karakter ayak tabanı).
WILD_TREES = "oak|birch|pine|willow|walnut|linden|chestnut|poplar|ash|beech|hornbeam|alder|elm"
TABLE = [
    (r"^iso_char_(man|woman|crier|elder_[mf]|johan_wick|horseless_knight)_r84", "yetişkin karakter", 72, 80, None),
    (r"^iso_char_(boy|girl)_r64", "çocuk", 52, 62, None),
    (r"^iso_greattree_plane_", "ulu çınar (3x3, 192x192)", 165, 190, 48),
    (r"^iso_greattree_", "ulu ağaç (2x2, 160x224)", 185, 215, 36),
    (rf"^iso_tree_({WILD_TREES})_grown_se", "yaban ağacı, olgun", 120, 140, 24),
    (rf"^iso_tree_({WILD_TREES})_young_se", "yaban ağacı, genç", 60, 85, 24),
    (r"^iso_tree_\w+_(grown|fruit)_se", "meyve ağacı, olgun", 95, 115, 24),
    (r"^iso_tree_\w+_young_se", "meyve ağacı, genç", 50, 72, 24),
    (r"^iso_(bush|plant)_\w+_(grown|fruit)_se", "çalı ve bodur bitki", 35, 70, 24),
    (r"^iso_item_(?!child_)\w*(chair|throne|armchair)\w*_s[ew]", "sandalye (sırt dahil)", 46, 66, 24),
    (r"^iso_item_child_chair", "çocuk sandalyesi", 28, 42, 24),
    (r"^iso_item_\w*stool\w*_se", "tabure", 22, 38, 24),
    (r"^iso_animal_(cat)", "kedi", 16, 24, None),
    (r"^iso_animal_dog_lapdog", "küçük köpek", 18, 26, None),
    (r"^iso_animal_dog", "köpek", 24, 36, None),
    (r"^iso_animal_(chicken|hen|rooster)", "tavuk", 15, 22, None),
    (r"^iso_animal_(duck|goose)", "kaz, ördek", 17, 26, None),
    (r"^iso_animal_(pigeon)|^iso_wild_(gull|crow|sparrow)", "küçük kuş", 9, 16, None),
    (r"^iso_animal_rabbit|^iso_wild_hare", "tavşan", 11, 17, None),
    (r"^iso_animal_sheep", "koyun", 30, 40, None),
    (r"^iso_animal_goat", "keçi", 32, 42, None),
    (r"^iso_animal_pig", "domuz", 28, 38, None),
    (r"^iso_animal_(ox|cow)", "sığır", 56, 68, None),
    (r"^iso_animal_(horse|donkey|mule)", "at, eşek", 60, 82, None),
    (r"^iso_wild_bear", "ayı", 44, 58, None),
    (r"^iso_wild_wolf", "kurt", 28, 36, None),
    (r"^iso_wild_fox", "tilki", 19, 26, None),
    (r"^iso_wild_(roe_deer|deer)", "karaca", 38, 48, None),
    (r"^iso_wild_boar", "yaban domuzu", 28, 38, None),
]


def classify(name):
    for pat, label, lo, hi, piv in TABLE:
        if re.search(pat, name):
            return label, lo, hi, piv
    return None


def above_pivot(path, pivot):
    im = Image.open(path).convert("RGBA")
    box = im.getchannel("A").point(lambda v: 255 if v > 10 else 0).getbbox()
    if not box:
        return 0
    top_from_bottom = im.size[1] - box[1]
    if pivot is None:
        return box[3] - box[1]
    return top_from_bottom - pivot


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+")
    ap.add_argument("--pivot", type=float, default=None)
    a = ap.parse_args()
    bad = 0
    for f in a.files:
        name = os.path.splitext(os.path.basename(f))[0]
        c = classify(name)
        if not c:
            print(f"sınıf yok  {name}")
            continue
        label, lo, hi, piv = c
        h = above_pivot(f, a.pivot if a.pivot is not None else piv)
        ok = lo <= h <= hi
        bad += not ok
        print(f"{'TAMAM' if ok else 'KALDI'}  {name}: {h:.0f} px ({label}: {lo}-{hi})")
    sys.exit(1 if bad else 0)


if __name__ == "__main__":
    main()
