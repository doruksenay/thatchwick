---
id: "421k"
title: "Kahve zinciri ürün ikonları (10)"
status: running
claimed_at: 2026-11-19T07:49Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 32 (kullanıcı 18.11.2026 kahve mekaniği)
family: arayuz
style_gate: false
cost_est: 10
cost_max: 20
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_sapling_coffee.png
  - Assets/Art/Iso/_aday/icon_item_coffee_cherry.png
  - Assets/Art/Iso/_aday/icon_item_coffee_dried.png
  - Assets/Art/Iso/_aday/icon_item_coffee_green.png
  - Assets/Art/Iso/_aday/icon_item_coffee_roasted.png
  - Assets/Art/Iso/_aday/icon_item_coffee_roasted_fine.png
  - Assets/Art/Iso/_aday/icon_item_coffee_ground.png
  - Assets/Art/Iso/_aday/icon_item_coffee_ground_fine.png
  - Assets/Art/Iso/_aday/icon_item_coffee.png
  - Assets/Art/Iso/_aday/icon_item_cezve.png
created: 2026-11-19
---
# 421k Kahve zinciri ürün ikonları (10)

## Amaç
Kahve zinciri `items.json`'a eklendi (belge 32): fidan, kiraz, kuru kiraz, yeşil çekirdek, kavrulmuş (iki kalite), toz (iki kalite), süzme kahve, cezve. Envanter, pazar ve tarif listesinde ikon gerekir; `IsoArt.ItemIcon` `icon_item_<id>` adını kendiliğinden bulur.

## Yöntem
419a ile aynı (316 stili): 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk. Pixen yüz üretirse `create_map_object` 32x32. Her açıklamanın sonuna `, simple still-life object, no face, no eyes, no creature, no person`. Cam yok (kâse ve fincan pişmiş toprak).

## Tarif
| items.json id | Dosya (`_aday/`) | Açıklama |
|---|---|---|
| `item.sapling_coffee` | `icon_item_sapling_coffee.png` | `small coffee sapling with glossy dark green leaves in a clod of earth` |
| `item.coffee_cherry` | `icon_item_coffee_cherry.png` | `handful of ripe red coffee cherries` |
| `item.coffee_dried` | `icon_item_coffee_dried.png` | `small pile of dried shrivelled dark brown coffee cherries` |
| `item.coffee_green` | `icon_item_coffee_green.png` | `small pile of pale green raw coffee beans` |
| `item.coffee_roasted` | `icon_item_coffee_roasted.png` | `small pile of medium brown roasted coffee beans` |
| `item.coffee_roasted_fine` | `icon_item_coffee_roasted_fine.png` | `small pile of dark glossy roasted coffee beans` |
| `item.coffee_ground` | `icon_item_coffee_ground.png` | `small cloth pouch of brown ground coffee` |
| `item.coffee_ground_fine` | `icon_item_coffee_ground_fine.png` | `small clay jar of fine dark ground coffee` |
| `item.coffee` | `icon_item_coffee.png` | `simple clay cup of dark brewed coffee` |
| `item.cezve` | `icon_item_cezve.png` | `small long-handled metal coffee pot (cezve)` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
