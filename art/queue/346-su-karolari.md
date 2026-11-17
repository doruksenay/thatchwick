---
id: "346"
title: Su karoları: gölet/dere suyu (iki değişke), buz, kıyı
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (su karosu, 2026-11-17); "user" yapınca işçi alır
family: zemin
style_gate: false
cost_est: 10
cost_max: 14
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_water_a.png
  - Assets/Art/Iso/iso_ground_water_b.png
  - Assets/Art/Iso/iso_ground_ice_a.png
  - Assets/Art/Iso/iso_ground_water_shore.png
created: 2026-11-17
---
# 346 Su karoları

## Amaç
Yeni zemin türü `FloorType.Water`: yapı modunda "Gölet kaz" ile açılır, demo haritadaki gölet de bundan. İçine girilir (yürüme x0,5, ıslatır), yıkanılır, kıyıdan balık tutulur, kova doldurulur; değirmen ve sulama kanalı ona değince su alır. Dış sıcaklık -1°C'nin altına inince donar: üstünden yürünür, buz kesilir. Şimdilik motor mavi ve soluk buz rengiyle çiziyor.

## Tarif
- Yöntem **002 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları; 64x32 elmas, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.
- Açıklamalar: su a = calm shallow pond water, muted blue-green with soft light ripples; su b = same water with a few small reflections and a lily pad; buz = frozen pond surface, pale blue-white ice with fine cracks and a dusting of snow; kıyı = pond water fading into a muddy bank with a few reeds on one side (yalnızca bir kenar, motor çevirerek kullanır).
- Su karoları yan yana dikişsiz döşenmeli; kıyı karosunun kara kenarı çim karosuna dikişsiz geçmeli.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`, 3x3 döşeme testi (su a/b karışık + bir kenarda kıyı ve çim; buz 3x3).

## Notlar
Kabulden sonra kod işi: `IsoArt.Ground` su ve buz karolarını döndürsün (şimdi `WorldView` su için renk kullanıyor), kıyı karosu kenar yönüne göre seçilsin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
