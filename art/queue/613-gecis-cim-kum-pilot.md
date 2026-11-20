---
id: "613g"
title: "Geçiş seti pilotu: çim ↔ kum (4 karo, stil kapısı)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("dediklerini uygula tamamen"; belge 64a §4 Aşama A madde 6)
priority: 10        # oyun dünyası (art/budget.md Öncelik); tester raporu 66
family: zemin
style_gate: true
cost_est: 6
cost_max: 10
depends_on: ["612"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_trans_grass_sand_01_a.png
  - Assets/Art/Iso/iso_trans_grass_sand_03_a.png
  - Assets/Art/Iso/iso_trans_grass_sand_05_a.png
  - Assets/Art/Iso/iso_trans_grass_sand_07_a.png
created: 2026-11-20
---
# 613 Geçiş seti pilotu: çim ↔ kum

## Amaç
Belge 64 §8 Z2. Dört geçiş seti (614-617) bu pilotun stiline bağlı. Dual-grid düzeni 004e ile aynı: köşe maskesi
bit 0 üst, 1 sağ, 2 alt, 3 sol; 1 = ikinci zemin (kum).

## Tarif
Araç: `create_tiles_pro`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`,
004e ile aynı ayarlar ve palet (referans: `iso_ground_grass_a`, `iso_ground_dirt_a`). Aynalama ve döndürme yok.
- Yalnızca 4 maske: 01 (tek köşe), 03 (bir kenar), 05 (karşı köşeler), 07 (üç köşe). Kenar konturu düzensiz, asimetrik
  (004c'nin reddedilme nedeni simetriydi); kumda ıslak kenar çizgisi yok, çim tutamları kuma taşar.
- Palet: `iso_ground_grass_a` + 612'nin `iso_ground_sand_a`.

## Doğrulama
`validate_iso.py --kind ground`; 12 karoluk kıvrılan kıyı önizlemesi (kod oturumu ya da `tools/map-preview`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-21: çift iş numarası "613g" yapıldı (tester raporu 66). -->
