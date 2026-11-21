---
id: "830"
title: "Saplı demir sac (_v3) ve bulaşık sepetinin kirli ve temiz katmanları (3)"
status: running
claimed_at: 2026-11-21T00:05Z
approved_by: user        # kullanıcı 21.11.2026: 795 "kabul + sac ve sepetler yeniden" (seçmeli liste)
priority: 5
family: esya
style_gate: false
cost_est: 3
cost_max: 8
depends_on: ["795", "797"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_griddle_iron_se_v3.png
  - Assets/Art/Iso/_aday/iso_item_dish_basket_load_dirty.png
  - Assets/Art/Iso/_aday/iso_item_dish_basket_load_clean.png
  - art/results/830-*.md
created: 2026-11-21
---
# 830 Saplı sac ve bulaşık sepeti katmanları (3)

## Amaç
795'te sac sapsız düz gri disk çıktı; kirli ve temiz sepet iki kez teslim edilemedi (model sepeti yeniden çizip bozuyor). Sepette 797'de tutan **katman yöntemi** kullanılır: oyun kabul edilmiş boş sepeti (`iso_item_dish_basket_empty_se.png`) çizer, üstüne kirli ya da temiz bulaşık katmanını koyar.

## Tarif
**Sac (`iso_item_griddle_iron_se_v3.png`):** 795 ile aynı yöntem ve ölçü (48x40, pivot 24/12). Açıklama: `round flat wrought iron griddle with a clearly visible long straight iron handle sticking out to one side, slightly raised rim, dark grey metal with warm brown seasoning, lying flat`. 795'teki `_v2` sapsızdı: sap tek başına okunmalı.

**Sepet katmanları:** 797'deki yöntem aynen: arka plan = boş sepet görseli, maske = sepetin iç ağzı, açıklama yalnızca yükü ister; çıktı yalnızca yük pikselleri, sepetle aynı tuval ve (0,0) konumu.

| Dosya | Yük |
|---|---|
| `iso_item_dish_basket_load_dirty.png` | `only a pile of used wooden plates, clay bowls and a small sooty pot inside the basket, smeared with food` |
| `iso_item_dish_basket_load_clean.png` | `only neatly stacked clean wet wooden plates and clay bowls inside the basket, a folded linen cloth on top` |

## Doğrulama
`validate_iso.py`, `style.py check --family esya` (sac; sepet + katman birleşik hâli). Katmanda sepet pikseli kalmamalı. Yan yana sayfası: boş sepet, sepet+kirli, sepet+temiz, sac `_v2` ve `_v3` (`art/results/830-yanyana.png`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
