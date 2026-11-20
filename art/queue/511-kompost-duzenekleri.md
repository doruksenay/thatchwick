---
id: "511"
title: Kompost düzenekleri (üç gözlü kompostluk, döner fıçı, solucan kasası, yaprak ağılı)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: esya
style_gate: false
cost_est: 45
cost_max: 60
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_compost_bays_3x1_se.png
  - Assets/Art/Iso/iso_item_compost_tumbler_se.png
  - Assets/Art/Iso/iso_item_worm_bin_se.png
  - Assets/Art/Iso/iso_item_leaf_bin_se.png
created: 2026-11-20
---
# 511 Kompost düzenekleri (üç gözlü kompostluk, döner fıçı, solucan kasası, yaprak ağılı)

## Amaç
Belge 57c §4: kompost süreci için düzenekler. Üç gözlü kompostluk 3x1, ötekiler tek karo.

## Tarif
Eşya ailesi stil eşlemesi, izometrik 2:1. Üç gözlü kompostluk: kalas bölmeli üç göz, birinde taze yeşil atık, birinde koyu yarı olmuş, birinde kara toprak. Döner kompost fıçısı: iki ayak üstünde yatay ahşap fıçı, yanında çevirme kolu (bilinçli dönem istisnası, belge 57c D2). Solucan kasası: kapaklı alçak ahşap sandık. Yaprak ağılı: söğüt örgü silindir, içi kuru yaprak.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`.

## Notlar
items.json pivotları geçicidir; işçi görsel boyutuna göre `pivotX/pivotY/pxW/pxH` önerisini raporuna yazar (items.json'a kod oturumu işler).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
