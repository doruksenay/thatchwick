---
id: "418p2"
title: "Tuzlu et ikonu (418p'de reddedilen v2 yerine v3)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20: "tuzlu eti tekrar üret"
priority: 20
family: arayuz
style_gate: false
cost_est: 1
cost_max: 4
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_salted_meat_v3.png
created: 2026-11-20
---
# 418p2 Tuzlu et ikonu

## Neden
418p'deki `icon_item_salted_meat_v2` reddedildi: düz kahverengi bir blok gibi okunuyor, tuz taneleri neredeyse görünmüyor.

## Tarif
418p'nin tarifi (tuval 32x32, kenar, ışık yönü, donmuş arayüz paleti `art/style/palette_arayuz.json`); stil referansı 418p'de kabul edilen `icon_item_meat_v2` ve `icon_item_game_meat_v2`.
- Tuzlanmış et parçası: koyu kırmızı-kahve et, kenarda krem yağ şeridi, **yüzeyde açıkça seçilen beyaz-gri iri tuz taneleri** (6-10 tane, 1-2 piksellik).
- Çiğ etten (`icon_item_meat_v2`) ayırt edilebilmeli: daha koyu, daha kuru, tuzlu.
- Düz dikdörtgen blok olmasın; hafif düzensiz kesik biçim. Yüz, göz, ağız yok.

## Doğrulama
`python tools/art/style.py check --family arayuz <dosya>` GEÇMELİ. Yan yana sayfası `art/results/418p2-yanyana.png`: eski | reddedilen v2 | v3 | `icon_item_meat_v2`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
