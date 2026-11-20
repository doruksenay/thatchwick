unity-import: pending

# 616g Geçiş seti: kum ↔ su (14 maske x 2 çeşit, 28 karo)

- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation` (004e/613g ayarları). Palet 8 renk: sand_a (`#dac6a2 #d5c7af #c6b193 #e0cdae`) + water_a (`#639292 #67958d #90d6be #659d96`). Birinci zemin kum.
- 2 paralel çağrı, her biri 10 generation (toplam 20; `cost_est` 22 altında). Hız sınırı hatası yok.

| Çeşit | Tile id | seed | boundary_seed | amplitude |
|---|---|---|---|---|
| a | 51376c0b-8295-4512-b290-96bff811e961 | 6161 | 61 | 0,25 |
| b | 019f0317-7009-425c-aa06-477cb47d9f92 | 6262 | 62 | 0,30 (araç üst sınırı; tarifteki 0,35 kabul edilmedi) |

- Maske eşlemesi 004e/613g ile aynı (bit0 üst, 1 sağ, 2 alt, 3 sol; 1 = su). PixelLab set bit = kum (NW=8, NE=4, SW=2, SE=1). tile_0 (tam su) ve tile_15 (tam kum) teslim edilmedi (zaten `iso_ground_water_*` ve `sand_a` var; 15 tarifte "612/346'dan"). Aynalama/döndürme yok.
- Ham karolar tam dikdörtgen (64 maske dışı opak piksel); yalnızca elmas maskesiyle kırpıldı, içerik boyanmadı.
- Gölge pişirilmemiş, ışık sol üstten (gözle).
- `get_balance` sonrası: 9809 (paralel şeritlerin harcamaları dahil).
- Masaüstü kopyası: yerel
- Yan yana: art/results/616g-yanyana.png

## Dosyalar
28 PNG, hepsi 64x32, pivot x=32, pivot y=16 (alttan), `Assets/Art/Iso/iso_trans_sand_water_<01-14>_<a|b>.png`.

## Doğrulama
| Kontrol | Sonuç |
|---|---|
| `validate_iso.py --kind ground` | 28/28 geçti |
| `style.py check --family zemin` | 19 GEÇTİ, 9 UYARI (hepsi a çeşidi: 03, 05, 06, 07, 09, 10, 12, 13, 14; "palete uzak", 07 ve 13 ayrıca doygun x1,27), KALDI yok |
| Kenar/iç parlaklık (tam karo, dikiş) | a: kum 0,990 / su 1,002; b: kum 0,985 / su 1,001 (hedef 0,97-1,03: tuttu) |

## Sorunlar
- a çeşidinde 9 UYARI (ort. uzaklık 10-14); b temiz. Teslim edildi.
- 12 karoluk kıvrılan kıyı önizlemesi bu işçide yapılmadı; a/b çeşit karışımında köşe dikişi kod önizlemesinde (`tools/map-preview`) kontrol edilmeli.
