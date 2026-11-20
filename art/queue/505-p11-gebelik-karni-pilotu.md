---
id: "505"
title: P11 gebelik karnı pilotu (betik)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["501"]
max_attempts: 1
pixellab: false
outputs:
  - art/reports/505-gebelik-karni.md
created: 2026-11-20
---
# 505 P11 gebelik karnı pilotu (betik)

## Amaç
Belge 51 §2.7: gebeliğin son 2 gününde karın betikle büyür (`ClothingMasks.Belly`, 0 gen).

## Tarif
PixelLab çağrılmaz. Aile 2 (entari) mankeninin yan yönlerinde 1 ve 2 piksel karın önizlemesi; yürüme kareleri.

## Doğrulama
Yürürken karın şekli titremiyor.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
