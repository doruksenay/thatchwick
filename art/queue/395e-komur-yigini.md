---
id: "395e"
title: Kömür yığını, altıncı deneme (395d aday 2 temel)
status: accepted
claimed_at: 2026-11-19T12:10Z
approved_by: user        # kullanıcı 19.11.2026: "uygula dediklerini"
priority: 20
family: esya
style_gate: false
cost_est: 8
cost_max: 15
depends_on: ["364"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_charcoal_clamp_f_2x2_se.png
  - art/results/395e-komur-yigini.md
created: 2026-11-19
---
# 395e Kömür yığını

## Amaç
395d'de şekil çözüldü (kubbe), ama çim neon yeşil ve palet uyumu %52-75 kaldı. Aday 2 (`art/results/395d-red/clamp2.png`) şekilde en iyisi; zemin yaması da vardı.

## Tarif
1. Önce ücretsiz yol: aday 2 üzerinde yerel işlem. Zemin yaması maskelenip silinir. Yeşiller 364 kireç ocağının (`_aday/iso_item_lime_kiln_2x2_se.png`) çim/yosun tonlarına en yakın renkle eşlenir. Sonra `reduce_colors` 40 ve `fit --family esya`. `style.py check` GEÇTİ ya da UYARI ise bu teslim edilir, üretim yapılmaz.
2. Olmazsa üretim: `create_map_object`, arka plan görseli yok, 4 aday. Metin 395d'deki gibi, ama `muted dull olive and brown turf, earthy desaturated colors, no bright green, no ground patch under the mound` eklenir. Aynı yerel renk eşlemesi uygulanır.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Yan yana sayfası: sonuç, 395d aday 2 ve kireç ocağı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
