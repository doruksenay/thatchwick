---
id: "503"
title: P9 baston pilotu (el çapası)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: karakter
style_gate: true
cost_est: 40
cost_max: 55
depends_on: ["501"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/Characters/lay_cane_plain_s.png
created: 2026-11-20
---
# 503 P9 baston pilotu (el çapası)

## Amaç
Belge 51 §3.4 ve §5 P9: düz baston, el çapasına (`ClothingMasks.Hand`, 501'deki bilek noktası) bağlı B parçası.

## Tarif
Yürüyen yetişkin mankeninde 8 yön (5 benzersiz + ayna); baston eli izler, yere değme anı adımla uyumlu olsun. Anahtar renk: deri/ahşap referansı (`KeyColors.Leather`).

## Doğrulama
Baston eli izliyor; uymuyorsa raporla: baston yalnız durmada görünür, yürüme hız etkisiyle kalır.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
