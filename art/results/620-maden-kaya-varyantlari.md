unity-import: pending

# 620 Maden kaya ve damar varyantları: rapor

- Araç: `create_tiles_pro` (şekil modu), 360/361 ile aynı ayarlar: isometric, 64 px, `tile_view_angle=30`, `tile_depth_ratio=0.3`, `tile_flat_top_px=2`. Stil modu kullanılmadı (yan yüz kaybolur). İki paralel çağrı, her biri 10 generation (`charged`), toplam 20 (`cost_est` 18, `cost_max` 28).
  - Çağrı A `d5980a2f-ba09-4e5e-a650-c0d2c40fcdcb`: 5 kaya türü + demir, kömür, kireçtaşı (8 numara, 16 varyasyon).
  - Çağrı B `874243cb-f8aa-4885-b9c6-79b159c8371b`: çakmaktaşı, kil, aşı boyası, alçıtaşı, kayrak (5 numara, 16 varyasyon).
- Her numaranın iki varyasyonu var: kaya türlerinde çift numaralı = `b`, tek numaralı = `c`; damarlarda yalnızca çift numaralı = `b`. Kullanılmayanlar PixelLab'de duruyor.
- Tarifteki metinler 360/361'den aynen; referans görsel verilmedi (361 ile aynı sınır): gri taban metinle tarif edildi.
- Kalan (get_balance): 898,6
- Masaüstü kopyası: yerel.
- Yan yana sayfası: [620-yanyana.png](620-yanyana.png) (3 zemin referansı + 18 yeni dosya)
- Karışık döşeme testi: [620-karisik-dosemeler.png](620-karisik-dosemeler.png) (6x6, a/b/c dağınık, taş ve toprak)

## Dosyalar
`Assets/Art/Iso/_aday/` altında. Tuval 64x64, elmas y 11-42, yan yüz 10 px. Pivot x=32, y=37 ALTTAN (360 ile aynı). Hepsi 64x64.

| Dosya | Pivot x | Pivot y (alttan) |
|---|---|---|
| iso_rock_soil_b / _c | 32 | 37 |
| iso_rock_soft_b / _c | 32 | 37 |
| iso_rock_stone_b / _c | 32 | 37 |
| iso_rock_hard_b / _c | 32 | 37 |
| iso_rock_bedrock_b / _c | 32 | 37 |
| iso_rock_vein_iron_ore_b | 32 | 37 |
| iso_rock_vein_coal_b | 32 | 37 |
| iso_rock_vein_limestone_b | 32 | 37 |
| iso_rock_vein_flint_b | 32 | 37 |
| iso_rock_vein_clay_b | 32 | 37 |
| iso_rock_vein_ochre_b | 32 | 37 |
| iso_rock_vein_gypsum_b | 32 | 37 |
| iso_rock_vein_slate_stone_b | 32 | 37 |

## Doğrulama
- `validate_iso.py --kind object`: 18/18 geçti (yarı saydam 0, 11-31 renk, opak sınır x 0-63).
- Stil denetimi (`zemin`, palete yakın %100 hepsinde): GEÇTİ: soil_b, soil_c. Diğer 16 dosya UYARI "ailesinden soluk" (gri kasvetli taş istenen yönle uyumlu; 360/361 ile aynı durum).

| Dosya | Ort. uzaklık | Doygunluk/aile | Sonuç |
|---|---|---|---|
| bedrock_b / _c | 3.7 / 3.6 | x0.31 / x0.32 | UYARI soluk |
| hard_b / _c | 3.7 / 3.6 | x0.13 / x0.14 | UYARI soluk |
| soft_b / _c | 7.0 / 7.0 | x0.36 / x0.37 | UYARI soluk |
| soil_b / _c | 5.1 / 5.2 | x0.82 / x0.86 | GEÇTİ |
| stone_b / _c | 4.5 / 4.6 | x0.09 / x0.08 | UYARI soluk |
| vein_clay_b | 3.7 | x0.07 | UYARI soluk |
| vein_coal_b | 2.7 | x0.20 | UYARI soluk |
| vein_flint_b | 3.7 | x0.08 | UYARI soluk |
| vein_gypsum_b | 3.8 | x0.29 | UYARI soluk |
| vein_iron_ore_b | 3.9 | x0.39 | UYARI soluk |
| vein_limestone_b | 5.5 | x0.56 | UYARI soluk |
| vein_ochre_b | 4.0 | x0.29 | UYARI soluk |
| vein_slate_stone_b | 5.3 | x0.46 | UYARI soluk |

- 2:1 ve ölçü: çıktı doğrudan 64x64 (64x32 elmas + 10 px yan), kırpma gerekmedi.
- Karışık döşeme: taş ve toprakta a/b/c rastgele dağıtıldı; tekrar ve ızgara belirgin değil, desenler farklı okunuyor. Kesik blok olduğundan dikiş parlaklık oranı anlamlı değil (360/361 ile aynı gerekçe).
- Gölge pişirilmemiş, ışık sol-üstten.

## Sorunlar
- Damar karolarında (b) şerit konturu a'dan farklı, ama limestone, ochre, clay yine geniş bir kısmı kaplıyor (361 notu).
- Vein_flint ve vein_gypsum taban taşı a'dakinden biraz daha açık gri; stone_a ile birebir aynı olmayabilir.
- Referans görsel kullanılmadığı için tür renkleri a ile az farklı olabilir; kullanıcı yan yana sayfasına bakarak karar versin.
