---
id: "618"
title: Kemik değirmeni ve araba sundurması
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: esya
style_gate: false
cost_est: 25
cost_max: 40
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_bone_mill_se.png
  - Assets/Art/Iso/iso_item_cart_shed_2x1_se.png
created: 2026-11-20
---
# 618 Kemik değirmeni ve araba sundurması

## Amaç
items.json `item.bone_mill` (kemik öğütme istasyonu, belge 57c) ve `item.cart_shed` (altındaki araba yağmurda yıpranmaz, belge 57e §2.2).

## Tarif
Eşya ailesi stil eşlemesi, izometrik 2:1. Kemik değirmeni: taş yalak içinde dik dönen taş, ahşap kol, tek karo. Araba sundurması: dört direk üstünde saz çatı, altı boş (araba yanına park eder), 2x1.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`.

## Notlar
items.json pivotları geçici (`art.pending`); işçi görsel boyutuna göre önerisini raporuna yazar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
