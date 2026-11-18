unity-import: pending

# 360 Kazı kaya karoları: rapor

- Araç: `create_tiles_pro` (şekil modu), tek çağrı, 8 numaralı tarif. isometric, 64 px, `tile_view_angle=30`, `tile_depth_ratio=0.3`, `tile_flat_top_px=2`. Stil modu denenmedi: referans 64x32 düz elmas, oysa blok için yan yüz gerekiyor (şekil kontrolleri stil modunda yok sayılır).
- Maliyet: 10 generation (aracın `charged` değeri). `cost_est` 12, `cost_max` 18.
- Çağrı 16 varyasyon döndürdü (8 çift). Her türden bir tane seçildi; kullanılmayanlar PixelLab'de duruyor (id `8f38bd98-b84a-4c98-b90c-05d10c826e93`).
- Kalan (get_balance, paralel şeritler dahil): 4581,9
- Masaüstü kopyası: yerel.
- Yan yana sayfası: [360-yanyana.png](360-yanyana.png)

## Dosyalar
Tuval 64x64; elmas y 11-42 (64x32), yan yüz 10 px (y 43-52, alt opak satır 52). Pivot elmas merkezi: x=32, y=37 ALTTAN (üstten y=27).

| Dosya | Boyut | Pivot x | Pivot y (alttan) | Seçilen varyasyon |
|---|---|---|---|---|
| iso_rock_soil_a.png | 64x64 | 32 | 37 | 0 |
| iso_rock_soft_a.png | 64x64 | 32 | 37 | 2 |
| iso_rock_stone_a.png | 64x64 | 32 | 37 | 4 |
| iso_rock_hard_a.png | 64x64 | 32 | 37 | 6 |
| iso_rock_bedrock_a.png | 64x64 | 32 | 37 | 8 |
| iso_rock_rubble_a.png | 64x64 | 32 | 37 | 10 |
| iso_rock_cracked_a.png | 64x64 | 32 | 37 | 13 |
| iso_rock_dug_floor_a.png | 64x64 | 32 | 37 | 14 |

## Doğrulama
`validate_iso.py` (object): 8/8 geçti (saydam piksel kenarı temiz, yarı saydam yok, 12-23 renk). `--kind ground` 64x32 şart koştuğu için kesik bloklarda başarısız olur; bu beklenen, blok 64x64 tuvaldir.
Yan yüz yüksekliği: x=0 ve x=63 sütunlarında y 26-37 → yan 10±2 px, tamam. Üst yüz elmasla hizalı (x=32'de y 11 üst, sol/sağ uç y 26-27).

Stil denetimi (`zemin` ailesi kilidi kurulmadı, palet uyumu yalnızca uyarı):

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk/aile | Sonuç |
|---|---|---|---|---|
| bedrock | %100 | 8.5 | x0.18 | UYARI soluk |
| cracked | %100 | 6.9 | x0.17 | UYARI soluk |
| dug_floor | %100 | 4.5 | x0.80 | GEÇTİ |
| hard | %100 | 9.6 | x0.15 | UYARI soluk |
| rubble | %100 | 4.3 | x0.45 | UYARI soluk |
| soft | %100 | 4.2 | x0.47 | UYARI soluk |
| soil | %100 | 4.7 | x0.75 | GEÇTİ |
| stone | %100 | 12.4 | x0.11 | UYARI palete uzak, soluk |

Solukluk kaya türleri için istenen "kasvetli ve soluk" ile uyumlu; stone gri olduğu için paletten en uzak.
Döşeme testi: 8x8 elmas ızgara (32,16 adımı) geçici olarak üretildi, üst üste binme kusursuz oturuyor (yan yüz bir alttaki sıranın üstüne biniyor, blok doğası gereği). Bloklarda koyu çerçeve olduğundan düz zemin gibi dikiş oranı anlamlı değil, bu yüzden parlaklık oranı ölçülmedi (kesik blok, düz zemin değil).

## Sorunlar
- `iso_rock_cracked_a.png`: tarif "glowing-free" idi, ama çıktıda turuncu kor çatlakları var (iki varyasyonda da). Duman lekesi ve çatlak var; kor rengi kod tarafında ya da yeni bir işte giderilebilir. Teslim edildi, kullanıcı karar versin.
- `iso_rock_dug_floor_a.png`: tarif "düz zemin" idi; araç 10 px yan yüzlü ince bir blok çizdi (diğerlerinden alçak değil, aynı kalınlık). Üst yüz doğru dokulu; kod tarafı yan yüzü gizleyebilir. Düz 64x32 gerekirse kırpma (maske) ile üst yüz alınabilir.
- `iso_rock_rubble_a.png`: tarife uygun moloz yığını, üstü 1 px taşıyor (y=10).
- Sınır: stil kilidi yok, bu yüzden stil denetimi yalnızca uyarı verir.
