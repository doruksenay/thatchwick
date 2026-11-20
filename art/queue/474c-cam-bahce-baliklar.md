---
id: "474c"
title: "Cam bahçe balıkları ve tatlı su canlıları (8)"
status: pending
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
  - Assets/Art/Iso/_aday/viv_shrimp.png
  - Assets/Art/Iso/_aday/viv_minnow.png
  - Assets/Art/Iso/_aday/viv_stickleback.png
  - Assets/Art/Iso/_aday/viv_loach.png
  - Assets/Art/Iso/_aday/viv_water_flea.png
  - Assets/Art/Iso/_aday/viv_tadpole.png
  - Assets/Art/Iso/_aday/viv_pond_snail.png
  - Assets/Art/Iso/_aday/viv_goby.png
created: 2026-11-20
---
# 474c Cam bahçe balıkları ve tatlı su canlıları (8)

## Amaç
Belge 47 (kullanıcı 20.11): tatlı su ve tuzlu su zemininde balık ve su canlıları. Kod yüzdürür, dipte gezdirir, taşın ardına saklar.

## Tarif
`create_image_pixen` (16x16 ve 24x24), `no_background=true`, `view=side`, yan görünüm, ayaklar ya da taban alt kenarda (pivot alttan ortada). Canlılar **sağa bakar** (sola kod aynalar). Balıklar yüzerken yandan; dip balıkları (taş balığı, kaya balığı) karnı alt kenarda.
Aile `hayvan`: 470-473 raporundaki ders, `esya` paleti kahverengi ağırlıklı olduğu için yeşil ve renkli türler palet dışı kaldı; bu iş kendi ailesinin referanslarıyla denetlenir.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.viv_shrimp` | `viv_shrimp.png` | 16x16 | `translucent pink rock shrimp swimming` |
| 2 | `item.viv_minnow` | `viv_minnow.png` | 24x24 | `small minnow fish, olive back, silver belly, side view swimming` |
| 3 | `item.viv_stickleback` | `viv_stickleback.png` | 24x24 | `three-spined stickleback fish, side view` |
| 4 | `item.viv_loach` | `viv_loach.png` | 24x24 | `stone loach, slim mottled brown bottom fish with whiskers` |
| 5 | `item.viv_water_flea` | `viv_water_flea.png` | 16x16 | `tiny water flea, round translucent body` |
| 6 | `item.viv_tadpole` | `viv_tadpole.png` | 16x16 | `black tadpole with tail, side view` |
| 7 | `item.viv_pond_snail` | `viv_pond_snail.png` | 16x16 | `pond snail with pointed spiral brown shell` |
| 8 | `item.viv_goby` | `viv_goby.png` | 24x24 | `small sandy rock goby fish resting on the bottom` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family hayvan <dosya> ...`, yan yana: `art/results/474c-yanyana.png`. Dönem kuralları (CLAUDE.md, Project belgesi 20): yalnızca Avrupa ve Akdeniz türleri.

## Notlar
Kod görseli adla bulur (`IsoArt.Get("viv_<ad>")`), gelene kadar renkli şekil çizer; dosya adı değişmesin. 472-473'te teslim edilmeyenlerin yeniden denemesi de burada (ham hâlleri `art/results/47x-red/`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
