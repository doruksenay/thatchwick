---
id: "323"
title: Katlar: merdivenler ve kat zemin parçaları (el merdiveni, 2 merdiven, 2 taş merdiven, üst kat döşemesi, bodrum zemini)
status: pending
approved_by: pending
cost_est: 8
cost_max: 16
depends_on: ["301", "306"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_ladder_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_up_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_down_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_up_stone_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_down_stone_2x1_se.png
  - Assets/Art/Iso/_aday/iso_floor_upper_plank_a.png
  - Assets/Art/Iso/_aday/iso_floor_upper_plank_b.png
  - Assets/Art/Iso/_aday/iso_floor_cellar_dirt_a.png
created: 2026-11-11
---
# 323 Katlar: merdivenler ve kat zemin parçaları

## Amaç
Üst kat ve bodrum (`GridPos.z`): `item.ladder`, `item.stairs_up`, `item.stairs_down`, `item.stairs_up_stone`, `item.stairs_down_stone` (hepsi 1x2 ayak izi, `stairs` ±1, dar kanat). Üst katta zemin ahşap tahta döşemedir; bodrum zemini sıkıştırılmış toprak/taştır (kat görünümünde zemin kat üstünde saydam ya da kararır, bu yüzden zemin sprite'ları 64x32 elmas olmalı, gölgesiz).

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

| # | Dosya | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama (+ ortak ek) |
|---|---|---|---|---|---|---|
| 1 | `iso_item_ladder_se.png` | 1x2 | 56x96 | `--n 1`+yükseklik | 64 | `wooden wall ladder with two rails and rungs leaning up through a floor hatch opening, no person` |
| 2 | `iso_item_stairs_up_2x1_se.png` | 1x2 | 96x96 | `--n 2` | 44 | `short straight wooden staircase with plank steps and a side handrail rising toward the back` |
| 3 | `iso_item_stairs_down_2x1_se.png` | 1x2 | 96x72 | `--n 2` | 24 | `wooden stairs descending into a dark rectangular floor opening, side handrail, top step flush with floor` |
| 4 | `iso_item_stairs_up_stone_2x1_se.png` | 1x2 | 96x96 | `--n 2` | 44 | `short straight grey stone staircase with chunky steps and a rough wall side` |
| 5 | `iso_item_stairs_down_stone_2x1_se.png` | 1x2 | 96x72 | `--n 2` | 24 | `grey stone steps descending into a dark floor opening, rough stone rim` |
| 6-7 | `iso_floor_upper_plank_{a,b}.png` | 1x1 zemin | 64x32 | elmas | 0 | `top-down pixel floor tile, warm light-brown wooden planks, seamless, no shading` (iki varyasyon) |
| 8 | `iso_floor_cellar_dirt_a.png` | 1x1 zemin | 64x32 | elmas | 0 | `top-down pixel floor tile, dark packed earth with small stones, seamless, cool grey-brown` |

- Merdivenin tırmanış yönü +x (SE sprite); diğer yönler ayna ile (301 ayna notu), merdiven bir yönde aynalanabilir ama basamak ışığı sol-üstten kalır.
- Maliyet 8 × 1 gen. İkinci deneme payı 8 → `cost_max` 16.

## Doğrulama
301 doğrulama tablosu. Ek: merdivenin zemin kat duvarıyla birleşimi (305/306 duvar parçaları ile yan yana); üst kat zemini ile zemin kat zemini ardışık katmanda ayırt ediliyor mu; bodrum zemini koyu yeşil/gri aile içinde, çimenle karışmıyor mu.

## Notlar
- Kat sıralama bantları `Iso.LevelSortStep` (14000) ile kodda çözülüyor; sprite pivotları değişmez.
- Üst kat duvar parçaları yeni iş gerektirmez: mevcut duvar seti (303/306) tüm katlarda kullanılır.
- Dosya adlarındaki `2x1` eki yönergedeki çok karolu adlandırmayı izler; ayak izi `items.json`'da width 1, height 2 olduğundan iki karoluk uzun kenar SE görünümünde yatay çizilir. İşçi pivotu elmas çiftinin merkezi olarak raporlar.
