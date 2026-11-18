unity-import: pending

# 313 Mezar taşı + festival süsleri (rapor)

Yan yana sayfası: [313-yanyana.png](313-yanyana.png) (sırayla: flama, şenlik direği, şenlik ateşi*, fener*, mezar 2. deneme*; *teslim edilmedi). Teslim edilmeyenler: `art/results/313-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`; 64 px genişlikli mezar ve fener için araç sol 8 px boşluk koyunca ayak izi tuvalden taştığından aynı elmas sol boşluksuz üretildi, pivot x=32), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek. Kılavuz referanssız. Araçta `seed` yok; ikinci deneme aynı kılavuz ve maskeyle.
Çağrılar (her biri 1 gen): 5 ilk deneme + 1 ikinci deneme (mezar) = 6 generation (cost_max 14). İlk çağrıda büyük base64 kısaltıldığı için reddedildi (ücret yok); kılavuz 2 renkli 1 bit PNG ile yeniden kodlandı.
get_balance: iş sonu 859.10 (paralel şeritler de harcıyor).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | mezar | TESLİM EDİLMEDİ | 1. deneme 53 renk (>48) ve çimenli zemin yaması; 2. deneme 38 renk, doğrulama geçti ama stil KALDI (palete yakın %80); yazı yok, tabanda çim parçası var |
| 2 | flama | teslim (GEÇTİ) | |
| 3 | şenlik direği | teslim (UYARI) | ailesinden soluk (x0.48); ince direk ayak izini doldurmaz (beklenen) |
| 4 | şenlik ateşi | TESLİM EDİLMEDİ | stil KALDI (alev renkleri palet dışı, doygunluk x1.41); görsel tarife uygun, alev serbest |
| 5 | kış feneri | TESLİM EDİLMEDİ | stil KALDI (palete yakın %85, doygunluk x1.96; sıcak mum ışığı); görsel tarife uygun |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_deco_bunting_2x1_se_nw.png | 112x96 | 56 | 32 |
| iso_deco_maypole_se.png | 80x120 | 40 | 24 |

Teslim edilmeyenlerin pivotu (kabul edilirse): mezar 32/24, ateş 40/24, fener 32/24.

## Doğrulama (validate_iso, --max-colors 48)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| flama | x25-85, y9-81 | 7 | 0 | yok | geçti |
| şenlik direği | x26-53, y12-107 | 23 | 0 | yok | geçti |
| şenlik ateşi | x10-69, y4-59 | 34 | 0 | yok | geçti (stil KALDI) |
| fener | x16-51, y6-72 | 37 | 0 | yok | geçti (stil KALDI) |
| mezar 1. | x2-61, y8-56 | 53 | 0 | yok | BAŞARISIZ (renk) |
| mezar 2. | x6-57, y6-56 | 38 | 0 | yok | geçti (stil KALDI) |

## Stil denetimi (style.py check --family esya)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| flama | %92 | 9.1 | x1.16 | GEÇTİ |
| şenlik direği | %92 | 9.0 | x0.48 | UYARI |
| şenlik ateşi | %83 | 21.6 | x1.41 | KALDI |
| fener | %85 | 11.4 | x1.96 | KALDI |
| mezar 1. | %95 | 13.5 | x0.63 | UYARI |
| mezar 2. | %80 | 11.9 | x0.76 | KALDI |

## Sorunlar
- Ateş ve fener sıcak alev/ışık renkleri yüzünden stil kapısını geçemedi; tarif alevi serbest bırakıyor. Kullanıcı yan yana sayfasına bakıp elle teslim isteyebilir (dosyalar `313-red/` içinde).
- Mezar iki denemede de zemin/çim yaması içeriyor; tarif tutmadı.
- Işık, ayna ve ayrıntı ölçümleri yapılmadı (zemin değil; ölçüm betiği yok), kullanıcı görsele bakmalı.
