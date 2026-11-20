---
id: "500"
title: P6 pelerin pilotu (B katmanı, omuz çapası)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: karakter
style_gate: true
cost_est: 40
cost_max: 55
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/Characters/lay_outer_short_cloak_s_fr.png
  - Assets/Art/Iso/Characters/lay_outer_short_cloak_s_bk.png
created: 2026-11-20
---
# 500 P6 pelerin pilotu (B katmanı, omuz çapası)

## Amaç
Giysi sistemi (belge 48 §13.3 P6): pelerinler B katmanında omuz çapasına bağlı ön ve arka parça olarak çizilir. Kısa yün pelerin (`short_cloak`) pilot.

## Tarif
Yetişkin mankeninin (43c) 5 benzersiz yönünde (s, se, e, ne, n; batı aynası) ön (`_fr`) ve arka (`_bk`) parça; PartCanvas sözleşmesi (64x96) yerine gövde tuvali; anahtar renk: kumaş referans gri (#9A9A9A, `KeyColors.ReferenceCloth`) ki rampa takası çalışsın. Omuz çapası `ClothingMasks.Shoulder` (gövde açıklığı ortası, omuz satırı). Yürüme ve bir etkileşim (odun kesme) karelerinde parça kopmuyor mu bak; sallanma `SwaySprite` ile.

## Doğrulama
Kol hareketinde pelerin kopmuyor, sırttan taşmıyor; kapüşon başlık maskesiyle çakışmıyor. `python tools/art/ramps.py check --ref cloth <dosya>`.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
