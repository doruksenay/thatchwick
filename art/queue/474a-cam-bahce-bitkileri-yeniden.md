---
id: "474a"
title: "Cam bahçe bitkileri: yeniden deneme ve tatlı su bitkileri (10)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe: balık, taş, yosun, kabuk), Project belgesi 47
priority: 20
family: bitki
style_gate: false
cost_est: 10
cost_max: 20
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/viv_moss.png
  - Assets/Art/Iso/_aday/viv_fern.png
  - Assets/Art/Iso/_aday/viv_violet.png
  - Assets/Art/Iso/_aday/viv_water_moss.png
  - Assets/Art/Iso/_aday/viv_houseleek.png
  - Assets/Art/Iso/_aday/viv_thyme.png
  - Assets/Art/Iso/_aday/viv_sea_lettuce.png
  - Assets/Art/Iso/_aday/viv_duckweed.png
  - Assets/Art/Iso/_aday/viv_hornwort.png
  - Assets/Art/Iso/_aday/viv_lily.png
created: 2026-11-20
---
# 474a Cam bahçe bitkileri: yeniden deneme ve tatlı su bitkileri (10)

## Amaç
Belge 47: cam bahçe bitkileri. 472'de 7 bitki palet nedeniyle teslim edilmedi; tatlı su zemini için su mercimeği, boynuz yaprağı ve nilüfer eklendi. Oyun sağlığa göre soldurur, büyümeye göre ölçekler.

## Tarif
`create_image_pixflux` (32x48), `no_background=true`, `view=side`, yan görünüm, ayaklar ya da taban alt kenarda (pivot alttan ortada). Canlılar **sağa bakar** (sola kod aynalar). Su mercimeği ve nilüfer su yüzeyinde yüzer; görselin üst kenarı su çizgisi sayılır.
Aile `bitki`: 470-473 raporundaki ders, `esya` paleti kahverengi ağırlıklı olduğu için yeşil ve renkli türler palet dışı kaldı; bu iş kendi ailesinin referanslarıyla denetlenir.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.viv_moss` | `viv_moss.png` | 32x48 | `low cushion of bright green forest moss on a pebble` |
| 2 | `item.viv_fern` | `viv_fern.png` | 32x48 | `young fern with three curled fronds` |
| 3 | `item.viv_violet` | `viv_violet.png` | 32x48 | `small wild violet plant with purple flowers` |
| 4 | `item.viv_water_moss` | `viv_water_moss.png` | 32x48 | `clump of dark green water moss on wet stone` |
| 5 | `item.viv_houseleek` | `viv_houseleek.png` | 32x48 | `houseleek rosette, fleshy green succulent leaves with red tips` |
| 6 | `item.viv_thyme` | `viv_thyme.png` | 32x48 | `tiny wild thyme bush with pale pink flowers` |
| 7 | `item.viv_sea_lettuce` | `viv_sea_lettuce.png` | 32x48 | `bright green wavy sea lettuce frond` |
| 8 | `item.viv_duckweed` | `viv_duckweed.png` | 32x48 | `floating cluster of tiny round duckweed leaves, seen from the side at the water line` |
| 9 | `item.viv_hornwort` | `viv_hornwort.png` | 32x48 | `underwater hornwort stem with whorls of thin dark green needle leaves` |
| 10 | `item.viv_lily` | `viv_lily.png` | 32x48 | `white water lily seedling: round floating pads and one white flower, long stem` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`, yan yana: `art/results/474a-yanyana.png`. Dönem kuralları (CLAUDE.md, Project belgesi 20): yalnızca Avrupa ve Akdeniz türleri.

## Notlar
Kod görseli adla bulur (`IsoArt.Get("viv_<ad>")`), gelene kadar renkli şekil çizer; dosya adı değişmesin. 472-473'te teslim edilmeyenlerin yeniden denemesi de burada (ham hâlleri `art/results/47x-red/`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
