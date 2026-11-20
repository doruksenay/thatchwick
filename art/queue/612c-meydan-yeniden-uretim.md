---
id: "612c"
title: "Köy meydanı: değirmen çarkı, köprü, ortak ocak ve iki iskele katmanı (612 reddedilenler)"
status: running
claimed_at: 2026-11-20T18:51Z
approved_by: user        # kullanıcı kararı 2026-11-20 (Project belgesi 64c §3; 612 kısmi kabul)
priority: 15
family: yapi
style_gate: false
cost_est: 8
cost_max: 20
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_square_millwheel_v2_se.png
  - Assets/Art/Iso/_aday/iso_square_bridge_v2_se.png
  - Assets/Art/Iso/_aday/iso_square_hearth_se.png
  - Assets/Art/Iso/_aday/iso_square_scaffold_small_v2_se.png
  - Assets/Art/Iso/_aday/iso_square_scaffold_large_v2_se.png
created: 2026-11-20
---
# 612c Köy meydanı: reddedilenlerin yeniden üretimi

## Neden
612'de kabul edilenler: kuyu, kiler, çan kulesi, pazar çardağı. Reddedilenler aşağıda, nedenleriyle. Stil referansı (`--ref`): kabul edilen dört yapı (`_aday/iso_square_{well,cellar,belltower,market_hall}_se.png`).

## Tarif
612'nin aracı ve ayarları; `tools/art/footprint_guide.py` kılavuzuyla (ayak izi elmasını doldursun).

| Dosya | Ayak izi, tuval | Ne olmalı | 612'deki hata |
|---|---|---|---|
| `iso_square_millwheel_v2_se` | 2x2, 128x160 | Büyük ahşap **su çarkı**: dikey duran tekerlek, kenarında kepçeler, kalın ahşap mil, iki taşıyıcı direk ve kısa taş oluk duvarı. Bina, çatı, rampa yok. Kendi çim ya da beton tabanı yok | rampalı bir kulübe çizildi |
| `iso_square_bridge_v2_se` | 3x2, 192x128 | Kemerli taş köprü, alçak korkuluk duvarları; **üstünde araç, insan, hayvan, eşya yok**; altı saydam; uçlar tuvalin sol ve sağ köşelerine uzanır | üstünde araba vardı |
| `iso_square_hearth_se` | 2x2, 128x128 | Meydanda ortak ocak: alçak yuvarlak taş halka, üç ayak üstünde demir kazan, çevresinde kütük oturaklar. **Sönük:** kor, alev, kırmızı ya da turuncu piksel yok. `--max-colors 48` | iki kez üretilemedi (boş ya da korlu) |
| `iso_square_scaffold_small_v2_se` | 2x2, 128x192 | **Yalnızca iskele:** bağlanmış ahşap direkler, kalaslar, bir merdiven. Bina, duvar, taş, çatı yok; direklerin arası **saydam**. Bir yapının önüne bindirilecek katman | içinde taş bina vardı |
| `iso_square_scaffold_large_v2_se` | 3x3, 192x256 | Aynı, daha geniş ve yüksek | içi beyaz doluydu (saydam olmalı) |

Dönem: ortaçağ köyü; taş, ahşap, demir. Işık sol üstten; gölge pişirilmez.

## Doğrulama
`validate_iso --kind object --max-colors 48`, `style.py check --family yapi`. İskelelerde opak piksel oranı %45'in altında olmalı (aradan arkadaki yapı görünsün); beyaz ya da düz renkli dolgu yok. Yan yana sayfası `art/results/612c-yanyana.png`: her iskele, kabul edilen çan kulesinin ve pazar çardağının önüne bindirilmiş hâliyle.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
