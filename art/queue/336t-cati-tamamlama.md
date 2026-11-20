---
id: "336t"
title: "Çatı tamamlama: saz, şindıl ve kara kiremit yeniden; arduvaz ve bütün sırtlar"
status: review
claimed_at: 2026-11-20T10:40Z
approved_by: user        # kullanıcı kararı 2026-11-20 (sohbette yapı incelemesi: "önerini uygula")
priority: 15
family: yapi
style_gate: false
cost_est: 14
cost_max: 30
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_roof_thatch_a_v2.png
  - Assets/Art/Iso/_aday/iso_roof_thatch_ridge.png
  - Assets/Art/Iso/_aday/iso_roof_shingle_a_v2.png
  - Assets/Art/Iso/_aday/iso_roof_shingle_ridge.png
  - Assets/Art/Iso/_aday/iso_roof_tile_ridge.png
  - Assets/Art/Iso/_aday/iso_roof_slate_a.png
  - Assets/Art/Iso/_aday/iso_roof_slate_ridge.png
  - Assets/Art/Iso/_aday/iso_roof_stone_slab_ridge.png
  - Assets/Art/Iso/_aday/iso_roof_tile_glazed_ridge.png
  - Assets/Art/Iso/_aday/iso_roof_tile_black_a_v2.png
  - Assets/Art/Iso/_aday/iso_roof_tile_black_ridge.png
created: 2026-11-20
---
# 336t Çatı tamamlama

## Neden
Yapı incelemesinde (2026-11-20) 336b ve 449 kabul edildi, ama:
- Saz çatı (`roof_thatch_a`) dama tahtası gibi duruyor.
- Şindıl (`roof_shingle_a`) mor-siyah çıkmış.
- "Kara kiremit" (`roof_tile_black_a`) pembe çıkmış.
- Arduvaz ve **bütün sırt parçaları** hiç teslim edilmemişti.

## Tarif
336b'deki tarif (düz eğim karosu ve sırt parçası, aynı tuval, kamera ve eğim). 449 da aynı tarifi kullanıyor. Farklar:
- **Saz (v2):** dama tahtası ya da örgü deseni olmamalı. Kalın, yumuşak, yuvarlatılmış saman demetleri, yatay katmanlar hâlinde, eğim yönünde akan ince çizgiler. Renk soluk altın-kahve (buğday), gölgeli kısımları koyu kahve.
- **Şindıl (v2):** ahşap pul kaplama. Sıra sıra, birbirine bindirilmiş küçük dikdörtgen tahta pullar. Renk **gri-kahve, eskimiş ahşap**; mor ya da siyah olmamalı.
- **Kara kiremit (v2):** gerçekten **koyu antrasit ya da siyah** sırlı kiremit; pembe ya da kırmızı ton olmamalı. Biçim olarak `roof_tile_a` ile aynı kiremit dizilişi.
- **Arduvaz:** mavi-gri, ince, düzensiz dikdörtgen taş levhalar, sıra sıra bindirilmiş.
- **Sırtlar (7):** her çatı malzemesi için çatı sırtı parçası. Saz sırtı kalın, yuvarlak bir saman rulosu olmalı. Şindıl sırtı iki tahta kapama. Kiremit, sırlı yeşil ve kara kiremit sırtları yarım yuvarlak mahya kiremidi. Arduvaz sırtı ince taş ya da kurşun kapama. Taş levha sırtı kalın taş kapak. Sırt, kendi düz karosuyla aynı renk ve ışıkta olmalı.
- Kabul edilen `roof_tile_a`, `roof_stone_slab_a` ve `roof_tile_glazed_a` değişmez; sırtlar onlara uymalı.

## Doğrulama
336b doğrulaması. Yan yana sayfası: `art/results/336t-yanyana.png`. Her malzemenin düz karosu ve sırtı yan yana, 2x2 döşenmiş hâliyle.

## Notlar
