---
id: "501"
title: P7 kesim maskesi pilotu (eklem çizgileri)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: karakter
style_gate: true
cost_est: 15
cost_max: 25
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - art/reports/501-kesim-maskesi.md
created: 2026-11-20
---
# 501 P7 kesim maskesi pilotu (eklem çizgileri)

## Amaç
Belge 48 §4.1: gövde, alt, ayak ve el modelleri çizimsizdir; bölge haritası eklem çizgileriyle alt bölgelere ayrılır (kod: `Assets/Scripts/Core/Customization/Layers/ClothingMasks.cs`).

## Tarif
P1 yetişkin mankeninin bölge haritalarında (8 yön, durma ve yürüme) dirsek, bilek, bel, diz, ayak bileği satırlarını bul (PixelLab `estimate-skeleton` ya da 43c çapa aracının genişletmesi) ve `tools/art/manken.py` ile mavi kanala işaret olarak yaz (3 omuz, 4 dirsek, 5 bilek, 6 bel, 7 diz, 8 ayak bileği). Kısa kol, kolsuz yelek (altında gömlek), yarım çizme ve önlük için `ClothingMasks.ApplyCuts` önizlemesini kare kare üret ve rapora koy.

## Doğrulama
8 yönde ve yürümede kesim çizgisi titremiyor; kısa kolda ten doğal görünüyor. Başarısızsa (belge 48 §13.3) gövde modelleri ~10'a iner; raporda öneri yaz.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
