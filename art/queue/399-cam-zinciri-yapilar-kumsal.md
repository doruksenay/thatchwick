---
id: "399c"
title: "Cam zinciri yapıları ve kumsal: cam ocağı, camcı tezgâhı, kalbur, kumsal (4)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 27 §18 (18.11.2026, "kumsaldan kum alınıp işlensin")
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301", "364"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_glass_furnace_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_glass_bench_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_sieve_se.png
  - Assets/Art/Iso/_aday/iso_item_forage_sand_se.png
created: 2026-11-18
---
# 399 Cam zinciri yapıları ve kumsal (4)

## Amaç
Ev dekorasyonu D3, cam zinciri (Project belgesi `27-ev-dekorasyonu` §18): `item.glass_furnace` (2x2, dış, Zanaatkâr), `item.glass_bench` (2x1, Zanaatkâr), `item.sieve` (1x1, Köylü), `item.forage_sand` (suyun kenarında beliren kum yığını, toplama düğümü). Kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Yöntem 301 ile aynı; 2x2 kılavuz `--n 2 --axis both` (tuval 144x112), 2x1 kılavuz `--n 2`. Ateşli olanda `--light`. Dönem: orman camı (yeşilimsi), ortaçağ cam evi; kömürle yanar.
- cam ocağı: `small medieval forest glass furnace: domed clay and stone kiln with glowing working holes, a heap of charcoal beside it, faint smoke`
- camcı tezgâhı: `glassblower's wooden workbench with long iron blowpipes, tongs and a few greenish glass vessels cooling on a board`
- kalbur: `round wooden sieve with a woven mesh resting on a low wooden stand, small heap of fine sand underneath`
- kumsal: `small patch of pale lake-shore sand with a few pebbles and a freshwater mussel shell, reeds at one edge` (toplama düğümü; 311/322 toplama düğümleri ölçeğinde)

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-21: çift iş numarası "399c" yapıldı (tester raporu 66). -->
