---
id: "433"
title: "Yerleşim: oturma eşyalarının arkası dönük görünümü (12)"
status: pending
approved_by: pending     # yerleşim görevi (belge 44): kullanıcı onayı bekler
family: esya
style_gate: false
cost_est: 12
cost_max: 20
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_chair_wood_ne.png
  - Assets/Art/Iso/_aday/iso_item_chair_carved_ne.png
  - Assets/Art/Iso/_aday/iso_item_chair_painted_se.png
  - Assets/Art/Iso/_aday/iso_item_chair_painted_ne.png
  - Assets/Art/Iso/_aday/iso_item_armchair_cushion_ne.png
  - Assets/Art/Iso/_aday/iso_item_x_chair_ne.png
  - Assets/Art/Iso/_aday/iso_item_bench_long_2x1_ne.png
  - Assets/Art/Iso/_aday/iso_item_han_bench_2x1_ne.png
  - Assets/Art/Iso/_aday/iso_item_garden_bench_2x1_ne.png
  - Assets/Art/Iso/_aday/iso_item_settle_bench_2x1_ne.png
  - Assets/Art/Iso/_aday/iso_item_chest_bench_2x1_ne.png
  - Assets/Art/Iso/_aday/iso_item_bleached_bench_2x1_ne.png
created: 2026-11-19
---
# 433 Yerleşim: oturma eşyalarının arkası dönük görünümü

## Amaç
Belge 44 açık kararı: arkası dönük sandalye ve bank görseli yok. Görsel gelene kadar şablonlar oturma eşyasını yalnızca kameraya
bakan taraflara (masanın doğu ve güneyi) koyuyor; yapı modunda Döndür arkası dönük yönleri atlıyor. Bu iş her oturma eşyası için
**arkası dönük** (`_ne`, mantıksal E; aynası `_nw` = S) görünümü üretir; sonra masanın dört yanına da oturulabilir.

## Tarif
Var olan ön görselin (aynı dosya adı, `_ne` yerine `_se` ya da `_sw`) stil ve ölçüsüyle aynı eşya, arkadan: sırtlık ve arka ayaklar
öne gelir, oturak arkada kalır. 301 yöntemi, ilgili ön görsel stil referansı.

- `iso_item_chair_wood_ne.png` ← ön görsel: `iso_item_chair_wood_sw`
- `iso_item_chair_carved_ne.png` ← ön görsel: `iso_item_chair_carved_se`
- `iso_item_chair_painted_se.png` ve `iso_item_chair_painted_ne.png`: boyalı sandalyenin ön görseli de yok; önce ön (`_se`), sonra arkası
- `iso_item_armchair_cushion_ne.png` ← ön görsel: `iso_item_armchair_cushion_se`
- `iso_item_x_chair_ne.png` ← ön görsel: `iso_item_x_chair_se`
- `iso_item_bench_long_2x1_ne.png` ← ön görsel: `iso_item_bench_long_2x1_se_nw`
- `iso_item_han_bench_2x1_ne.png` ← ön görsel: `iso_item_han_bench_2x1_se`
- `iso_item_garden_bench_2x1_ne.png` ← ön görsel: `iso_item_garden_bench_2x1_se_nw`
- `iso_item_settle_bench_2x1_ne.png` ← ön görsel: `iso_item_settle_bench_2x1_se`
- `iso_item_chest_bench_2x1_ne.png` ← ön görsel: `iso_item_chest_bench_2x1_se`
- `iso_item_bleached_bench_2x1_ne.png` ← ön görsel: `iso_item_bleached_bench_2x1_se`

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Ön görselle yan yana: aynı boy, aynı palet, pivot aynı.

## Notlar
- Eşleme görseller gelince: `items.json` → `art.e` (ve simetrikse ayna S'yi verir). Sonra şablonlarda `Facing.FacesCamera`
  kısıtı oturma için kaldırılabilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
