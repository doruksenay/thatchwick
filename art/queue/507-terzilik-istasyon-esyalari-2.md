---
id: "507"
title: Terzilik ev eşyaları 2 (çay takımı, okuma taşı, biçki kalıbı ikonu)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: esya
style_gate: false
cost_est: 35
cost_max: 50
depends_on: ["506"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_tea_set_se.png
  - Assets/Art/Iso/icon_reading_stone.png
  - Assets/Art/Iso/icon_sewing_pattern.png
created: 2026-11-20
---
# 507 Terzilik ev eşyaları 2 (çay takımı, okuma taşı, biçki kalıbı ikonu)

## Amaç
Çay takımı (cam bardak ve tepsi), okuma taşı (belge 51 §3.4), biçki kalıbı (parşömen).

## Tarif
Eşya ailesi stil eşlemesi; ikonlar envanter ikon ölçüsünde.

## Doğrulama
`python tools/art/validate_iso.py`, `python tools/art/style.py check --family esya`.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
