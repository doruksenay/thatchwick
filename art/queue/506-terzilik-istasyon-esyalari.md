---
id: "506"
title: Terzilik istasyon ve ev eşyaları 1 (dikiş sehpası, kirli sepeti, demlik)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: esya
style_gate: false
cost_est: 40
cost_max: 55
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_sewing_table_se.png
  - Assets/Art/Iso/iso_item_laundry_basket_se.png
  - Assets/Art/Iso/iso_item_teapot_se.png
created: 2026-11-20
---
# 506 Terzilik istasyon ve ev eşyaları 1 (dikiş sehpası, kirli sepeti, demlik)

## Amaç
items.json'daki `art.pending` eşyalar: dikiş sehpası (Terzilik istasyonu), kirli sepeti (Hepsini yıka), toprak demlik (siyah çay).

## Tarif
Eşya ailesi stil eşlemesi (`art/style/references.json`), izometrik 2:1, items.json'daki pivot ve boyutlar. Dikiş sehpası Köylü ahşap, üstünde kumaş ve makas; kirli sepeti söğüt örgü; demlik pişmiş toprak.

## Doğrulama
`python tools/art/validate_iso.py`, `python tools/art/style.py check --family esya`.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
