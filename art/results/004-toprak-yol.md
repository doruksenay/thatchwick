unity-import: pending

# 004 Toprak yol seti

- Araç: `create_path_tiles`, isometric, 64 px, açı 30, derinlik 0, outline modu varsayılan, palet = kabul edilen `iso_ground_grass_a` ve `iso_ground_dirt_a` renkleri (8 renk). Gerekçe: tarif.
- Çağrı: 1 (10 generation, araç açıklamasıyla aynı, `cost_est` 10). Tile id: 9c7d946f-7c74-448c-aa71-1398194e12d9.
- Harcanan: 10. `get_balance` sonrası: 4738,9 (paralel şeritler aynı bakiyeden harcadığı için bu iş dışı harcamalar da içinde).
- Yan yana sayfası: art/results/004-yanyana.png
- Masaüstü kopyası: yerel

## Çıktı ve kırpma
Ham çıktı 64x64 tuvaldi; elmas 64x32 olarak 16-48. satırlarda duruyordu. 16-48 kırpıldı, ardından 2:1 maske dışındaki 32 köşe/kenar piksel silindi (içerik yeniden boyanmadı). Ham dosyalar teslim edilmedi.

## Topoloji
tile_0 mask 0 (düz çimen), tile_1 mask 0 (düz çimen varyant), tile_2 N(1), tile_3 E(2), tile_4 S(4), tile_5 W(8) (yol sonu),
tile_6 E+W(10), tile_7 N+S(5) (düz), tile_8 E+S(6), tile_9 S+W(12), tile_10 N+E(3), tile_11 N+W(9) (köşe),
tile_12 E+S+W(14), tile_13 N+S+W(13), tile_14 N+E+W(11), tile_15 N+E+S(7) (T), tile_16 N+E+S+W(15) (kavşak),
tile_17 tamamı toprak (stamp-only, kenar kuralı yok). Bit: 0=N 1=E 2=S 3=W (PixelLab get_tiles_pro).
Dosya numarası = tile_N.

## Dosyalar (pivot x, pivot y alttan)
| Dosya | Boyut | Pivot x | Pivot y | validate_iso --kind ground |
|---|---|---|---|---|
| Assets/Art/Iso/iso_ground_path_dirt_00.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_01.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_02.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_03.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_04.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_05.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_06.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_07.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_08.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_09.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_10.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_11.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_12.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_13.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_14.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_15.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_16.png | 64x32 | 32 | 0 | geçti |
| Assets/Art/Iso/iso_ground_path_dirt_17.png | 64x32 | 32 | 0 | geçti |

## Stil denetimi (zemin ailesi, kilit kurulmadı: palet uyumu yalnızca uyarı)
GEÇTİ: 01, 06, 08, 12, 14, 17. UYARI (teslim edildi): 00, 02, 03, 04, 05, 07, 09, 10, 11, 13, 15, 16 (palete uzak ya da palet dışı renk; doygunluk x0,92-0,97, yani ailenin altında). KALDI yok. Ayrıntılı tablo yan yana sayfasının üretildiği `style.py check` çıktısındadır.

## Döşeme testi
8x8 elmas ızgara, adım (32,16); tile_17, tile_0 ve tile_1 için denendi. Dikiş parlaklık oranı (kenar piksel ortalaması / iç ortalaması): 17: 0,737; 0: 0,779; 1: 0,758. Kenarlar iç alandan %22-26 daha koyu: karolarda koyu bir iç çerçeve (outline modu) var, döşenince ızgara çizgisi görünür olabilir. Mevcut zemin karolarıyla (grass_a vb.) kıyas için kullanıcı yan yana sayfasına bakmalı. Gölge pişirilmemiş, ışık sol üstten.

## Sorunlar / öneri
- Dikiş çizgisi koyu: `outline_mode=segmentation` ("kenar çizgisi yok") ile yeniden üretim önerilir (10 gen), yalnızca kullanıcı isterse.
- tile_0 ile tile_1 ikisi de mask 0 (çimen varyantı); tile_17 yalnızca toprak.
- Çimen tonu mevcut grass_a'dan biraz farklı olabilir (palet aynı, doku farklı).
