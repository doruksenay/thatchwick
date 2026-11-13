---
id: "317"
title: Kök ekinleri evreleri (yaban havucu, havuç, soğan, sarımsak, pancar; 3 evre)
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
  - Assets/Art/Iso/_aday/iso_crop_parsnip_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_carrot_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_onion_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_garlic_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_beet_{1,2,3}_se.png
created: 2026-11-11
---
# 317 Kök ekinleri evreleri

## Amaç
Çiftçilik genişlemesi (v0): `item.crop_parsnip`, `item.crop_carrot`, `item.crop_onion`, `item.crop_garlic`, `item.crop_beet` (hepsi `plant`, `tilled: true`). Kök ekinler toprağın altında büyür; olgun evrede kök ucu toprak üstüne çıkarak ürünü ele verir. Evre mantığı 310 ile aynı.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

- Ayak izi 1x1, tuval 64x64, kılavuz `--n 1`, yükseklik: evre 1 → 10, evre 2 → 22, evre 3 → 32 (en çok 36). Toprak sprite'a çizilmez ("no soil"); zemin `iso_ground_dirt_*` ile gelir.
- Açıklamaya ortak ek: "muted green and earthy colours" ("dark brown wood" çıkarılır). Aynı bitkinin üç evresi aynı palet ve kamerayla; taban elması ve piksel sınır kutusu evreler arasında orta çizgide hizalı.
- Evre 1 = filiz (0-33%), 2 = büyüyor (33-99%), 3 = olgun/hasada hazır. Kış ölümü ve kuruma için ayrı sprite yok (kod `Water`/`FertilityAt` göstergesini HUD'da verir).
- Açıklamalar:
  - parsnip: `{1: small feathery pale-green sprout, 2: fern-like green fronds, 3: cream-coloured tapered root top showing above feathery fronds}`
  - carrot: `{1: tiny feathery sprout, 2: dense feathery green tops, 3: orange carrot shoulder showing above dense green tops}`
  - onion: `{1: thin upright green shoot, 2: several tall green tubular leaves, 3: round golden-brown onion bulb half out of the ground with drooping green tops}`
  - garlic: `{1: thin upright green shoot, 2: flat green blades with a curling stem, 3: white garlic bulb half out of the ground with dried yellowing tops}`
  - beet: `{1: two small red-veined leaves, 2: broad dark-green red-veined leaves, 3: deep red-purple beetroot half out of the ground with red-veined leaves}`
- Maliyet: 15 parça × 1 gen = 15. İkinci deneme payı 9 → `cost_max` 24.

## Doğrulama
301 doğrulama tablosu. Ek: her bitkinin 3 evresi yan yana önizleme; evre boyları artan sırada mı; olgun evre rengi ürün ikonuyla (316) uyuşuyor mu; 310'daki 5 ekinle yan yana okunaklı ve ayırt edilebilir mi (özellikle havuç ↔ yaban havucu).

## Notlar
- Dosya adındaki `crop_<ad>` pivot tablosu önek eşleşmesini kullanır (310 ile aynı).
- Tohum ve ürün ikonları 316'nın kapsamındadır; bu işte yok.
