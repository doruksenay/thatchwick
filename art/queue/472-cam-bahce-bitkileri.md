---
id: "472"
title: "Cam bahçe bitkileri (8)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe), Project belgesi 47
priority: 20
family: esya
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["471"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/viv_moss.png
  - Assets/Art/Iso/_aday/viv_fern.png
  - Assets/Art/Iso/_aday/viv_violet.png
  - Assets/Art/Iso/_aday/viv_water_moss.png
  - Assets/Art/Iso/_aday/viv_houseleek.png
  - Assets/Art/Iso/_aday/viv_thyme.png
  - Assets/Art/Iso/_aday/viv_seaweed.png
  - Assets/Art/Iso/_aday/viv_sea_lettuce.png
created: 2026-11-20
---
# 472 Cam bahçe bitkileri (8)

## Amaç
Belge 47: cam bahçe görünümünde bitkiler. Oyun sağlığa göre soldurur (renk), büyümeye göre ölçekler.

## Tarif
`create_image_pixflux`, `no_background=true`, yan görünüm, kök tabanda ortada (kod tabanı zemine oturtur). 471 ile aynı palet ve ışık.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.viv_moss` | `viv_moss.png` | 32x48 | `low cushion of bright green forest moss on a pebble` |
| 2 | `item.viv_fern` | `viv_fern.png` | 32x48 | `young fern with three curled fronds` |
| 3 | `item.viv_violet` | `viv_violet.png` | 32x48 | `small wild violet plant with purple flowers` |
| 4 | `item.viv_water_moss` | `viv_water_moss.png` | 32x48 | `clump of dark green water moss on wet stone` |
| 5 | `item.viv_houseleek` | `viv_houseleek.png` | 32x48 | `houseleek rosette, fleshy green succulent leaves with red tips` |
| 6 | `item.viv_thyme` | `viv_thyme.png` | 32x48 | `tiny wild thyme bush with pale pink flowers` |
| 7 | `item.viv_seaweed` | `viv_seaweed.png` | 32x48 | `olive-brown bladderwrack seaweed frond` |
| 8 | `item.viv_sea_lettuce` | `viv_sea_lettuce.png` | 32x48 | `bright green wavy sea lettuce frond` |

## Doğrulama
`art/results/472-yanyana.png`: 471 zemininin üstünde. Dönem kuralları (CLAUDE.md, Project belgesi 20): orman camı (yeşilimsi, kabarcıklı), Yeni Dünya türü yok.

## Notlar
Kod görseli adla bulur (`IsoArt.Get`), gelene kadar renkli şekil çizer; dosya adı değişmesin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
