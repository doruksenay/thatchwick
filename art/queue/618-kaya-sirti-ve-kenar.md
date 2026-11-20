---
id: "618k"
title: "Kaya sırtı parçaları ve kenar dekoru (10 eşya)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("dediklerini uygula tamamen"; belge 64a §4 Aşama A madde 6)
priority: 10        # oyun dünyası (art/budget.md Öncelik); tester raporu 66
family: esya
style_gate: false
cost_est: 12
cost_max: 18
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_terrain_ridge_straight_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_terrain_ridge_corner_se.png
  - Assets/Art/Iso/_aday/iso_terrain_ridge_end_se.png
  - Assets/Art/Iso/_aday/iso_terrain_ridge_tall_straight_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_terrain_ridge_tall_corner_se.png
  - Assets/Art/Iso/_aday/iso_terrain_boulder_cluster_se.png
  - Assets/Art/Iso/_aday/iso_terrain_boulder_flat_se.png
  - Assets/Art/Iso/_aday/iso_terrain_boulder_mossy_se.png
  - Assets/Art/Iso/_aday/iso_terrain_root_mound_se.png
  - Assets/Art/Iso/_aday/iso_terrain_fallen_log_short_se.png
created: 2026-11-20
---
# 618 Kaya sırtı ve kenar dekoru

## Amaç
Belge 64 §7 (gerçek yükseklik yok, 64a K2): tepe ve yamaçlarda geçilmez kaya sırtları yükseklik hissi versin; kenar
bandında ağaç öbeklerinin arasına kaya ve kök. Kod: taslaktaki `rock` karoları bu eşyalarla dolar (`OutdoorBuilder.EdgeBand`).

## Tarif
Yöntem 357 ile aynı (`create_map_object`, stil eşleme; referans `iso_terrain_boulder_se`, `iso_terrain_boulder_large_2x1_se_nw`).
Ortak ek: `, weathered grey limestone, patches of moss and lichen, high top-down isometric view, light from top-left, single dark outline, transparent background`

| # | Dosya | Ayak izi | Tuval | Açıklama |
|---|---|---|---|---|
| 1 | ridge_straight_2x1_se_nw | 2x1 | 112x96 | `low rocky ridge segment, layered rock shelf about knee high` |
| 2 | ridge_corner_se | 1x1 | 80x96 | `corner piece of the same low rocky ridge, turning 90 degrees` |
| 3 | ridge_end_se | 1x1 | 80x88 | `tapering end of the low rocky ridge, crumbling into stones` |
| 4 | ridge_tall_straight_2x1_se_nw | 2x1 | 112x128 | `tall rocky cliff ridge segment, chest high, vertical cracks` |
| 5 | ridge_tall_corner_se | 1x1 | 80x128 | `corner of the tall rocky cliff ridge` |
| 6 | boulder_cluster_se | 1x1 | 80x72 | `cluster of three small boulders with grass between` |
| 7 | boulder_flat_se | 1x1 | 80x56 | `flat wide slab of rock half sunk in the ground` |
| 8 | boulder_mossy_se | 1x1 | 80x72 | `rounded boulder heavily covered with moss and ferns` |
| 9 | root_mound_se | 1x1 | 80x64 | `mound of exposed old tree roots and earth` |
| 10 | fallen_log_short_se | 1x1 | 80x64 | `short mossy log stump lying on its side` |

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Sırt parçaları yan yana konunca kesintisiz bir sırt oluşmalı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-21: çift iş numarası "618k" yapıldı (tester raporu 66). -->
