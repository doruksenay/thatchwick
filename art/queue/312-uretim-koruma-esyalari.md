---
id: "312"
title: Saklama eşyaları (tuzlama fıçısı, kök mahzeni)
status: pending
approved_by: pending
family: esya
style_gate: false
cost_est: 2
cost_max: 5
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_salting_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_root_cellar_2x2_se.png
created: 2026-11-08
---
# 312 Saklama eşyaları

## Amaç
`items.json` `utility` kategorisi: `item.salting_barrel` (1x1, turşu) ve `item.root_cellar` (2x2, bozulmayı %40 yavaşlatır). Kurutma askısı (303), marangoz tezgâhı (303) ve kümes (305) Dalga 1 işlerinde olduğu için bu işten çıkarıldı.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

| # | Dosya | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama (+ ortak ek) |
|---|---|---|---|---|---|---|
| 1 | `iso_item_salting_barrel_se.png` | 1x1 | 80x72 | `--n 1` | 30 | `wooden barrel with iron hoops and a loose round lid, closed, no liquid visible` |
| 2 | `iso_item_root_cellar_2x2_se.png` | 2x2 | 144x112 | `--n 2` | 40 | `low earth-mound root cellar with a small wooden plank door on the front and a turf roof, closed door` |

- Yön: 2x2 simetrik.
- Maliyet 2 gen; ikinci deneme payı 3 → `cost_max` 5.

## Doğrulama
301 tablosu. Ek: kök mahzeninde kapı kesin görünür mü (etkileşim noktası), fıçıda sıvı/yiyecek yok mu.
