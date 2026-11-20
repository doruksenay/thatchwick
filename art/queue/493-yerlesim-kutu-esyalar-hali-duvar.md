---
id: "493"
title: "Yerleşim: kutu çizilen halı, masa üstü, duvar ve tavan eşyaları (10)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); kullanıcı 20.11.2026 "ne eksik ... dükkan dekorasyonları ... doğal duruyor mu" (yerleşim raporu: kutu çizilen eşyalar)
priority: 15
family: esya
style_gate: false
cost_est: 12
cost_max: 24
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_irrigation_ditch_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_rush_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_wool_small_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_wool_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_morris_board_se.png
  - Assets/Art/Iso/_aday/iso_item_book_se.png
  - Assets/Art/Iso/_aday/iso_item_glass_bottle_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_tapestry_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_chisel_rack_sw_ne.png
  - Assets/Art/Iso/_aday/iso_deco_ceiling_herbs_se.png
created: 2026-11-20
---
# 493 Yerleşim: kutu çizilen halı, masa üstü, duvar ve tavan eşyaları (10)

## Amaç
Yerleşim raporu (`tools/layout-report`, `noart`): yeni oyunun dünyasında bu eşyalar dükkânlarda ve evlerde kutu (yer tutucu) olarak çiziliyor. Ya hiç işleri yoktu ya da önceki işlerinde (302, 303, 304, 321, 337, 338, 390, 400, 409, 421, 424k) teslim edilmediler. `items.json` eşlemesi hazır (`pending: true`); dosya gelince kod görseli kendiliğinden kullanır.

## Tarif
Halılar **337 ile aynı yöntem** (düz, gölgesiz, ayak izi elmasına oturur). Masa üstü **383 ile aynı yöntem** (yuva K). Duvar eşyaları **306 ile aynı yöntem** (`iso_wall_item_*_sw_ne`, duvara asılı, pivot duvar yüzünde). Tavan **313 fener satırıyla aynı yöntem** (`iso_deco_*`). Sulama hendeği 321 yöntemiyle zemin üstü.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.irrigation_ditch` | `iso_item_irrigation_ditch_se.png` | 1x1 zemin | 64x40 | 32, 16 | `short straight irrigation ditch cut in earth with a little running water` |
| 2 | `item.rug_rush` | `iso_item_rug_rush_se.png` | 1x1 halı | 64x32 | 32, 16 | `small woven rush mat, straw yellow` |
| 3 | `item.rug_wool_small` | `iso_item_rug_wool_small_2x1_se.png` | 2x1 halı | 96x48 | 48, 24 | `small wool rug, red and ochre stripes` |
| 4 | `item.rug_wool` | `iso_item_rug_wool_2x2_se.png` | 2x2 halı | 128x64 | 64, 32 | `square wool rug with a simple border, madder red and indigo` |
| 5 | `item.morris_board` | `iso_item_morris_board_se.png` | yuva K | 32x24 | 16, 8 | `small wooden nine men's morris board with a few pegs` |
| 6 | `item.book` | `iso_item_book_se.png` | yuva K | 32x24 | 16, 8 | `single closed leather-bound book with brass corners` |
| 7 | `item.glass_bottle` | `iso_item_glass_bottle_se.png` | yuva K | 16x32 | 8, 6 | `small greenish forest-glass bottle with a cork` |
| 8 | `item.tapestry` | `iso_wall_item_tapestry_sw_ne.png` | duvar | 48x64 | 24, 32 | `woven wall hanging with a hunting scene, muted red, green and ochre` |
| 9 | `item.chisel_rack` | `iso_wall_item_chisel_rack_sw_ne.png` | duvar | 48x48 | 24, 24 | `small wooden wall rack holding stonemason chisels and a mallet` |
| 10 | `item.ceiling_herbs` | `iso_deco_ceiling_herbs_se.png` | tavan | 64x48 | 32, 24 | `bundles of dried herbs hanging from a ceiling beam` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...` (`--max-colors 48`, ayak izi kutusu), `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/493-yanyana.png`: yerleştiği dükkân ve evdeki komşu eşyalarla (raf, tezgâh, fıçı) aynı satırda. Dönem kuralları (CLAUDE.md, belge 20).

## Notlar
- Önceki denemelerde kalma nedeni çoğunlukla palet ve doygunluktu (303 raporu: tezgâh 57 renk, x1,54 doygun; su fıçısı %70 palet dışı). Açıklamaya `earthy muted medieval palette, desaturated` eki konur.
- Dosya adları `items.json`'daki eşlemeyle aynı olmalı; teslimde `cp -n`, `.meta` yok. Teslim edilen eşyanın `pending` alanını işçi değil kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
