---
id: "364"
title: İşleme yapıları: kömür yığını, kavurma yığını, çiçek ocağı, kireç ocağı
status: running
claimed_at: 2026-11-18T13:46Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 28 (18.11.2026, "tek seferde yapalım")
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_charcoal_clamp_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_roasting_heap_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_bloomery_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_lime_kiln_2x2_se.png
created: 2026-11-18
---
# 364 İşleme yapıları: kömür yığını, kavurma yığını, çiçek ocağı, kireç ocağı

## Amaç
23. belge 8 (işleme zincirleri): `item.charcoal_clamp`, `item.roasting_heap`, `item.bloomery`, `item.lime_kiln` (hepsi 2x2, dış mekân, Köylü).

## Tarif
Yöntem 301 ile aynı; 2x2 kılavuz `--n 2 --axis both`, tuval 144x112. Dumanlı/ateşli olanlarda `--light`.
- kömür yığını: `medieval charcoal clamp: dome-shaped mound of logs covered with earth and turf, thin smoke wisps`
- kavurma yığını: `low open roasting heap of reddish iron ore stones mixed with burning logs on a stone base`
- çiçek ocağı: `small medieval clay bloomery furnace, waist-high chimney shape, leather bellows beside it`
- kireç ocağı: `round stone lime kiln with an arched opening at the base, white lime dust around`

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
