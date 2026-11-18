---
id: "333"
title: Zemin stilleri: çam tahta, meşe tahta, balıksırtı parke
status: running
claimed_at: 2026-11-18T08:42Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: zemin
style_gate: false
cost_est: 15
cost_max: 20
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_floor_pine_a.png
  - Assets/Art/Iso/iso_floor_pine_b.png
  - Assets/Art/Iso/iso_floor_oak_a.png
  - Assets/Art/Iso/iso_floor_oak_b.png
  - Assets/Art/Iso/iso_floor_herringbone_a.png
  - Assets/Art/Iso/iso_floor_herringbone_b.png
created: 2026-11-17
---
# 333 Zemin stilleri: ahşap

## Amaç
`kind: floor` parçaları (`floorBase: Wood`). Oyun `iso_floor_<ad>_a/_b` adlarını bulur (`IsoArt.FloorPart`, parça kimliğindeki `floor_` atılır).

## Tarif
- Yöntem **002 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları; 64x32 elmas, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.
- Açıklamalar: çam = light pine floor planks running diagonally along the tile; meşe = darker oak planks; balıksırtı = herringbone parquet.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`, 8x8 döşeme testi (dikiş parlaklık oranı rapora).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
