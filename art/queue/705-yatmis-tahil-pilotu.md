---
id: "705"
title: Yatmış tahıl pilotu (buğday, arpa, çavdar, yulaf)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: bitki
style_gate: false
cost_est: 35
cost_max: 55
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_plant_crop_wheat_lodged_se.png
  - Assets/Art/Iso/iso_plant_crop_barley_lodged_se.png
  - Assets/Art/Iso/iso_plant_crop_rye_lodged_se.png
  - Assets/Art/Iso/iso_plant_crop_oats_lodged_se.png
created: 2026-11-20
---
# 705 Yatmış tahıl pilotu (buğday, arpa, çavdar, yulaf)

## Amaç
Belge 57d §2: tahıl yatması görsel olarak işlenmeli. Birinci yol shader (`_Lodge`, satır kaydırma); bu iş ikinci yolun (ayrı sprite) pilotu: shader Unity'de yeterince iyi görünmezse bu kareler kullanılır.

## Tarif
Bitki ailesi stil eşlemesi; her tahılın olgun karesi referans, aynı tuval ve pivot. Saplar güneydoğuya doğru yatmış, başaklar yere değiyor, sap dipleri görünür.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family bitki <dosya>`.

## Notlar
Kod: `PlantSystem.IsLodged(item)`, `LifeSystems.CerealsHit` olayı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
