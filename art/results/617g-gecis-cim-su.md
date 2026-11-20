unity-import: pending

# 617g Geçiş seti: çim ↔ su (14 maske x 2 çeşit, 28 karo)

- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation` (004e/613g/616g ayarları). Palet 8 renk: grass_a (`#75b560 #639d54 #528647 #76af64`) + water_a (`#639292 #67958d #90d6be #659d96`). Birinci zemin çim.
- 2 paralel çağrı, her biri 10 generation (toplam 20; `cost_est` 22 altında). Hız sınırı hatası yok.

| Çeşit | Tile id | seed | boundary_seed | amplitude |
|---|---|---|---|---|
| a | 2ac5dc76-4e9c-4b90-9fe7-bebba33f275c | 6171 | 71 | 0,25 |
| b | e97f125c-de4e-4edb-9525-c099ad337cea | 6272 | 72 | 0,30 (araç üst sınırı; tarifteki 0,35 kabul edilmedi) |

- Maske eşlemesi 004e ile aynı (bit0 üst, 1 sağ, 2 alt, 3 sol; 1 = su). PixelLab set bit = çim (NW=8, NE=4, SW=2, SE=1). tile_0 (tam su) ve tile_15 (tam çim) teslim edilmedi (zaten var; 15 = ikinci zemin dolgusu tarifte 612/346'dan). Aynalama/döndürme yok.
- Ham karolar tam dikdörtgen (64 maske dışı opak piksel); yalnızca grass_a elmas maskesiyle kırpıldı, içerik boyanmadı.
- Gölge pişirilmemiş, ışık sol üstten (gözle).
- `get_balance` sonrası: 9782 (paralel şeritlerin harcamaları dahil).
- Masaüstü kopyası: yerel
- Yan yana: art/results/617g-yanyana.png

## Dosyalar
28 PNG, hepsi 64x32, pivot x=32, pivot y=16 (alttan), `Assets/Art/Iso/iso_trans_grass_water_<01-14>_<a|b>.png`.

## Doğrulama
| Kontrol | Sonuç |
|---|---|
| `validate_iso.py --kind ground` | 28/28 geçti |
| `style.py check --family zemin` | 26 GEÇTİ, 2 UYARI (b çeşidi 11 ve 13: "palete uzak", doygunluk x1,15 / x1,20), KALDI yok |

## Sorunlar
- Döşeme testi (8x8) ve kenar/iç parlaklık oranı geçiş karoları için yapılmadı (tek başına döşenmez); 12 karoluk kıvrılan kıyı önizlemesi ve a/b karışımında köşe dikişi kod önizlemesinde (`tools/map-preview`) kontrol edilmeli.
- b çeşidinde 2 UYARI, teslim edildi.
