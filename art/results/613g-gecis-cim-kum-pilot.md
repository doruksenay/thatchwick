unity-import: pending

# 613g Geçiş seti pilotu: çim ↔ kum (4 karo)

- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`, `boundary_amplitude=0.3`, `boundary_seed=51`, `seed=6131` (004e ayarları; kenar düzensiz/asimetrik). Palet 8 renk: grass_a (`#75b560 #639d54 #528647 #76af64`) + sand_a (`#dac6a2 #d5c7af #c6b193 #e0cdae`). Birinci zemin çim.
- Tile id: 3510e4f6-35e1-4fdf-af2f-55a978fc90f6. 1 çağrı, 10 generation (`cost_est` 6 üstünde, `cost_max` 10 içinde; aracın en düşük fiyatı). Hız sınırı hatası yok. `get_balance` üretimden önce 9954; üretim sonrası ayrıca okunmadı (paralel şeritler harcıyor), defter tahmini 9944.
- Maske eşlemesi 004e ile aynı (bit0 üst, 1 sağ, 2 alt, 3 sol; 1 = kum). PixelLab set bit = çim: 01=tile_7, 03=tile_3, 05=tile_6, 07=tile_2. Tam çim (tile_15) ve tam kum (tile_0) teslim edilmedi (zaten `iso_ground_grass_a`/`sand_a` var). Aynalama/döndürme yok.
- Ham karolar 64x32 tam dikdörtgen geldi (64 maske dışı opak piksel); yalnızca elmas maskesiyle kırpıldı (grass_a alfası), içerik boyanmadı.
- Masaüstü kopyası: yerel
- Yan yana: art/results/613g-yanyana.png

## Dosyalar (hepsi 64x32, pivot x=32, pivot y=16 alttan)
`Assets/Art/Iso/iso_trans_grass_sand_01_a.png`, `_03_a`, `_05_a`, `_07_a`.

## Doğrulama
| Dosya | validate_iso ground | style check | Renk |
|---|---|---|---|
| 01 | geçti | GEÇTİ (%100, 4.1, x0.99) | 8 |
| 03 | geçti | GEÇTİ (%100, 3.4, x1.06) | 7 |
| 05 | geçti | GEÇTİ (%100, 3.1, x1.06) | 8 |
| 07 | geçti | GEÇTİ (%100, 2.6, x1.14) | 7 |

Gölge pişirilmemiş, ışık sol üstten (gözle).

## Sorunlar
- Döşeme testi (8x8 elmas) yapılmadı: geçiş karoları tek başına döşenmez; bunun yerine tam çim karosunun kenar/iç parlaklık oranı ölçüldü: 0,918 (hedef 0,97-1,03, tutmadı; 004e dolgusundaki sorunun aynısı, kenarda koyu şerit). Bağlantılı dikişlerde ızgara çizgisi görünebilir.
- Kıvrılan 12 karoluk kıyı önizlemesi (kod oturumu/`tools/map-preview`) bu işçide yapılmadı; kabulden önce yapılmalı.
- Maske eşlemesi (özellikle 05: üst+alt kum) gözle doğrulandı, kod önizlemesinde yönler teyit edilmeli.
