unity-import: pending

# 004e Toprak yol, çim geçiş seti (asimetrik, 3 çeşit, 48 karo)

- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`, palet = zemin ailesi 8 renk (grass_a + dirt_a; `#75b560 #639d54 #528647 #76af64 #8a6844 #9a7650 #634a30 #77583a`).
- 3 paralel çağrı, her biri 10 generation (toplam 30; `cost_est` 32 altında). Hız sınırı hatası olmadı.

| Çeşit | Tile id | Tohum | boundary_seed | amplitude | Ayrıntı |
|---|---|---|---|---|---|
| a | 39c9704e-b8e3-4ffd-8ffa-7cd017cd7524 | 4101 | 11 | 0,25 | sarkan uzun çim tutamları, seyrek çakıl |
| b | 83397f59-c312-4a8e-8c36-5eb58178e6d8 | 4202 | 22 | 0,30 | kısa kırpılmış çim, ezilmiş toprak, kökler |
| c | 743da07e-224e-4493-93be-db268ff7355f | 4303 | 33 | 0,30 | çim öbekleri, yabani ot, taşlar |

- **Sapma:** tarif b için 0,35 istiyordu; araç en çok 0,3 kabul ediyor, b 0,30 ile üretildi.
- Aynalama, çevirme, döndürme yok; her karo PixelLab'in kendi çıktısı. Karolar yalnızca elmas maskesiyle kırpıldı (karo başına 64 maske dışı piksel silindi, içerik yeniden boyanmadı).
- Maske eşlemesi (004c notuyla aynı; PixelLab set bit = çim): iş dosyası NN: bit0=K, bit1=D, bit2=G, bit3=B, 1=toprak; `tile_N` çim köşeleri `NW=8, NE=4, SW=2, SE=1`. NN = (!N&8 ? 1:0) | (!N&4 ? 2:0) | (!N&1 ? 4:0) | (!N&2 ? 8:0). tile_0 = tam toprak (kare içi toprak rengi), tile_15 = tam çim doğrulandı.
- Tam toprak (NN=15) = `fill_v2_a/_b/_c`; wang_15 ayrıca yazılmadı. Tam çim (NN=00) `wang_00_a/_b/_c` olarak teslim edildi.
- Masaüstü kopyası: yerel
- Harcanan 30. `get_balance` sonrası: 3518,9 (paralel şeritlerin harcamaları da içinde).
- Yan yana: art/results/004e-yanyana-izgara.png (16 maske x 3 çeşit, etiketli; asıl). Ayrıca `style.py sheet` çıktısı art/results/004e-yanyana.png (tek satır 13496 px, referanslarla; okunması zor).

## Dosyalar
48 PNG, hepsi 64x32, pivot x=32, pivot y=16 (alttan; elmas merkezi), `Assets/Art/Iso/`:
`iso_path_dirt_wang_00..14_{a,b,c}.png` (45) ve `iso_path_dirt_fill_v2_{a,b,c}.png` (3). `fill_a` (004c) olduğu gibi duruyor.

## Doğrulama
| Kontrol | Sonuç |
|---|---|
| `validate_iso.py --kind ground` | 48/48 geçti (8 renk, maske dışı 0, delik 0) |
| `style.py check --family zemin` | 46 GEÇTİ, 2 UYARI, KALDI yok |
| Döşeme / kenar-iç parlaklık (dolgu) | a 0,889, b 0,959, c 0,955 (hedef 0,97-1,03: **tutmadı**) |

UYARI (teslim edildi, palete uzak): wang_00_c (%87, ort. uzaklık 13,7, x0,90), wang_01_c (%94, 11,6, x0,91). Tam tablo: art/tmp/004e/style.txt (commit edilmedi).
Gölge pişirilmemiş, ışık sol üstten. 8x8 döşeme PNG'leri art/tmp/004e/tile_{a,b,c}.png (commit edilmez).

## Sorunlar
- Dolgu kenar/iç oranı hedefin altında, özellikle a (0,889: kenar ~%11 koyu). Birleşik döşemede ızgara çizgisi seçilebilir; b/c 0,96 civarı. `style.py fit` doygunluk düzeltir, kenar parlaklığını düzeltmez; kenar pikselini içe göre düzelten araç yok, bu yüzden yeniden boyama yapılmadı. Öneri: koda bağlamadan önce kullanıcı görsel karar versin, gerekirse `tools/art`'a kenar parlaklık düzeltmesi (PR) eklensin.
- Çeşitler arası kenar konturu belirgin farklı (görsel kontrol: izgara). Köşe geçiş noktalarının çeşitler arası birebir aynı olduğu piksel düzeyinde ölçülmedi, yalnızca gözle bakıldı; üç çağrı ayrı tohumla üretildiği için çeşit karıştırma dikişinde kayma olabilir. Kod önizlemesinde (12 karo kıvrılan yol) kontrol edilmeli.
