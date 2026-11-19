---
id: "422k"
title: "Kahve çalısı evreleri ve cezve (iso)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 32
family: esya
style_gate: false
cost_est: 6
cost_max: 12
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_plant_coffee_young_se.png
  - Assets/Art/Iso/_aday/iso_plant_coffee_grown_se.png
  - Assets/Art/Iso/_aday/iso_plant_coffee_fruit_se.png
  - Assets/Art/Iso/_aday/iso_item_cezve_se.png
created: 2026-11-19
---
# 422k Kahve çalısı evreleri ve cezve (iso)

## Amaç
`item.bush_coffee` ılık serada büyüyen kalıcı çalı (belge 32-3): genç, olgun ve kirazlı üç evre. `item.cezve` mutfakta konan küçük eşya (pişir, iç, fala bak).

## Tarif
- Çalı evreleri: 371-374 ağaç evreleri ile aynı ölçek ve yöntem (1x1 karo, `esya` ailesi, dibinde toprak). Açıklamalar:
  - genç: `young coffee shrub, small, glossy dark green paired leaves`
  - olgun: `waist-high coffee shrub, dense glossy dark green leaves`
  - kirazlı: `waist-high coffee shrub with clusters of ripe red coffee cherries along the branches`
- Cezve: 1x1 masa üstü eşyası, 302 ölçeği. `small long-handled copper coffee pot (cezve) on a low stone hearth brick`.
- Her açıklamanın sonuna `, no face, no eyes, no creature, no person`.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`.

## Notlar
- Kod eşlemesi (`IsoArt.Plants` evreleri ve `IsoArt.Items` cezve) görseller gelince kod oturumunun işi.
- Dönem: kahve belge 20'de bilinçli istisna.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
