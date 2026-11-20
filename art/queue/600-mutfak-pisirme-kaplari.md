---
id: "600"
title: "Mutfak, pişirme kapları: tencere, sac, tava, güveç, şiş, tepsi, sacayağı (11)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (63a-K10: "mutfak eşyaları hemen girebilir: olur"), Project belgeleri 63, 63a
priority: 30
family: esya
style_gate: false
cost_est: 11
cost_max: 22
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_pot_clay_se.png
  - Assets/Art/Iso/_aday/iso_item_griddle_iron_se.png
  - Assets/Art/Iso/_aday/iso_item_pan_copper_se.png
  - Assets/Art/Iso/_aday/iso_item_guvec_plain_se.png
  - Assets/Art/Iso/_aday/iso_item_guvec_glazed_se.png
  - Assets/Art/Iso/_aday/iso_item_guvec_fine_se.png
  - Assets/Art/Iso/_aday/iso_item_spit_iron_se.png
  - Assets/Art/Iso/_aday/iso_item_spit_steel_se.png
  - Assets/Art/Iso/_aday/iso_item_baking_tray_clay_se.png
  - Assets/Art/Iso/_aday/iso_item_baking_tray_copper_se.png
  - Assets/Art/Iso/_aday/iso_item_trivet_iron_se.png
created: 2026-11-20
---
# 600 Mutfak, pişirme kapları: tencere, sac, tava, güveç, şiş, tepsi, sacayağı (11)

## Amaç
Gerekenler modeli (Project belgeleri 63 §2, 63a §2 ve §5): pişirme kabı gerçek eşyadır; kancada, rafta ya da masada durur, karakter alıp ocağa götürür. Her kap bir pişirme yöntemini açar; kademe (K1-K3) yemek kalitesinin tavanını belirler. Demir kazan (K2) var olan `iso_item_cooking_pot_se_v3`, bakır kazan (K3) var olan `iso_item_copper_pot_se` görselini kullanır; yeniden üretilmez.

## Tarif
Eşya dalgasının yöntemi (301/302): `create_map_object` stil eşleme, `tools/art/footprint_guide.py` kılavuzu ve maskesi, aile `esya`. Masa üstü eşyalar masa üstü dekor ölçüsünde (referans: `iso_item_copper_pot_se`, `iso_item_ladle_se`, `iso_item_wooden_bowl_se`); 1x1 eşyalar eşya dalgası ölçüsünde.

| Dosya | Ayak izi | Açıklama |
|---|---|---|
| `iso_item_pot_clay_se.png` | masa üstü | `small round unglazed earthenware cooking pot with two lug handles and a clay lid, soot-darkened base` |
| `iso_item_griddle_iron_se.png` | masa üstü | `shallow round convex black iron griddle with two small ring handles` |
| `iso_item_pan_copper_se.png` | masa üstü | `shallow round tinned copper frying pan with one long flat iron handle` |
| `iso_item_guvec_plain_se.png` | masa üstü | `wide low unglazed reddish clay casserole with a domed clay lid and a knob` |
| `iso_item_guvec_glazed_se.png` | masa üstü | `wide low clay casserole with a green-brown glazed lid and rim` |
| `iso_item_guvec_fine_se.png` | masa üstü | `wide low lidded clay casserole with fine blue-and-white glazed pattern on the lid` |
| `iso_item_spit_iron_se.png` | masa üstü | `pair of low forked iron stands with two long iron roasting spits resting across them` |
| `iso_item_spit_steel_se.png` | masa üstü | `low iron grill grate on four feet with three bright steel skewers laid across it` |
| `iso_item_baking_tray_clay_se.png` | masa üstü | `round shallow unglazed clay baking dish with a low rim, empty` |
| `iso_item_baking_tray_copper_se.png` | masa üstü | `round shallow tinned copper baking tray with a rolled rim, empty` |
| `iso_item_trivet_iron_se.png` | masa üstü | `low three-legged iron trivet ring for holding a pot over embers` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Işık kaynağı kuralı geçerli (bu işte alev yok).

## Notlar
Eşya kimlikleri kod oturumunda bu dosya adlarıyla açılır; pivotlar `IsoPivots.txt`'e kod oturumunda yazılır. Aynı görsel karakterin elinde de çizilir (el çapası), bu yüzden siluet küçük ölçekte okunmalı: tek parça, ince çıkıntı yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
