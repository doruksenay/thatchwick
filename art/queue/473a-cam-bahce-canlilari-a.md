---
id: "473a"
title: "Cam bahçe canlıları A (8)"
status: running
claimed_at: 2026-11-20T07:23Z
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
  - Assets/Art/Iso/_aday/viv_aphid.png
  - Assets/Art/Iso/_aday/viv_ladybird.png
  - Assets/Art/Iso/_aday/viv_snail.png
  - Assets/Art/Iso/_aday/viv_worm.png
  - Assets/Art/Iso/_aday/viv_woodlouse.png
  - Assets/Art/Iso/_aday/viv_firefly.png
  - Assets/Art/Iso/_aday/viv_caterpillar.png
  - Assets/Art/Iso/_aday/viv_butterfly.png
created: 2026-11-20
---
# 473a Cam bahçe canlıları A (8)

## Amaç
Belge 47: cam bahçede yaşayan böcek ve küçük hayvanlar. Kod yürütür, zıplatır, uçurur; sağa bakan tek kare yeter (sola aynalanır).

## Tarif
`create_image_pixflux`, `no_background=true`, yan görünüm, **sağa bakar**, ayaklar alt kenarda. Ateş böceği gündüz hâliyle (ışığı kod çizer). Gerçek türlerin rengine sadık.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.viv_aphid` | `viv_aphid.png` | 16x16 | `tiny green aphid` |
| 2 | `item.viv_ladybird` | `viv_ladybird.png` | 16x16 | `red ladybird with black spots` |
| 3 | `item.viv_snail` | `viv_snail.png` | 16x16 | `brown garden snail with striped shell` |
| 4 | `item.viv_worm` | `viv_worm.png` | 16x16 | `pink earthworm curled` |
| 5 | `item.viv_woodlouse` | `viv_woodlouse.png` | 16x16 | `grey woodlouse` |
| 6 | `item.viv_firefly` | `viv_firefly.png` | 16x16 | `firefly beetle with glowing yellow tail` |
| 7 | `item.viv_caterpillar` | `viv_caterpillar.png` | 16x16 | `green caterpillar` |
| 8 | `item.viv_butterfly` | `viv_butterfly.png` | 24x24 | `orange butterfly with open wings, side view` |

## Doğrulama
`art/results/473a-yanyana.png`: 471 nemli zemini üstünde 2x ölçekte. Dönem kuralları (CLAUDE.md, Project belgesi 20): orman camı (yeşilimsi, kabarcıklı), Yeni Dünya türü yok.

## Notlar
Kod görseli adla bulur (`IsoArt.Get`), gelene kadar renkli şekil çizer; dosya adı değişmesin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
