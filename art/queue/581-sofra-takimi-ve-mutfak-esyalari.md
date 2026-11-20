---
id: "581"
title: "Sofra takımı, baharat rafı, salça tepsisi, piknik ve pazar tezgâhı (11)"
status: review
claimed_at: 2026-11-20T15:04Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 30
family: esya
style_gate: false
cost_est: 11
cost_max: 22
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_plate_wood_full.png
  - Assets/Art/Iso/_aday/iso_item_plate_wood_empty.png
  - Assets/Art/Iso/_aday/iso_item_bowl_clay_full.png
  - Assets/Art/Iso/_aday/iso_item_bowl_clay_empty.png
  - Assets/Art/Iso/_aday/iso_item_cup_wood.png
  - Assets/Art/Iso/_aday/iso_item_dirty_dishes.png
  - Assets/Art/Iso/_aday/iso_item_spice_rack_se.png
  - Assets/Art/Iso/_aday/iso_item_paste_tray_se.png
  - Assets/Art/Iso/_aday/iso_item_picnic_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_picnic_cloth_se.png
  - Assets/Art/Iso/_aday/iso_item_food_stall_se.png
created: 2026-11-20
---
# 581 Sofra takımı, baharat rafı, salça tepsisi, piknik ve pazar tezgâhı (11)

## Amaç
Sofra ve servis (58 §3), bulaşık (58-S6), baharat rafı ve salça (58a §2-3), piknik (58 §10), pazar tezgâhında yemek (60b §5.7).

## Tarif
Eşya dalgasının yöntemi (301/302): `create_map_object` stil eşleme, `tools/art/footprint_guide.py` kılavuzu ve maskesi, aile `esya`. Masa üstü eşyalar masa üstü dekor ölçüsünde (meyve sepeti `iso_item_fruit_basket`), duvar eşyası duvar eşyası yönteminde.

| Dosya | Ayak izi | Açıklama |
|---|---|---|
| `iso_item_plate_wood_full.png` | masa üstü | `small round wooden plate with a portion of food` |
| `iso_item_plate_wood_empty.png` | masa üstü | `small round wooden plate, empty, with a few crumbs` |
| `iso_item_bowl_clay_full.png` | masa üstü | `small clay bowl filled with soup and a wooden spoon` |
| `iso_item_bowl_clay_empty.png` | masa üstü | `small empty clay bowl with a wooden spoon resting in it` |
| `iso_item_cup_wood.png` | masa üstü | `small turned wooden drinking cup` |
| `iso_item_dirty_dishes.png` | masa üstü | `short untidy stack of used wooden plates and clay bowls with spoons` |
| `iso_item_spice_rack_se.png` | duvar | `narrow wall shelf with a row of small cloth spice pouches and three little clay jars, a bunch of dried herbs hanging below` |
| `iso_item_paste_tray_se.png` | 1x1 | `wide shallow wooden tray on low trestles spread with thick dark red paste drying in the sun` |
| `iso_item_picnic_basket_se.png` | 1x1 (küçük) | `lidded woven willow basket with a folded linen cloth and a loaf peeking out` |
| `iso_item_picnic_cloth_se.png` | 2x2 (yere serili, düz) | `square striped linen cloth spread flat on the ground with a basket, a loaf, cheese and a jug laid out` |
| `iso_item_food_stall_se.png` | 2x1 | `small wooden market stall with a striped cloth awning, a counter with covered clay pots and stacked wooden bowls` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Işık kaynağı kuralı geçerli.

## Notlar
Eşya kimlikleri kod oturumunda bu dosya adlarıyla açılır; pivotlar `IsoPivots.txt`'e kod oturumunda yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
