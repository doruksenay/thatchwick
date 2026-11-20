---
id: "483"
title: "Deniz balıkları ve kavanoz malzemeleri ikonları (7)"
status: review
claimed_at: 2026-11-20T09:48Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe her şey doğadan, deniz kıyısı, balık tutma oyunu), Project belgeleri 47 ve 50
priority: 20
family: esya
style_gate: false
cost_est: 7
cost_max: 14
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_fish_herring.png
  - Assets/Art/Iso/_aday/icon_fish_mackerel.png
  - Assets/Art/Iso/_aday/icon_viv_forest_soil.png
  - Assets/Art/Iso/_aday/icon_viv_sea_sand.png
  - Assets/Art/Iso/_aday/icon_viv_marsh_mud.png
  - Assets/Art/Iso/_aday/icon_viv_fresh_water.png
  - Assets/Art/Iso/_aday/icon_viv_sea_water.png
created: 2026-11-20
---
# 483 Deniz balıkları ve kavanoz malzemeleri ikonları (7)

## Amaç
Belge 47 ek ve 50: denizden tutulan ringa ve uskumru; cam bahçe zemini için doğadan kavanoza doldurulan toprak, kum, çamur ve su. Envanter ve mini oyunda görünür (`IsoArt.ItemIcon`).

## Tarif
`create_image_pixflux` 32x32, `no_background=true`, ikon görünümü (hafif yukarıdan). Kavanozlar aynı cam kavanoz biçiminde, yalnızca içerik değişir (tutarlılık için önce 1, sonra ötekiler aynı tohumla).

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.fish_herring` | `icon_fish_herring.png` | 32x32 | `silver-blue herring fish, side view, item icon` |
| 2 | `item.fish_mackerel` | `icon_fish_mackerel.png` | 32x32 | `mackerel fish with dark wavy stripes on green-blue back, side view, item icon` |
| 3 | `item.viv_forest_soil` | `icon_viv_forest_soil.png` | 32x32 | `small glass jar full of dark forest soil with a leaf, item icon` |
| 4 | `item.viv_sea_sand` | `icon_viv_sea_sand.png` | 32x32 | `small glass jar full of pale sea sand with a tiny shell, item icon` |
| 5 | `item.viv_marsh_mud` | `icon_viv_marsh_mud.png` | 32x32 | `small glass jar of grey-brown marsh mud, item icon` |
| 6 | `item.viv_fresh_water` | `icon_viv_fresh_water.png` | 32x32 | `small corked glass jar of clear greenish lake water, item icon` |
| 7 | `item.viv_sea_water` | `icon_viv_sea_water.png` | 32x32 | `small corked glass jar of clear blue sea water, item icon` |

## Doğrulama
`python tools/art/style.py check --family esya <dosya> ...`; renkli içerik (deniz suyu mavisi) palet dışı çıkarsa `hayvan` ya da `zemin` ailesiyle yeniden denetle (474 dersi). Dönem kuralları (CLAUDE.md, Project belgesi 20).

## Notlar
Kod görseli adla bulur; dosya adı değişmesin. Gelene kadar yer tutucu çizilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
