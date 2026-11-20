---
id: "508"
title: Gözlük: üç çerçeve (göz çapası)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: karakter
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["501"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/Characters/lay_glasses_bone_s.png
  - Assets/Art/Iso/Characters/lay_glasses_brass_s.png
  - Assets/Art/Iso/Characters/lay_glasses_silver_s.png
created: 2026-11-20
---
# 508 Gözlük: üç çerçeve (göz çapası)

## Amaç
Belge 51 §3.4: kemik, pirinç, gümüş çerçeveli gözlük; yalnız yüzün göründüğü yönler (s, se, e), göz çapasına (`ClothingMasks.Glasses`) oturur.

## Tarif
PartCanvas 64x96, çapa (32,24) yerine göz merkezi; üç çerçeve aynı mercek, renk ve çerçeve kalınlığı farkı.

## Doğrulama
Gözlük iki göze oturuyor; arkadan görünmüyor.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
