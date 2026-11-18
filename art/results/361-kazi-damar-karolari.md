unity-import: pending

# 361 Kazı damar karoları: rapor

- Araç: `create_tiles_pro` (şekil modu), tek çağrı, 8 numaralı tarif; 360 ile aynı ayarlar (isometric, 64 px, `tile_view_angle=30`, `tile_depth_ratio=0.3`, `tile_flat_top_px=2`). Stil modu kullanılmadı (yan yüz kaybolur); taban "gri taş blok" metniyle tarif edildi.
- Maliyet: 10 generation (`charged`). `cost_est` 12, `cost_max` 18. Tile id `78d70de1-50fb-4235-a914-a7945a39df14` (16 varyasyon, her türden çift; çift numaralılar seçildi).
- Kalan (get_balance): 4561,9
- Hint karosu: PixelLab'siz, Python ile çizildi (64x32, 35 piksel, 3 soluk bej-gri ton, elmas kenarında; kod damara göre boyar).
- Masaüstü kopyası: yerel.
- Yan yana sayfası: [361-yanyana.png](361-yanyana.png)

## Dosyalar
Tuval 64x64, elmas y 11-42, yan yüz 10 px, en alt opak satır 52. Pivot elmas merkezi x=32, y=37 ALTTAN (360 ile aynı).

| Dosya | Boyut | Pivot x | Pivot y (alttan) | Varyasyon |
|---|---|---|---|---|
| iso_rock_vein_limestone_a.png | 64x64 | 32 | 37 | 0 |
| iso_rock_vein_flint_a.png | 64x64 | 32 | 37 | 2 |
| iso_rock_vein_clay_a.png | 64x64 | 32 | 37 | 4 |
| iso_rock_vein_ochre_a.png | 64x64 | 32 | 37 | 6 |
| iso_rock_vein_coal_a.png | 64x64 | 32 | 37 | 8 |
| iso_rock_vein_iron_ore_a.png | 64x64 | 32 | 37 | 10 |
| iso_rock_vein_gypsum_a.png | 64x64 | 32 | 37 | 12 |
| iso_rock_vein_slate_stone_a.png | 64x64 | 32 | 37 | 14 |
| iso_rock_vein_hint.png | 64x32 | 32 | 16 | (betik) |

## Doğrulama
`validate_iso.py`: 9/9 geçti (yarı saydam yok, 21-30 renk). Yan yüz 10 px (y 43-52).

Stil denetimi (`zemin` kilidi kurulmadı, yalnızca uyarı):

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk/aile | Sonuç |
|---|---|---|---|---|
| clay | %100 | 8.6 | x0.90 | GEÇTİ |
| coal | %100 | 7.1 | x0.16 | UYARI soluk |
| flint | %100 | 9.7 | x0.15 | UYARI soluk |
| gypsum | %100 | 8.3 | x0.22 | UYARI soluk |
| iron_ore | %100 | 5.1 | x0.81 | GEÇTİ |
| limestone | %100 | 5.8 | x0.82 | GEÇTİ |
| ochre | %100 | 6.1 | x0.85 | GEÇTİ |
| slate_stone | %97 | 13.6 | x0.36 | UYARI palete uzak, soluk |

Soluk uyarıları gri taş tabanından; kasvetli ton istenen yönle uyumlu.

## Sorunlar
- Tarif "360 çıktısı referans" diyordu; referans görseli verilmedi, taban metinle tarif edildi. Gri ton `iso_rock_stone_a` ile birebir aynı olmayabilir.
- Limestone ve ochre taşın büyük kısmını kaplıyor (şerit yerine geniş katman).
- Hint karosu seyrek (35 piksel); kod boyayacağı için yeterli sayıldı.
- Döşeme testi: kesik blok olduğundan dikiş oranı anlamlı değil (360 ile aynı gerekçe).
