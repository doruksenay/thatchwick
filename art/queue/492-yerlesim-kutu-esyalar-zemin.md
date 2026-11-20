---
id: "492"
title: "Yerleşim: kutu çizilen yer eşyaları (14)"
status: running
claimed_at: 2026-11-20T10:25Z
approved_by: user        # otomatik akış (art/budget.md); kullanıcı 20.11.2026 "ne eksik ... dükkan dekorasyonları ... doğal duruyor mu" (yerleşim raporu: kutu çizilen eşyalar)
priority: 15
family: esya
style_gate: false
cost_est: 16
cost_max: 30
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_water_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_sini_table_se.png
  - Assets/Art/Iso/_aday/iso_item_copper_pots_se.png
  - Assets/Art/Iso/_aday/iso_item_firewood_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_brine_crock_se.png
  - Assets/Art/Iso/_aday/iso_item_barrel_table_se.png
  - Assets/Art/Iso/_aday/iso_item_candlestick_se.png
  - Assets/Art/Iso/_aday/iso_item_water_trough_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_workbench_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_bed_frame_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_table_painted_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_pantry_shelf_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_drying_rack_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_rope_coil_se.png
created: 2026-11-20
---
# 492 Yerleşim: kutu çizilen yer eşyaları (14)

## Amaç
Yerleşim raporu (`tools/layout-report`, `noart`): yeni oyunun dünyasında bu eşyalar dükkânlarda ve evlerde kutu (yer tutucu) olarak çiziliyor. Ya hiç işleri yoktu ya da önceki işlerinde (302, 303, 304, 321, 337, 338, 390, 400, 409, 421, 424k) teslim edilmediler. `items.json` eşlemesi hazır (`pending: true`); dosya gelince kod görseli kendiliğinden kullanır.

## Tarif
**303 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. 1x1 `_se`; 2x1 `--n 2 --axis x`, `_2x1_se_nw` (303 tezgâh satırıyla aynı).

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.water_barrel` | `iso_item_water_barrel_se.png` | 1x1 | 80x72 | 40, 24 | `oak water barrel with iron hoops and a wooden lid, a dipper hanging on the rim` |
| 2 | `item.sini_table` | `iso_item_sini_table_se.png` | 1x1 | 80x64 | 40, 24 | `low round wooden tray table on short folding legs, copper tray top` |
| 3 | `item.copper_pots` | `iso_item_copper_pots_se.png` | 1x1 | 80x72 | 40, 24 | `stack of three hammered copper cauldrons of different sizes on the floor` |
| 4 | `item.firewood_basket` | `iso_item_firewood_basket_se.png` | 1x1 | 80x64 | 40, 24 | `wicker basket full of split firewood logs` |
| 5 | `item.brine_crock` | `iso_item_brine_crock_se.png` | 1x1 | 80x72 | 40, 24 | `large glazed earthenware crock with a wooden lid weighted by a stone` |
| 6 | `item.barrel_table` | `iso_item_barrel_table_se.png` | 1x1 | 80x80 | 40, 24 | `upright barrel used as a small tavern table, a clay mug on top` |
| 7 | `item.candlestick` | `iso_item_candlestick_se.png` | 1x1 | 48x88 | 24, 24 | `tall wrought-iron floor candlestick on three legs, one lit tallow candle` |
| 8 | `item.water_trough` | `iso_item_water_trough_2x1_se_nw.png` | 2x1 | 112x80 | 56, 32 | `long hollowed log water trough for animals, water inside` |
| 9 | `item.workbench` | `iso_item_workbench_2x1_se_nw.png` | 2x1 | 112x88 | 56, 32 | `carpenter workbench, thick oak top with a vise, a saw and a mallet lying on it, shavings` |
| 10 | `item.bed_frame` | `iso_item_bed_frame_2x1_se_nw.png` | 2x1 | 112x88 | 56, 32 | `plain wooden bed frame with a straw mattress and a wool blanket` |
| 11 | `item.table_painted` | `iso_item_table_painted_2x1_se_nw.png` | 2x1 | 112x80 | 56, 32 | `wooden table with painted flower patterns on the edges` |
| 12 | `item.pantry_shelf` | `iso_item_pantry_shelf_2x1_se_nw.png` | 2x1 | 112x104 | 56, 32 | `tall wooden pantry shelf with jars, cheese wheels and sacks` |
| 13 | `item.drying_rack` | `iso_item_drying_rack_2x1_se_nw.png` | 2x1 | 112x96 | 56, 32 | `wide wooden drying rack with herbs and strips of dried meat hanging` |
| 14 | `item.rope_coil` | `iso_item_rope_coil_se.png` | 1x1 | 80x56 | 40, 24 | `coil of thick hemp rope on the ground, beside a fishing pier` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...` (`--max-colors 48`, ayak izi kutusu), `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/492-yanyana.png`: yerleştiği dükkân ve evdeki komşu eşyalarla (raf, tezgâh, fıçı) aynı satırda. Dönem kuralları (CLAUDE.md, belge 20).

## Notlar
- Önceki denemelerde kalma nedeni çoğunlukla palet ve doygunluktu (303 raporu: tezgâh 57 renk, x1,54 doygun; su fıçısı %70 palet dışı). Açıklamaya `earthy muted medieval palette, desaturated` eki konur.
- Dosya adları `items.json`'daki eşlemeyle aynı olmalı; teslimde `cp -n`, `.meta` yok. Teslim edilen eşyanın `pending` alanını işçi değil kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
