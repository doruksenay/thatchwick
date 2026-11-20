---
id: "474b"
title: "Cam bahçe canlıları: yeniden deneme (8)"
status: running
claimed_at: 2026-11-20T07:39Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe: balık, taş, yosun, kabuk), Project belgesi 47
priority: 20
family: hayvan
style_gate: false
cost_est: 8
cost_max: 16
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/viv_butterfly.png
  - Assets/Art/Iso/_aday/viv_caterpillar.png
  - Assets/Art/Iso/_aday/viv_firefly.png
  - Assets/Art/Iso/_aday/viv_ladybird.png
  - Assets/Art/Iso/_aday/viv_worm.png
  - Assets/Art/Iso/_aday/viv_spider.png
  - Assets/Art/Iso/_aday/viv_ground_beetle.png
  - Assets/Art/Iso/_aday/viv_periwinkle.png
created: 2026-11-20
---
# 474b Cam bahçe canlıları: yeniden deneme (8)

## Amaç
Belge 47: 473a ve 473b'de palet nedeniyle teslim edilmeyen böcekler. Kod yürütür, uçurur, toprağa sokar.

## Tarif
`create_image_pixen` (16x16 ve 24x24; pixflux 32x32 altını reddeder), `no_background=true`, `view=side`, yan görünüm, ayaklar ya da taban alt kenarda (pivot alttan ortada). Canlılar **sağa bakar** (sola kod aynalar). Ateş böceği gündüz hâliyle (ışığı kod çizer).
Aile `hayvan`: 470-473 raporundaki ders, `esya` paleti kahverengi ağırlıklı olduğu için yeşil ve renkli türler palet dışı kaldı; bu iş kendi ailesinin referanslarıyla denetlenir.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.viv_butterfly` | `viv_butterfly.png` | 24x24 | `orange butterfly with open wings, side view` |
| 2 | `item.viv_caterpillar` | `viv_caterpillar.png` | 16x16 | `green caterpillar` |
| 3 | `item.viv_firefly` | `viv_firefly.png` | 16x16 | `firefly beetle with pale yellow tail` |
| 4 | `item.viv_ladybird` | `viv_ladybird.png` | 16x16 | `red ladybird with black spots` |
| 5 | `item.viv_worm` | `viv_worm.png` | 16x16 | `pink earthworm curled` |
| 6 | `item.viv_spider` | `viv_spider.png` | 16x16 | `garden cross spider hanging on a thread` |
| 7 | `item.viv_ground_beetle` | `viv_ground_beetle.png` | 16x16 | `shiny blue-black ground beetle` |
| 8 | `item.viv_periwinkle` | `viv_periwinkle.png` | 16x16 | `dark sea periwinkle snail` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family hayvan <dosya> ...`, yan yana: `art/results/474b-yanyana.png`. Dönem kuralları (CLAUDE.md, Project belgesi 20): yalnızca Avrupa ve Akdeniz türleri.

## Notlar
Kod görseli adla bulur (`IsoArt.Get("viv_<ad>")`), gelene kadar renkli şekil çizer; dosya adı değişmesin. 472-473'te teslim edilmeyenlerin yeniden denemesi de burada (ham hâlleri `art/results/47x-red/`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
