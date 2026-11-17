---
id: "334"
title: Zemin stilleri: yassı taş, arnavut kaldırımı, tuğla, sırlı çini
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (Sims düzeyinde yapı modu, 2026-11-17); "user" yapınca işçi alır
family: zemin
style_gate: false
cost_est: 20
cost_max: 25
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_floor_flag_a.png
  - Assets/Art/Iso/iso_floor_flag_b.png
  - Assets/Art/Iso/iso_floor_cobble_a.png
  - Assets/Art/Iso/iso_floor_cobble_b.png
  - Assets/Art/Iso/iso_floor_brick_a.png
  - Assets/Art/Iso/iso_floor_brick_b.png
  - Assets/Art/Iso/iso_floor_tile_a.png
  - Assets/Art/Iso/iso_floor_tile_b.png
created: 2026-11-17
---
# 334 Zemin stilleri: taş

## Amaç
`floorBase: Stone` parçaları. Yassı taş varsayılandır.

## Tarif
- Yöntem **002 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları; 64x32 elmas, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.
- Açıklamalar: yassı taş = irregular grey flagstones; arnavut = rounded cobblestones; tuğla = red brick floor in running bond; sırlı çini = glazed teal and cream ceramic floor tiles.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`, 8x8 döşeme testi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
