---
id: "617g"
title: "Geçiş seti: çim ↔ su (16 maske x 2 çeşit)"
status: review
claimed_at: 2026-11-20T23:54Z
approved_by: user        # kullanıcı onayı 2026-11-20 ("dediklerini uygula tamamen"; belge 64a §4 Aşama A madde 6)
priority: 10        # oyun dünyası (art/budget.md Öncelik); tester raporu 66
family: zemin
style_gate: false
cost_est: 22
cost_max: 34
depends_on: ["613g"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_trans_grass_water_<NN>_<v>.png (NN 01-14, v = a/b; 15 = ikinci zeminin dolgusu, 612/346'dan)
created: 2026-11-20
---
# 617 Geçiş seti: çim ↔ su (çamurlu kıyı, saz ve kök; bugünkü tek kıyı karosunun yerini alır)

## Amaç
Belge 64 §8 Z2. Taslaktaki zeminler arasında yumuşak geçiş (dual-grid, köşe maskesi; 004e düzeni). 613 pilotunun stili.

## Tarif
Araç: `create_tiles_pro`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`,
004e ile aynı ayarlar ve palet (referans: `iso_ground_grass_a`, `iso_ground_dirt_a`). Aynalama ve döndürme yok.
- 14 maske (01-14) x 2 çeşit (a, b); iki ayrı çağrı, farklı tohum ve `boundary_amplitude` (0.25 / 0.35).
- Geçiş: çim ↔ su (çamurlu kıyı, saz ve kök; bugünkü tek kıyı karosunun yerini alır). Kenar konturu asimetrik, her çeşitte farklı; köşe noktalarındaki geçiş konumu aynı (karolar birleşir).
- Ad: `iso_trans_grass_water_<NN>_<v>`; bit 1 = `water`. Kod bu adı okuyacak (bağlama işi 64b'de listeli).

## Doğrulama
`validate_iso.py --kind ground`; yan yana sayfası (14 maske x 2); 12 karoluk kıvrılan geçiş önizlemesi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-21: çift iş numarası "617g" yapıldı (tester raporu 66). -->
