---
id: "418t"
title: "Ürün ikonu düzeltmeleri (19 ikon: yüzsüz, döneme uygun, okunur)"
status: review
claimed_at: 2026-11-20T10:47Z
approved_by: user        # kullanıcı kararı 2026-11-20 (sohbette arayüz incelemesi: "önerini uygula")
priority: 20
family: arayuz
style_gate: false
cost_est: 19
cost_max: 35
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_turnip_v2.png
  - Assets/Art/Iso/_aday/icon_item_cabbage_v2.png
  - Assets/Art/Iso/_aday/icon_item_egg_v2.png
  - Assets/Art/Iso/_aday/icon_item_mushroom_v2.png
  - Assets/Art/Iso/_aday/icon_item_nut_v2.png
  - Assets/Art/Iso/_aday/icon_item_honey_v2.png
  - Assets/Art/Iso/_aday/icon_item_pumpkin_v2.png
  - Assets/Art/Iso/_aday/icon_item_milk_v2.png
  - Assets/Art/Iso/_aday/icon_item_carrot_v2.png
  - Assets/Art/Iso/_aday/icon_item_radish_v2.png
  - Assets/Art/Iso/_aday/icon_item_elderflower_v2.png
  - Assets/Art/Iso/_aday/icon_item_cress_v2.png
  - Assets/Art/Iso/_aday/icon_item_nails_v2.png
  - Assets/Art/Iso/_aday/icon_item_feed_v2.png
  - Assets/Art/Iso/_aday/icon_item_hunting_bow_v2.png
  - Assets/Art/Iso/_aday/icon_item_potion_focus_v2.png
  - Assets/Art/Iso/_aday/icon_item_animal_salve_v2.png
  - Assets/Art/Iso/_aday/icon_item_iron_bar_v2.png
  - Assets/Art/Iso/_aday/icon_item_cezve_v2.png
created: 2026-11-20
---
# 418t Ürün ikonu düzeltmeleri

## Neden
Arayüz incelemesinde (2026-11-20) 316, 418-421 kabul edildi, ama aşağıdaki ikonlar ya sevimli çizgi film yüzlü (oyunun gerçekçi tonuyla uyumsuz), ya döneme aykırı, ya da okunmuyor.

## Tarif
Her ikon kendi kaynak işinin tarifini (tuval, kenar, palet, ışık yönü) aynen kullanır. Ortak kural: **hiçbir ikonda yüz, göz ya da ağız yok.** Stil, kabul edilen 418-420 ikonlarıyla aynı: düz, az tonlu, net siluet, koyu ince kontur.

| İkon | Kaynak | Ne olmalı |
|---|---|---|
| `icon_item_turnip` | 316 | Şalgam kökü: beyaz-mor yuvarlak kök, üstte yeşil yapraklar. **Yüz, göz ya da ağız yok.** |
| `icon_item_cabbage` | 316 | Lahana başı: açık yeşil, sıkı sarılmış yapraklar. Yüzsüz. |
| `icon_item_egg` | 316 | Tek kahve-krem tavuk yumurtası, hafif parıltı. Yüzsüz. |
| `icon_item_mushroom` | 316 | Kahve şapkalı 2-3 yabani mantar. Karakter ya da yüz yok. |
| `icon_item_nut` | 316 | Fındık: 3-4 kahve fındık, biri yeşil zarfında. Yüzsüz. |
| `icon_item_honey` | 316 | Küçük toprak çömlek içinde altın bal, ağzında bez ve ip. Tilki ya da hayvan figürü yok. |
| `icon_item_pumpkin` | 316 | **Su kabağı** (Lagenaria): açık yeşil-krem, dar boyunlu şişe biçimli kabak. Turuncu balkabağı olmamalı (dönem kuralı, Project 20). |
| `icon_item_milk` | 418a | Ağzı bezle bağlı toprak testi ya da ahşap kova içinde beyaz süt. Kırmızı şişe olmamalı. |
| `icon_item_carrot` | 419a | **Mor havuç** (dönem kuralı): koyu mor kök, üstte tüylü yeşil yapraklar. Turuncu olmamalı. |
| `icon_item_radish` | 419a | Turp: yuvarlak kırmızı-pembe kök, beyaz kuyruk ucu, yeşil yaprak. Kırmızı biber gibi uzun ve sivri olmamalı. |
| `icon_item_elderflower` | 419b | Mürver çiçeği: yassı şemsiye biçimli öbek, çok sayıda minik krem-beyaz çiçek, kısa yeşil sap. Mor ya da lavanta gibi başak olmamalı. |
| `icon_item_cress` | 419b | Tere: ince saplı, küçük yuvarlak yapraklı taze yeşil demet. Brokoli gibi olmamalı. |
| `icon_item_nails` | 420a | Demir çiviler: 4-6 gri-koyu demir çivi, başları belirgin, çapraz dağınık. Yeşil olmamalı. |
| `icon_item_feed` | 420a | Hayvan yemi: ağzı açık çuval, içinde tahıl ve saman. Yüzsüz. |
| `icon_item_hunting_bow` | 420d | **Uzun yay** (ahşap, ip gerili) ve yanında 1-2 ok. Tüfek ya da ateşli silah gibi görünmemeli. |
| `icon_item_potion_focus` | 420b | Odak iksiri: küçük cam şişe, içinde berrak mavi sıvı, mantar tıpa. Saksı gibi olmamalı. |
| `icon_item_animal_salve` | 420b | Hayvan merhemi: kapaklı küçük toprak kavanoz, içinde yeşil merhem, yanında bir şifalı yaprak. Saksıda bitki olmamalı. |
| `icon_item_iron_bar` | 420b | Demir külçe: gri-mavi metalik dikdörtgen çubuk, parlak kenarlı. Mor olmamalı. |
| `icon_item_cezve` | 421k | **Cezve**: uzun saplı, gövdesi alta doğru genişleyen küçük bakır cezve. Moka demlik (iki parçalı, köşeli) olmamalı. |

- Çıktılar `_v2` ekiyle; kabul edilince kod eşlemesi `_v2` dosyalarına çevrilir.

## Doğrulama
Kaynak işlerin doğrulaması. Yan yana sayfası: `art/results/418t-yanyana.png`. Her ikonun eski hâli ve v2'si yan yana, ayrıca 418b'den 3 ikonla aynı satırda (stil uyumu).

## Notlar
