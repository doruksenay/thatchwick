---
id: "612"
title: "Zemin türleri: kum, çayır, koyu çim, çakıllı toprak (10 karo)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("dediklerini uygula tamamen"; belge 64a §4 Aşama A madde 6)
family: zemin
style_gate: false
cost_est: 10
cost_max: 16
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_sand_a.png
  - Assets/Art/Iso/iso_ground_sand_b.png
  - Assets/Art/Iso/iso_ground_sand_c.png
  - Assets/Art/Iso/iso_ground_meadow_a.png
  - Assets/Art/Iso/iso_ground_meadow_b.png
  - Assets/Art/Iso/iso_ground_meadow_c.png
  - Assets/Art/Iso/iso_ground_grass_dark_a.png
  - Assets/Art/Iso/iso_ground_grass_dark_b.png
  - Assets/Art/Iso/iso_ground_dirt_gravel_a.png
  - Assets/Art/Iso/iso_ground_dirt_gravel_b.png
created: 2026-11-20
---
# 612 Zemin türleri

## Amaç
Belge 64 §8 Z1, 64a. Harita taslağındaki kum (`FloorType.Sand`, göl ve deniz kıyısı) ve zemin yamaları (çayır, koyu çim,
çakıllı toprak): her yer aynı çim olmasın. Kod hazır: `IsoArt.Ground` kum için `iso_ground_sand_a/b/c` arar.

## Tarif
Araç: `create_tiles_pro`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`,
004e ile aynı ayarlar ve palet (referans: `iso_ground_grass_a`, `iso_ground_dirt_a`). Aynalama ve döndürme yok.
- Tek karo (dolgu), her türden 2-3 çeşit; çeşitler yan yana dizildiğinde dikiş görünmemeli (kenar parlaklığı içle aynı).
- `sand`: açık, ince taneli göl/deniz kumu, birkaç küçük çakıl ve kabuk kırığı, sıcak bej (`iso_ground_beach_a` tonuna yakın).
- `meadow`: çim tabanı `iso_ground_grass_a` ile aynı ton, üstünde seyrek beyaz ve sarı çayır çiçeği noktaları.
- `grass_dark`: gölgeli, biraz daha koyu ve sık çim (ağaç altı).
- `dirt_gravel`: `iso_ground_dirt_a` tonunda, gri çakıl serpintili sert toprak.

## Doğrulama
`validate_iso.py --kind ground`; 4x4 döşeme testi (her çeşit kendi ve diğer çeşitlerle yan yana; ızgara seçilmemeli).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
