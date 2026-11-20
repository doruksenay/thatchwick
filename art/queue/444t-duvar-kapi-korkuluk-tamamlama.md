---
id: "444t"
title: "Duvar, kapı, korkuluk ve çit tamamlama (eksikler ve 2 yeniden)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20 (sohbette yapı incelemesi: "önerini uygula")
priority: 15
family: yapi
style_gate: false
cost_est: 26
cost_max: 50
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_finish_masonry_sw_ne_v2.png
  - Assets/Art/Iso/_aday/iso_wall_finish_masonry_se_nw_v2.png
  - Assets/Art/Iso/_aday/iso_wall_finish_blue_wash_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_finish_stencil_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_finish_stencil_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_finish_mural_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_finish_mural_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_finish_star_tile_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_finish_star_tile_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_finish_tapestry_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_door_blue_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_door_blue_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_door_green_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_door_green_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_railing_black_sw_ne_v2.png
  - Assets/Art/Iso/_aday/iso_wall_railing_black_se_nw_v2.png
  - Assets/Art/Iso/_aday/iso_wall_railing_red_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_railing_red_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_railing_green_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_railing_green_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_railing_iron_red_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_railing_iron_red_se_nw.png
  - Assets/Art/Iso/_aday/iso_wall_fence_hedge_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_fence_hedge_se_nw.png
created: 2026-11-20
---
# 444t Duvar, kapı, korkuluk ve çit tamamlama

## Neden
Yapı incelemesinde (2026-11-20) 328, 331, 444a, 446a, 447a ve 447b kabul edildi. Ancak bu parçalar hiç teslim edilmemişti; taş örgü kaplaması ve kara korkuluk da beğenilmedi.

## Tarif
Her parça kendi kaynak işinin tarifini aynen kullanır (iki eksen, aynı tuval, duvar yüksekliği ve kamera):

| Parça | Kaynak iş | Not |
|---|---|---|
| `finish_masonry` v2 (2 eksen) | 444a | **Taş örgü kaplaması.** Fıstık yeşili ya da yeşil zemin üstüne kırmızı çizgi olmamalı. Açık gri-bej, harçlı, düzensiz yontma taş dizisi. 445a'daki `rubble_lime`'dan daha düzgün sıralı. |
| `finish_blue_wash` se_nw | 444a | Kabul edilen sw_ne ile aynı. |
| `finish_stencil`, `finish_mural`, `finish_star_tile` (2'şer eksen) | 444a | 444a tarifindeki açıklamalar. Desen küçük ve tekrarlı olmalı; duvarı tek bir resim gibi doldurmamalı. |
| `finish_tapestry` se_nw | 328 | Kabul edilen sw_ne ile aynı dokuma. |
| `door_blue`, `door_green` (2'şer eksen) | 446a | Kabul edilen `door_red` ile aynı kapı biçimi, yalnızca boya rengi farklı (soluk mavi, koyu yeşil). |
| `railing_black` v2 (2 eksen) | 447a | **Kara boyalı ahşap korkuluk.** Yanık kütük yığını olmamalı. 332'deki `railing_wood` ile aynı düzgün biçim (üst tırabzan, dikmeler, alt kuşak), koyu antrasit boyalı. |
| `railing_red`, `railing_green` (2'şer eksen) | 447a | `railing_wood` biçimi, kırmızı ve yeşil boyalı. |
| `railing_iron_red` (2 eksen) | 447b | 332'deki `railing_iron` biçimi, koyu kırmızı boyalı demir. |
| `fence_hedge` (2 eksen) | 331 | Budanmış çit bitkisi. Kutu biçimli, sık yapraklı koyu yeşil çalı duvarı, diz-bel yüksekliğinde, üstü düz budanmış. |

## Doğrulama
Kaynak işlerin doğrulaması. Yan yana sayfası: `art/results/444t-yanyana.png`. Her parça kendi ailesinin kabul edilmiş bir örneğiyle yan yana (örneğin kara korkuluk, `railing_wood` ile).

## Notlar
