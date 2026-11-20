unity-import: pending

# 612 Zemin türleri: kum, çayır, koyu çim, çakıllı toprak

- Araç: `create_tiles_pro`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`, seed 6121, şekil modu (palet verilmedi: kum tonu grass/dirt paletine sığmaz). Tek çağrı, 16 numaralı karo (10 hedef + 6 yedek). Tile id: `6bcd4aa2-9aaa-4eea-bfba-2dc10daf62dc`.
- Harcanan: 10 gen (çağrı başı 10). `get_balance` sonrası: 1079,6 (ortak bakiye, paralel şeritler dahil).
- Seçim: sand a/b/c = t0/t1/t15, meadow a/b/c = t4/t5/t11, grass_dark a/b = t6/t7, dirt_gravel a/b = t8/t9. t2 ve t3'te maske içinde 1 boş piksel vardı, yedeklerle (t15, t11) değiştirildi.
- PixelLab 64x64 tuval verdi; 64x32 elmas bölgesi kırpıldı, maske dışı 32+ piksel yalnızca maskeyle silindi (içerik yeniden boyanmadı).
- `sand_a`: ham hâli "ailesinden çok doygun" yüzünden KALDI; `style.py fit` ile doygunluk x0,76 indirildi, geçti. Ham hâl `art/results/612-red/iso_ground_sand_a_ham.png`. `sand_b` ve `sand_c` UYARI (doygun, x1,44 / x1,35), teslim edildi (kum doğası gereği açık/sıcak).
- Yan yana sayfası: `art/results/612-yanyana.png`. Döşeme testleri: `art/results/tile_<tür>.png`.
- Masaüstü kopyası: yerel

| Dosya | Boyut | Pivot (x, y alttan) | validate_iso | Stil denetimi | Dikiş oranı |
|---|---|---|---|---|---|
| iso_ground_sand_a.png | 64x32 | 32, 16 | geçti | GEÇTİ (fit x0,76) | 1,016 |
| iso_ground_sand_b.png | 64x32 | 32, 16 | geçti | UYARI doygun | 1,019 |
| iso_ground_sand_c.png | 64x32 | 32, 16 | geçti | UYARI doygun | 1,024 |
| iso_ground_meadow_a.png | 64x32 | 32, 16 | geçti | GEÇTİ | 1,016 |
| iso_ground_meadow_b.png | 64x32 | 32, 16 | geçti | GEÇTİ | 0,988 |
| iso_ground_meadow_c.png | 64x32 | 32, 16 | geçti | GEÇTİ | 0,972 |
| iso_ground_grass_dark_a.png | 64x32 | 32, 16 | geçti | GEÇTİ | 0,840 |
| iso_ground_grass_dark_b.png | 64x32 | 32, 16 | geçti | GEÇTİ | 0,871 |
| iso_ground_dirt_gravel_a.png | 64x32 | 32, 16 | geçti | GEÇTİ | 0,966 |
| iso_ground_dirt_gravel_b.png | 64x32 | 32, 16 | geçti | GEÇTİ | 1,000 |

Dikiş oranı = kenar bandı parlaklığı / iç ortalama (8x8 ızgara, adım 32,16, çeşitler karışık). Gölge pişirilmemiş, ışık sol-üstten.

## Sorunlar
- `grass_dark` kenar bandı içten %13-16 koyu (0,84 / 0,87): döşemede hafif ızgara izi görünebilir. Karar kullanıcıda (kabul ya da yeniden üretim).
- Çiçek noktaları (meadow) seyrek ve küçük; yan yana sayfasında kontrol edilmeli.
