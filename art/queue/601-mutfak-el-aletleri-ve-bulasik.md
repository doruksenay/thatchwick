---
id: "601"
title: "Mutfak, el aletleri ve bulaşık: bıçak, tahta, kaşık, oklava, kevgir, kürek, bulaşık sepeti, leğen, yıkama taşı (11)"
status: running
claimed_at: 2026-11-20T15:22Z
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
  - Assets/Art/Iso/_aday/iso_item_kitchen_knife_se.png
  - Assets/Art/Iso/_aday/iso_item_cutting_board_se.png
  - Assets/Art/Iso/_aday/iso_item_wooden_spoon_se.png
  - Assets/Art/Iso/_aday/iso_item_rolling_board_se.png
  - Assets/Art/Iso/_aday/iso_item_colander_se.png
  - Assets/Art/Iso/_aday/iso_item_baker_peel_se.png
  - Assets/Art/Iso/_aday/iso_item_dish_basket_empty_se.png
  - Assets/Art/Iso/_aday/iso_item_dish_basket_dirty_se.png
  - Assets/Art/Iso/_aday/iso_item_dish_basket_clean_se.png
  - Assets/Art/Iso/_aday/iso_item_dish_tub_se.png
  - Assets/Art/Iso/_aday/iso_item_washing_stone_se.png
created: 2026-11-20
---
# 601 Mutfak, el aletleri ve bulaşık: bıçak, tahta, kaşık, oklava, kevgir, kürek, bulaşık sepeti, leğen, yıkama taşı (11)

## Amaç
Gerekenler modeli (63 §2.4) ve bulaşık sistemi (63a §4): el aletleri kancada ya da rafta durur; kirli kap ve tabaklar bulaşık sepetine toplanır, derede ya da evde leğende yıkanır.

## Tarif
Eşya dalgasının yöntemi (301/302): `create_map_object` stil eşleme, `tools/art/footprint_guide.py` kılavuzu ve maskesi, aile `esya`. Masa üstü eşyalar masa üstü dekor ölçüsünde (referans: `iso_item_copper_pot_se`, `iso_item_ladle_se`, `iso_item_wooden_bowl_se`); 1x1 eşyalar eşya dalgası ölçüsünde.

| Dosya | Ayak izi | Açıklama |
|---|---|---|
| `iso_item_kitchen_knife_se.png` | masa üstü | `small medieval kitchen knife with a broad iron blade and a plain wooden handle, lying flat` |
| `iso_item_cutting_board_se.png` | masa üstü | `thick rectangular wooden chopping board with knife marks and a hanging hole in one corner` |
| `iso_item_wooden_spoon_se.png` | masa üstü | `long-handled carved wooden cooking spoon lying flat` |
| `iso_item_rolling_board_se.png` | masa üstü | `round low wooden dough board with a long thin rolling pin lying across it and a dusting of flour` |
| `iso_item_colander_se.png` | masa üstü | `round tinned copper colander bowl pierced with small holes, two side handles` |
| `iso_item_baker_peel_se.png` | 1x1 (duvara yaslı, uzun) | `long-handled flat wooden baker peel leaning upright against a wall` |
| `iso_item_dish_basket_empty_se.png` | 1x1 (küçük) | `sturdy wide woven willow carrying basket with two handles, empty` |
| `iso_item_dish_basket_dirty_se.png` | 1x1 (küçük) | `sturdy wide woven willow carrying basket piled with used wooden plates, clay bowls and a sooty iron pot` |
| `iso_item_dish_basket_clean_se.png` | 1x1 (küçük) | `sturdy wide woven willow carrying basket with neatly stacked clean wet wooden plates and clay bowls, a linen cloth on top` |
| `iso_item_dish_tub_se.png` | 1x1 | `low wide wooden washing tub bound with iron hoops, filled with grey water, a scrubbing bundle of straw on the rim` |
| `iso_item_washing_stone_se.png` | 1x1 (yere, düz) | `broad flat smooth riverside stone slab at the water edge with a wet surface and a small pile of sand beside it` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Işık kaynağı kuralı geçerli (bu işte alev yok).

## Notlar
Sepetin üç hâli aynı sepettir (aynı boyut, aynı pivot; yalnız içi değişir). Yıkama taşı dere ve göl kıyısına konur; çamaşır yıkamada da kullanılır. Eşya kimlikleri ve pivotlar kod oturumunda.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
