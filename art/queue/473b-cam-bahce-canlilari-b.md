---
id: "473b"
title: "Cam bahçe canlıları B (8)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe), Project belgesi 47
priority: 20
family: esya
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["471"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/viv_frog.png
  - Assets/Art/Iso/_aday/viv_cricket.png
  - Assets/Art/Iso/_aday/viv_spider.png
  - Assets/Art/Iso/_aday/viv_ground_beetle.png
  - Assets/Art/Iso/_aday/viv_lizard.png
  - Assets/Art/Iso/_aday/viv_periwinkle.png
  - Assets/Art/Iso/_aday/viv_hermit.png
  - Assets/Art/Iso/_aday/viv_shrimp.png
created: 2026-11-20
---
# 473b Cam bahçe canlıları B (8)

## Amaç
Belge 47: cam bahçede yaşayan böcek ve küçük hayvanlar. Kod yürütür, zıplatır, uçurur; sağa bakan tek kare yeter (sola aynalanır).

## Tarif
`create_image_pixflux`, `no_background=true`, yan görünüm, **sağa bakar**, ayaklar alt kenarda. Ateş böceği gündüz hâliyle (ışığı kod çizer). Gerçek türlerin rengine sadık.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.viv_frog` | `viv_frog.png` | 24x24 | `small young green frog sitting` |
| 2 | `item.viv_cricket` | `viv_cricket.png` | 16x16 | `dark brown field cricket` |
| 3 | `item.viv_spider` | `viv_spider.png` | 16x16 | `garden cross spider hanging` |
| 4 | `item.viv_ground_beetle` | `viv_ground_beetle.png` | 16x16 | `shiny blue-black ground beetle` |
| 5 | `item.viv_lizard` | `viv_lizard.png` | 24x24 | `small brown-green wall lizard` |
| 6 | `item.viv_periwinkle` | `viv_periwinkle.png` | 16x16 | `dark sea periwinkle snail` |
| 7 | `item.viv_hermit` | `viv_hermit.png` | 24x24 | `hermit crab in a whelk shell` |
| 8 | `item.viv_shrimp` | `viv_shrimp.png` | 16x16 | `translucent pink rock shrimp swimming` |

## Doğrulama
`art/results/473b-yanyana.png`: 471 nemli zemini üstünde 2x ölçekte. Dönem kuralları (CLAUDE.md, Project belgesi 20): orman camı (yeşilimsi, kabarcıklı), Yeni Dünya türü yok.

## Notlar
Kod görseli adla bulur (`IsoArt.Get`), gelene kadar renkli şekil çizer; dosya adı değişmesin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
