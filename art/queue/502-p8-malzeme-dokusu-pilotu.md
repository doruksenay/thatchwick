---
id: "502"
title: P8 malzeme dokusu pilotu (betik)
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
  - art/reports/502-malzeme-dokusu.md
created: 2026-11-20
---
# 502 P8 malzeme dokusu pilotu (betik)

## Amaç
Belge 48 §6.5 ve §13.3 P8: keçe pürüzü, örgü ilmeği, kürk kenarı, ipek parıltısı betikle (`ClothingMasks.MaterialShade`).

## Tarif
PixelLab çağrılmaz. 501'in maskeli karelerinde dört malzemeyle önizleme üret (84 px ve 2x büyütme), yürüme karelerinde dokunun kayıp kaymadığını incele.

## Doğrulama
84 px'te malzemeler birbirinden ayırt ediliyor; yürürken doku kaymıyor.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
