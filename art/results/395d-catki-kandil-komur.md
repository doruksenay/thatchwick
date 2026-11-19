unity-import: pending

# 395d Beşinci tur: çatkı, kandil, kömür yığını (rapor)

Sonuç: 2/3 teslim (çatkı, kandil), kömür yığını teslim edilmedi. Yan yana sayfası: [395d-yanyana.png](395d-yanyana.png). Tüm adaylar ve adaylık önizlemeleri (prev_*.png): `art/results/395d-red/`. Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- `create_map_object`, arka plan görseli yok (basic mod), `low top-down`, tarifteki metinler aynen. Parça başına 4 aday (varyasyon: shading medium/basic, detail medium/low). 12 çağrı.
- Hız sınırı: 3 kömür çağrısı ilk denemede "rate limit exceeded" verdi (üretim başlamadı), 15 sn bekleyip bir kez yeniden gönderildi.
- Harcama: get_balance 3737,5 → 3723,5 = 14 (diğer şeritlerle ortak bakiye; çağrı başına ~1,2). cost_max 25 altında. Kalan: 3723,5.
- Çatkıya yerel `style.py fit --family esya` (doygunluk x1.32 → x1.15, 0 gen). Kandile gerek olmadı. `reduce_colors` kullanılmadı.

## Seçimler
| Parça | Seçilen | Gerekçe |
|---|---|---|
| çatkı | aday 1 (medium shading) | iki düz açık kahverengi dikme + bir kiriş, çapraz yok, bordo yok. Ölçüt karşılandı. Diğerleri: 2 koyu kahve, 3 kiriş dikmelere yan yana oturmuş, 4 masa gibi |
| kandil | aday 3 (basic shading) | küçük kil çanak, tek alev, yüksek ayak yok. Çömlek-kıvrım görünümü var. Aday 1 ve 4 ayaklı havan gibi, 2 alevsiz |
| kömür yığını | yok | bkz. aşağıda |

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_item_mine_support_wood_e_se.png | 64x96 | 32 | 7 |
| Assets/Art/Iso/_aday/iso_item_mine_lamp_e_se.png | 32x32 | 16 | 7 |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| support_wood_e (fit sonrası) | %100 | 5.4 | x1.15 | GEÇTİ (ham: UYARI x1.32) |
| mine_lamp_e (`--light`) | %100 | 7.1 | x0.76 | GEÇTİ (ışık pikseli %16 denetim dışı) |
| clamp2 (ham) | %52 | 28.8 | x1.74 | KALDI |
| clamp2 (fit) | %52 | 19.0 | x1.16 | KALDI (palet dışı renkler) |
| clamp4 (fit) | %75 | 11.7 | x0.74 | KALDI |
| clamp3 (fit) | %56 | 15.2 | x0.82 | KALDI |

`validate_iso.py --kind object`: çatkı geçti (alt boşluk 7), kandil geçti (alt boşluk 7). Işık sol-üstten, gölge pişirilmemiş.

## Kömür yığını (teslim edilmedi)
Dört aday da şekil olarak kubbe yığını (davul sorunu çözüldü), ama hepsi parlak yeşil çim ve sivri/koni yapıyla aileden uzak; fit sonrası palet uyumu %52-75, KALDI. Aday 2 (alçak kubbe, duman, kütük uçları) şekilde en yakın ama zemin yaması ve neon yeşil var. Aday 4 zemin yamasız ama koni. Adaylar `395d-red/clamp*.png`.

## Sorular
- Kömür yığını için öneri: aday 2'nin yeşilini aileye indirgemek için elle renk eşleme (workbench) ya da tarifte "muted olive green turf, dark brown earth, no ground tile" ekleyip yeni tur; ya da kullanıcı aday 2/4'ü olduğu gibi görsel olarak değerlendirsin.
- Kandil çanağı koyu kahve içi ve alev büyük; kullanıcı ölçeği onaylamalı.
