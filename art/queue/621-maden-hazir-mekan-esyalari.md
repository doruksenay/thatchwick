---
id: "621"
title: "Maden hazır mekân eşyaları (7 eşya)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("dediklerini uygula tamamen"; belge 64a §4 Aşama A madde 6)
family: esya
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["362"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_rotten_prop_se.png
  - Assets/Art/Iso/_aday/iso_item_old_mine_chest_se.png
  - Assets/Art/Iso/_aday/iso_item_broken_barrow_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_crystal_cluster_se.png
  - Assets/Art/Iso/_aday/iso_item_crystal_cluster_small_se.png
  - Assets/Art/Iso/_aday/iso_item_pool_edge_stones_se.png
  - Assets/Art/Iso/_aday/iso_item_old_lamp_niche_se.png
created: 2026-11-20
---
# 621 Maden hazır mekân eşyaları

## Amaç
Belge 64 §6, 58 M4: üreteç v2'nin hazır mekânları (eski galeri, yer altı gölcüğü, çökmüş geçit, kemerli oda) ve doğal
kovuklar. Bugün yalnızca kaya ve destek durumuyla gösteriliyor; bu eşyalar mekânı okunur kılar (M4'te bağlanır).

## Tarif
362 yöntemi ve referansları. Ortak ek: `, inside a dark mine gallery, lit warmly from a lamp at top-left, high top-down isometric view, transparent background`

| # | Dosya | Ayak izi | Tuval | Açıklama |
|---|---|---|---|---|
| 1 | rotten_prop_se | 1x1 | 80x112 | `old rotten wooden mine prop, cracked and sagging` |
| 2 | old_mine_chest_se | 1x1 | 80x72 | `forgotten small iron-banded chest covered in dust` |
| 3 | broken_barrow_2x1_se_nw | 2x1 | 112x72 | `broken old wooden wheelbarrow tipped over with spilled ore` |
| 4 | crystal_cluster_se | 1x1 | 80x96 | `cluster of pale quartz crystals growing from rock, faint glow` |
| 5 | crystal_cluster_small_se | 1x1 | 80x64 | `small group of quartz crystals` |
| 6 | pool_edge_stones_se | 1x1 | 80x56 | `wet stones at the edge of an underground pool` |
| 7 | old_lamp_niche_se | 1x1 | 80x96 | `small niche carved in rock with an old clay oil lamp` |

## Doğrulama
`validate_iso.py`, `style.py check --family esya --light` (4 ve 7).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
