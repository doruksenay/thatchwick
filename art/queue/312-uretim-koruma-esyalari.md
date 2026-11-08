---
id: "312"
title: Üretim ve saklama eşyaları (tuzlama fıçısı, kurutma askısı, kök mahzeni, tezgâh, kümes)
status: pending
approved_by: pending
cost_est: 5
cost_max: 12
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_salting_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_drying_rack_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_root_cellar_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_workbench_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_coop_2x1_se_nw.png
created: 2026-11-08
---
# 312 Üretim ve saklama eşyaları

## Amaç
`items.json` `utility` kategorisi: `item.salting_barrel` (1x1, turşu), `item.drying_rack` (2x1, kurutma), `item.root_cellar` (2x2, bozulmayı %40 yavaşlatır), `item.workbench` (2x1, marangoz tezgâhı), `item.coop` (2x1, kümes; 305'teki hayvan yapılarının küçük kardeşi — 305 yapılırsa bu iş onunla çakışır, önce 305'e bak).

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

| # | Dosya | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama (+ ortak ek) |
|---|---|---|---|---|---|---|
| 1 | `iso_item_salting_barrel_se.png` | 1x1 | 80x72 | `--n 1` | 30 | `wooden barrel with iron hoops and a loose round lid, closed, no liquid visible` |
| 2 | `iso_item_drying_rack_2x1_se_nw.png` | 2x1 | 112x88 | `--n 2 --axis x` | 48 | `open wooden drying rack: two posts and a crossbar with empty hanging string loops, no food` |
| 3 | `iso_item_root_cellar_2x2_se.png` | 2x2 | 144x112 | `--n 2` | 40 | `low earth-mound root cellar with a small wooden plank door on the front and a turf roof, closed door` |
| 4 | `iso_item_workbench_2x1_se_nw.png` | 2x1 | 112x84 | `--n 2 --axis x` | 34 | `sturdy carpentry workbench with a thick plank top and a small vise at one end, bare top, no tools lying around` |
| 5 | `iso_item_coop_2x1_se_nw.png` | 2x1 | 112x96 | `--n 2 --axis x` | 44 | `small wooden chicken coop house on short stilts with a ramp and a pitched shingle roof, no animals` |

- Yön: 2x2 simetrik; 2x1 `se_nw` ekseni 301'deki yatakla aynıdır.
- Maliyet 5 gen; ikinci deneme payı 7 → `cost_max` 12.

## Doğrulama
301 tablosu. Ek: kök mahzeninde kapı kesin görünür mü (etkileşim noktası), fıçıda sıvı/yiyecek yok mu, kümeste hayvan yok mu.
