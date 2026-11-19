unity-import: pending

# 421 Tüccar malları ve risaleler (durum: review, kısmen teslim: 4/7)

Yan yana sayfası: [421-yanyana.png](421-yanyana.png) (teslim edilen 4 dosya). Teslim edilmeyenler: `art/results/421-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 7 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (401/417 yöntemi), tarif açıklamaları + "simple still-life object..." eki. Hepsi paralel, hız sınırı yok. **7 gen** (cost_est 7, cost_max 14).
- Risaleler tarifteki "yuva yöntemi" (kılavuz + maske) yerine basic modla üretildi (kılavuz betiği bulutta yok); pivot yaklaşık taban merkezi.
- Doygunluk fazlası olan üçü için `style.py fit` denendi (smithing x0,73, indigo x0,82, saffron x0,71 çarpanı, almanac x1,00); yalnızca smithing UYARI düzeyine indi, ham ve fit hâlleri `421-red/`de.
- `get_balance` sonrası: **3778,5**.

## Sonuç özeti
| # | Öğe | Durum | Not |
|---|---|---|---|
| 1 | indigo | TESLİM EDİLMEDİ | koyu mavi yuvarlak kalıp; mavi aile paletinde yok (%21 palete yakın), fit sonrası da KALDI; toz belirgin değil |
| 2 | saffron | TESLİM EDİLMEDİ | kil tabakta kırmızı iplikler tanınıyor, ama palet dışı/aşırı doygun; fit sonrası da KALDI (%79) |
| 3 | black_pepper | teslim | kese içinde karabiber yığını, GEÇTİ |
| 4 | book_almanac | TESLİM EDİLMEDİ | şekil tanınıyor (koyu mavi kitap, pirinç köşe) ama mavi palet dışı (%16); fit çözmedi |
| 5 | book_herbal | teslim (UYARI) | yeşil kapak, yaprak kabartması; palete uzak, soluk (x0,63) |
| 6 | book_husbandry | teslim | kahverengi kapak, GEÇTİ |
| 7 | book_smithing | teslim (UYARI) | koyu kırmızı kapak; doygunluk fit ile x1,16'ya indirildi (ham: x1,58 KALDI) |

## Dosyalar (pivot x, pivot y ALTTAN)
| Yol (`Assets/Art/Iso/_aday/`) | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| icon_item_black_pepper.png | 32x32 | 16 | 0 (ikon) |
| iso_item_book_herbal_se.png | 32x32 | 16 | 5 |
| iso_item_book_husbandry_se.png | 32x32 | 16 | 5 |
| iso_item_book_smithing_se.png | 32x32 | 16 | 5 |

## Doğrulama (validate_iso --kind object --max-colors 48; ham hâller)
| Dosya | Opak sınır | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| almanac | x2-29, y5-27 | 17 | 0 | geçti |
| herbal | x2-29, y2-30 | 17 | 0 | geçti (alt boşluk 1) |
| husbandry | x2-28, y4-27 | 29 | 0 | geçti |
| smithing | x2-29, y4-27 | 19 | 0 | geçti |
| indigo | x7-25, y7-24 | 7 | 0 | geçti |
| saffron | x5-25, y6-26 | 27 | 0 | geçti |
| black_pepper | x5-26, y4-27 | 18 | 0 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| almanac (fit) | %16 | 21.7 | x1.03 | KALDI |
| herbal | %82 | 15.7 | x0.63 | UYARI |
| husbandry | %93 | 7.0 | x1.13 | GEÇTİ |
| smithing (fit) | %93 | 14.2 | x1.16 | UYARI |
| indigo (fit) | %21 | 28.9 | x1.16 | KALDI |
| saffron (fit) | %79 | 13.0 | x1.15 | KALDI |
| black_pepper | %100 | 9.9 | x1.07 | GEÇTİ |

## Sorunlar / öneri
- Çivit ve almanak için mavi, esya paletinde yok; ayrı bir 421b işi (renk tabanlı yeniden üretim ya da palete uyarlama) gerekir. Gözle iki görsel de tanınıyordu; kullanıcı `421-red/`e bakıp paleti genişletmeyi seçebilir.
- Safran fit ile bile palete tam oturmadı.
- Tek deneme kuralı gereği yeniden üretilmedi.
