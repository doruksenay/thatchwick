---
id: "612d"
title: "Köy meydanı: kuyu, kiler, çan kulesi, pazar çardağının yıkık hâli (maskeli)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20: yıkık hâller maskeli düzenlemeyle (Project belgesi 64c §3)
priority: 15
family: yapi
style_gate: false
cost_est: 24
cost_max: 44
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_square_{well,cellar,belltower,market_hall}_ruin_v2_se.png
created: 2026-11-20
---
# 612d Köy meydanı: yıkık hâller (maskeli düzenleme)

## Neden
612b'de yıkık hâller yeniden üretimle çizildi ve onarılmış hâle benzemedi (çan kulesi başka bir yapı oldu, pazar çardağı yıkık okunmadı). Kullanıcı kararı: yıkık hâl, onarılmış görselin **maskeli düzenlemesiyle** üretilir; yalnızca maskenin içi yeniden çizilir, gerisi piksel piksel aynı kalır.

## Yöntem
- Araç: `inpaint_image_pro_flash` (önce, ≈5 gen); tutmazsa `edit_image` (≈10 gen). `create_image_pixflux` + `init_image` **kullanılmaz** (612b'de tutmadı).
- Kaynak: aşağıdaki onarılmış dosya, olduğu gibi. Tuval ve boyut aynı kalır.
- Maske: `art/tmp/` altında Python ile. Taban elması ve alt satırlar (yapının ayak izi ve temel taşları) **maskenin dışında** kalır; maske yapının üst kısmını (çatı, üst duvar, direklerin üst yarısı) kapsar ve dışa doğru birkaç piksel taşar ki devrik parçalar ve moloz zemine düşebilsin.
- Maske dışındaki pikseller sonuçta kaynakla **aynı** olmalı (doğrula: fark sayısı 0; inpaint kenarda kayarsa maske dışını kaynaktan geri kopyala ve raporda yaz).

| Çıktı | Kaynak (onarılmış) | Yıkık hâlde ne olmalı |
|---|---|---|
| `iso_square_well_ruin_v2_se` | `iso_square_well_se` | Ahşap çatı ve direkler devrik, yerde yatıyor; kova ve ip yok; bilezik taşlarının üst sırası dağılmış, yanına düşmüş taşlar; yabani ot |
| `iso_square_cellar_ruin_v2_se` | `iso_square_cellar_se` | Taş kemerin üst taşları düşmüş, kemer yarı çökük; kapı kanadı tek menteşede sarkıyor; toprak basamakların üstüne dökülmüş |
| `iso_square_belltower_ruin_v2_se` | `iso_square_belltower_se` | Kiremit çatı ve ahşap çan katı yok olmuş; taş kaidenin üstünde yalnızca kırık ahşap direk uçları; çan yerde, kaidenin dibinde; çevrede kiremit kırıkları |
| `iso_square_market_hall_ruin_v2_se` | `iso_square_market_hall_se` | Çatının yarısı çökmüş, kiremitler ve kirişler yığın hâlinde yerde; direklerin bir kısmı ayakta, bir kısmı devrik; tezgâhlar kırık |

Dönem: ortaçağ; yangın, kan, insan yok. Işık sol üstten.

## Doğrulama
`validate_iso --kind object --max-colors 48`, `style.py check --family yapi`. Maske dışı fark 0 (sayıyla raporla). Yan yana sayfası `art/results/612d-yanyana.png`: her satırda onarılmış | yıkık | maske.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
