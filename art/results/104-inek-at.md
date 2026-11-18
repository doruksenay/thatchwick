unity-import: pending

# 104 Hayvanlar 4: inek + at — sonuç

- Durum `review`, ama **hiçbir aday Assets'e teslim edilmedi** (8 adaydan 4'ü güney kontrolünde elendi, 3'ü zemin lekesiyle, 1'i stil denetiminde).
- Harcanan: **12,4 generation** (pixflux 8 + v3 döndürme 4 + reduce_colors 4 x 0,1). `get_balance` öncesi 1788,8; sonrası 1760,4. Fark paralel şeritlerin harcamasını da içerir, bu işin payı 12,4.
- Masaüstü kopyası: yerel (bulut işçisi, yapılamadı).
- Not: palet referansı (`iso_char_farmer_a2_56_cand_se.png`) base64 ile bozuk çözüldü; `get_upload_url` ile yüklenip `color_image_url` olarak verildi (ek maliyet yok). v3 döndürme maliyeti çağrıdan önce araç açıklamasından okundu: `ceil(w*h*8/65536)` = 1 (60x60 ve 76x76), tarifle uyumlu.

## Çağrılar
| Aday | Çağrı | Gen |
|---|---|---|
| cow_1..4, horse_1..4 | create_image_pixflux (60x60 / 76x76, south, high top-down, palet zorlama) | 8 |
| cow_1, cow_2, cow_3, horse_4 | create_character v3 ref, quadruped, template horse, 8 yön | 4 |
| cow_1, cow_2, cow_3, horse_4 | reduce_colors, 8 yön birlikte, 32 renk, dithering yok | 0,4 |

## Güney kontrolü (tek kare; baş ve alt gövde x merkezi farkı)
| Aday | Sonuç |
|---|---|
| cow_1 | geçti (baş 29,0 / alt 28,5) |
| cow_2 | geçti (29,0 / 27,1; 1,9 px) |
| cow_3 | geçti (29,5 / 30,6) |
| cow_4 | **geçmedi**: baş 27,1, alt gövde 30,3 (3,2 px, >3), 3/4 dönük. Döndürülmedi |
| horse_1 | **yandan** (baş 26,6, alt 41,0). Döndürülmedi |
| horse_2 | **yandan** (23,8 / 42,8). Döndürülmedi |
| horse_3 | **yandan/3-4** (21,6 / 44,2). Döndürülmedi |
| horse_4 | geçti (37,2 / 36,9) |

cow_4 ve horse_1-3 tek kareleri `/tmp` altında kaldı (bulut, kalıcı değil); gerekirse yeniden üretilmeli. Yeniden deneme yapılmadı.

## Döndürülen adaylar ve elenme nedeni
Ham ortak renk (8 yön): cow_1 39, cow_2 35, cow_3 46, horse_4 45; hepsi 32'den fazlaydı, `reduce_colors` uygulandı, sonuç ortak palet 32. Alfa değişmedi (yarı saydam 0).

| Aday | Tuval | SE boy / uzunluk | Hedef sapması | Doluluk (SE boy/tuval) | Pivot y (alttan) oynama | Zemin lekesi | Teslim |
|---|---|---|---|---|---|---|---|
| cow_1 | 60 | 49 / 43 (E: 49 uzunluk) | +2 % (hedef 48) | 0,82 | 5-6 | **var** (ayak altı kahverengi gölge) | hayır |
| cow_2 | 60 | 52 / 51 (E: 52 uzunluk) | +8 % | 0,87 | 4-6 | yok | **hayır: stil denetimi KALDI** |
| cow_3 | 60 | 56 / 42 (E: 52) | +17 % | 0,93 | 2 | **var** (mavi-gri su/gölge lekesi) | hayır |
| horse_4 | 76 | 69 / 64 (E: 68) | +15 % (hedef 60) | 0,91 | 3-4 | **var** (kahverengi gölge) | hayır |

Zemin lekesi olan adaylar maskeyle silinmedi (kural). Pivot x (SE): cow_1 26,5; cow_2 26,5; cow_3 30,0; horse_4 43,0.

## cow_2 stil denetimi (`style.py check --family hayvan`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk / aile | Sonuç |
|---|---|---|---|---|
| cow_2 se | %100 | 7,8 | x1,67 | KALDI (ailesinden çok doygun) |
| cow_2 sw | %100 | 8,0 | x1,67 | KALDI |
| cow_2 nw | %100 | 6,6 | x1,58 | KALDI |
| cow_2 ne | %100 | 6,7 | x1,59 | KALDI |
| cow_2 s | %100 | 7,5 | x1,61 | KALDI |
| cow_2 w | %100 | 7,3 | x1,68 | KALDI |
| cow_2 n | %100 | 6,5 | x1,54 | KALDI |
| cow_2 e | %100 | 7,4 | x1,67 | KALDI |

`validate_iso.py` cow_2 SE: geçti (60x60, 28 renk, yarı saydam 0, alt boşluk 4). Yan yana sayfası: `art/results/104-yanyana.png` (cow_2 SE + E, referanslarla).
Diğer adaylar için stil denetimi yapılmadı (zemin lekesi nedeniyle zaten teslim dışı).

## Gözlemler
- Uzunluk sınırı: 60 px inek tuvalinde E/W uzunluğu 49-52 px, tuvale sığdı, kırpılma yok (kenara değen piksel yok). Hedef uzunluk ~70'e ulaşmıyor; at 68 (E) hedefe yakın.
- Boylar hedefin üstünde (inek +2..+17 %, at +15 %); sonraki işte inek tuvali ~52, at ~66 denenebilir (tahmin).
- Pixflux 3 adayda (cow_1, cow_3, horse_4) ayak altı zemin/gölge pişirdi; "no shadow, no dirt under feet" açıklamasına rağmen. Sonraki denemede açıklamayı yumuşatmak ya da zemini döndürme sonrası pixelart_workbench ile ayak altı lekeyi maskelemek için kullanıcı izni gerekir.
- Hiçbiri Assets'e girmedi; kullanıcı kararı gerekir: (a) cow_1/cow_3/horse_4'ün lekesinin maskeyle silinmesine izin, (b) cow_2 için stil KALDI'sına istisna, (c) yan çıkan horse_1-3 ve cow_4 için yeni üretim (yeni iş dosyası).
