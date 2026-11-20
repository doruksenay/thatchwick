---
id: "612e"
title: "Köy meydanı: değirmen çarkı, köprü, ortak ocağın yıkık hâli (maskeli)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20: yıkık hâller maskeli düzenlemeyle (Project belgesi 64c §3)
priority: 15
family: yapi
style_gate: false
cost_est: 18
cost_max: 33
depends_on: ["612c"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_square_{millwheel,bridge,hearth}_ruin_v2_se.png
created: 2026-11-20
---
# 612e Köy meydanı: yıkık hâller (maskeli düzenleme)

## Neden
612b'de yıkık hâller yeniden üretimle çizildi ve onarılmış hâle benzemedi (çan kulesi başka bir yapı oldu, pazar çardağı yıkık okunmadı). Kullanıcı kararı: yıkık hâl, onarılmış görselin **maskeli düzenlemesiyle** üretilir; yalnızca maskenin içi yeniden çizilir, gerisi piksel piksel aynı kalır.

## Yöntem
- Araç: `inpaint_image_pro_flash` (önce, ≈5 gen); tutmazsa `edit_image` (≈10 gen). `create_image_pixflux` + `init_image` **kullanılmaz** (612b'de tutmadı).
- Kaynak: aşağıdaki onarılmış dosya, olduğu gibi. Tuval ve boyut aynı kalır.
- Maske: `art/tmp/` altında Python ile. Taban elması ve alt satırlar (yapının ayak izi ve temel taşları) **maskenin dışında** kalır; maske yapının üst kısmını (çatı, üst duvar, direklerin üst yarısı) kapsar ve dışa doğru birkaç piksel taşar ki devrik parçalar ve moloz zemine düşebilsin.
- Maske dışındaki pikseller sonuçta kaynakla **aynı** olmalı (doğrula: fark sayısı 0; inpaint kenarda kayarsa maske dışını kaynaktan geri kopyala ve raporda yaz).

| Çıktı | Kaynak (onarılmış) | Yıkık hâlde ne olmalı |
|---|---|---|
| `iso_square_millwheel_ruin_v2_se` | `iso_square_millwheel_v2_se` (612c) | Çarkın kepçelerinin yarısı kopmuş, birkaç kolu kırık, tekerlek eğik; kopan tahtalar yerde; mil yerinde |
| `iso_square_bridge_ruin_v2_se` | `iso_square_bridge_v2_se` (612c) | Taş kemerin ortası çökmüş; boşluğun üstüne iki-üç kalastan derme çatma yaya geçidi atılmış; korkuluk taşları eksik. Uçlar ve ayaklar aynı |
| `iso_square_hearth_ruin_v2_se` | `iso_square_hearth_se` (612c) | Taş halka dağılmış, kazan yok ya da devrik; kütük oturaklar çürümüş; is lekeli taşlar, kül; ateş yok |

Dönem: ortaçağ; yangın, kan, insan yok. Işık sol üstten.

## Doğrulama
`validate_iso --kind object --max-colors 48`, `style.py check --family yapi`. Maske dışı fark 0 (sayıyla raporla). Yan yana sayfası `art/results/612e-yanyana.png`: her satırda onarılmış | yıkık | maske.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
