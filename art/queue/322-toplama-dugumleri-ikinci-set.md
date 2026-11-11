---
id: "322"
title: Toplama düğümleri ikinci set (yabani sarımsak, ısırgan, mürver, yaban çileği, kestane, kuşburnu, kozalak, su teresi)
status: pending
approved_by: pending
cost_est: 8
cost_max: 14
depends_on: ["301", "311"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_wild_garlic_se.png
  - Assets/Art/Iso/_aday/iso_forage_nettle_se.png
  - Assets/Art/Iso/_aday/iso_forage_elderflower_se.png
  - Assets/Art/Iso/_aday/iso_forage_strawberry_se.png
  - Assets/Art/Iso/_aday/iso_forage_chestnut_se.png
  - Assets/Art/Iso/_aday/iso_forage_rosehip_se.png
  - Assets/Art/Iso/_aday/iso_forage_pinecone_se.png
  - Assets/Art/Iso/_aday/iso_forage_cress_se.png
created: 2026-11-11
---
# 322 Toplama düğümleri ikinci set

## Amaç
`ForageSystem` genişledi: yağmurlu günde nem seven türler (`rain_loving`: yabani sarımsak, ısırgan) çıkar; her mevsimde en az 3 düğüm vardır. 311'deki 6 düğümün yanına 8 yeni düğüm gelir. Hepsi 1x1, topla etkileşimiyle kalkar.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

- Tuval 64x56, ayak izi 1x1 (`--n 1`), yükseklik 12-30; zemin çizilmez. Ortak ek: 311 ile aynı ("muted natural colours").
- Açıklamalar:
  1. wild_garlic (ilkbahar, 16): `patch of broad green wild-garlic leaves with small white star-shaped flowers`
  2. nettle (ilkbahar-yaz, 20): `dense clump of serrated dark-green stinging nettles`
  3. elderflower (yaz, 30): `small elder shrub with flat white flower clusters`
  4. strawberry (yaz, 12): `low patch of wild strawberry plants with a few small red berries`
  5. chestnut (sonbahar, 18): `a few spiky green-brown chestnut burs and shiny brown chestnuts on fallen leaves`
  6. rosehip (sonbahar-kış, 26): `thorny rose bush with bright orange-red rosehips, few leaves`
  7. pinecone (kış, 10): `three brown pinecones lying on pine needles and a little snow`
  8. cress (ilkbahar-sonbahar, 8): `small clump of bright green watercress at a puddle edge`
- Maliyet 8 × 1 gen. İkinci deneme payı 6 → `cost_max` 14.

## Doğrulama
301 doğrulama tablosu. Ek: 14 düğümün (311 + bu iş) yan yana ve çimen karosu ile görünümü; kış düğümleri (kuşburnu, kozalak) karlı zeminde seçilebiliyor mu; hiçbiri ağaçtan büyük değil.

## Notlar
- Kış düğümleri için karlı zemin kabul edilmediyse çimen/toprak zemini üzerinde doğrulanır; kar kaplaması ayrı iş.
