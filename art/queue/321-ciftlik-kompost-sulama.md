---
id: "321"
title: Çiftlik eşyaları (kompost yığını, tohum yapıcı, sulama hendeği, kova-makara, su oluğu)
status: pending
approved_by: pending
family: esya
style_gate: false
cost_est: 5
cost_max: 9
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_compost_heap_se.png
  - Assets/Art/Iso/_aday/iso_item_seed_maker_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_irrigation_ditch_se.png
  - Assets/Art/Iso/_aday/iso_item_bucket_pulley_se.png
  - Assets/Art/Iso/_aday/iso_item_water_channel_2x1_se.png
created: 2026-11-11
---
# 321 Çiftlik eşyaları

## Amaç
Çiftçilik eşyaları (`garden`/`work`): `item.compost_heap` (1x1, gübre üretir), `item.seed_maker` (2x1, ürünü tohuma çevirir), `item.irrigation_ditch` (1x1, `waterRadius` 1), `item.bucket_pulley` (1x1, yarıçap 2), `item.water_channel` (2x1, yarıçap 3). Sulama eşyaları yarıçapları sayesinde ekinleri otomatik suladığı için görselleri ekin sıralarının yanında okunmalı.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

| # | Dosya | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama (+ ortak ek) |
|---|---|---|---|---|---|---|
| 1 | `iso_item_compost_heap_se.png` | 1x1 | 80x64 | `--n 1` | 22 | `low heap of dark rotting plant matter and straw with a few peeling vegetable scraps` |
| 2 | `iso_item_seed_maker_2x1_se.png` | 2x1 | 112x80 | `--n 2` | 34 | `simple wooden bench with a hand-cranked seed press and a small wooden sieve tray, clay seed bowl` |
| 3 | `iso_item_irrigation_ditch_se.png` | 1x1 | 64x40 | `--n 1` | 6 | `shallow dug earth channel with clear blue water and a few wooden edge planks, no grass` |
| 4 | `iso_item_bucket_pulley_se.png` | 1x1 | 72x88 | `--n 1` | 52 | `stone-ringed well-top with a wooden post and rope pulley holding a wooden bucket, no person` |
| 5 | `iso_item_water_channel_2x1_se.png` | 2x1 | 112x56 | `--n 2` | 12 | `raised wooden trough water channel on short stakes, clear blue water, small side spouts` |

- Yön: yalnızca SE; hendek ve oluk için SW-NE yönü sonraki iş (döndürülmüş ayna kullanılabilir, ayna notu 301'de).
- Ortak ek: 301'deki ekten "dark brown wood" kalır; sulama eşyalarında su `#4a7fb5` ailesinden tek ton.
- Maliyet 5 × 1 gen. İkinci deneme payı 4 → `cost_max` 9.

## Doğrulama
301 doğrulama tablosu. Ek: hendek ve oluk çimen + işlenmiş toprak karolarında ayırt ediliyor mu; kova-makara köylü boyundan (≈48 px) uzun değil mi.

## Notlar
- `waterRadius` oyunda yalnızca mantıktır; görselde menzil çizilmez.
