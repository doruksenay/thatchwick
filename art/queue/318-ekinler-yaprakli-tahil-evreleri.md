---
id: "318"
title: Yapraklı ve tahıl ekinleri evreleri (turp, marul, bakla, arpa, ıspanak; 3 evre)
status: pending
approved_by: pending
family: bitki
style_gate: false
cost_est: 15
cost_max: 24
depends_on: ["301", "310"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_radish_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_lettuce_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_broad_bean_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_barley_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_spinach_{1,2,3}_se.png
created: 2026-11-11
---
# 318 Yapraklı ve tahıl ekinleri evreleri

## Amaç
Çiftçilik genişlemesi (v0): `item.crop_radish`, `item.crop_lettuce`, `item.crop_broad_bean`, `item.crop_barley`, `item.crop_spinach`. Kısa devirli yapraklı ekinler (turp, marul, ıspanak) ve soğuğa dayanıklı bakla ile arpa. Kuraklıkta büyüme 0,3'e düştüğü için sprite değişmez; sulanmamış ekin HUD'da işaretlenir.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

- Ayak izi 1x1, tuval 64x64, kılavuz `--n 1`, yükseklik: evre 1 → 10, evre 2 → 22, evre 3 → 32 (en çok 36). Toprak sprite'a çizilmez ("no soil"); zemin `iso_ground_dirt_*` ile gelir.
- Açıklamaya ortak ek: "muted green and earthy colours" ("dark brown wood" çıkarılır). Aynı bitkinin üç evresi aynı palet ve kamerayla; taban elması ve piksel sınır kutusu evreler arasında orta çizgide hizalı.
- Evre 1 = filiz (0-33%), 2 = büyüyor (33-99%), 3 = olgun/hasada hazır. Kış ölümü ve kuruma için ayrı sprite yok (kod `Water`/`FertilityAt` göstergesini HUD'da verir).
- Arpa 1 karoyu aşmaz ama ince ve uzundur (evre 3 yüksekliği 36). Bakla 310'daki bezelyeden farklı: çit yok, kalın dik gövde.
- Açıklamalar:
  - radish: `{1: two tiny round leaves, 2: small green leaves with a pink-red top just visible, 3: round bright red radish half out of the ground with small green leaves}`
  - lettuce: `{1: two small pale-green leaves, 2: loose frilly green rosette, 3: full round frilly light-green lettuce head}`
  - broad_bean: `{1: single sturdy green shoot, 2: upright thick stem with grey-green leaves and white flowers, 3: upright stem with fat green bean pods hanging, grey-green leaves}`
  - barley: `{1: thin grass-like green blades, 2: tall green blades with a young head, 3: tall golden stalks each ending in a bearded ear of barley}`
  - spinach: `{1: two small dark-green leaves, 2: low rosette of dark-green leaves, 3: dense low rosette of broad dark-green crinkled leaves}`
- Maliyet: 15 × 1 gen = 15. İkinci deneme payı 9 → `cost_max` 24.

## Doğrulama
301 doğrulama tablosu. Ek: 310 ve 317 ekinleri ile aynı karoda yan yana: 15 ekin tek kareye sığdırılıp siluet ayrılıyor mu; arpa ince olsa da 1 karoyu aşmıyor mu.

## Notlar
- Ürün ve tohum ikonları 316 kapsamında.
