---
id: "471"
title: "Cam bahçe iç zeminleri: toprak, kum, tuzlu su (3)"
status: running
claimed_at: 2026-11-20T07:19Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe), Project belgesi 47
priority: 20
family: esya
style_gate: false
cost_est: 3
cost_max: 6
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/viv_bg_moist.png
  - Assets/Art/Iso/_aday/viv_bg_dry.png
  - Assets/Art/Iso/_aday/viv_bg_salt.png
created: 2026-11-20
---
# 471 Cam bahçe iç zeminleri: toprak, kum, tuzlu su (3)

## Amaç
Belge 47: cam bahçeye dokununca açılan büyük ekranın arka planı (yan görünüm, izometrik değil).

## Tarif
`create_image_pixflux`, yan görünüm (side view), çerçevesiz; zemin şeridi alttan %20 (tuzlu suda %12), üst kısım boş kalsın (canlılar orada uçar). Kod 160x96 dosyayı panele esnetir; nokta filtre.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `—` | `viv_bg_moist.png` | 160x96 | `inside view of a glass case: dark moist soil floor with pebbles, faint glass reflections, empty, no plants, no animals` |
| 2 | `—` | `viv_bg_dry.png` | 160x96 | `inside view of a glass case: pale sand floor with small stones and a flat rock, empty, no plants, no animals` |
| 3 | `—` | `viv_bg_salt.png` | 160x96 | `inside view of a glass case filled with sea water: sand floor, a few shells and rocks, light rays, empty, no animals` |

## Doğrulama
`art/results/471-yanyana.png`: üçü yan yana. Ön planda canlı ya da bitki olmamalı. Dönem kuralları (CLAUDE.md, Project belgesi 20): orman camı (yeşilimsi, kabarcıklı), Yeni Dünya türü yok.

## Notlar
Kod görseli adla bulur (`IsoArt.Get`), gelene kadar renkli şekil çizer; dosya adı değişmesin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
