unity-import: pending

# 474b Cam bahçe canlıları: yeniden deneme

- Araç: `create_image_pixen`, `no_background=true`, `view=side`, `direction=east` (sağa bakar), 24x24 (kelebek) ve 16x16 (diğerleri). Pixen küçük sprite için temiz (pixflux 32 altını reddeder).
- Çağrı: 8 başarılı x 1 gen = **8 gen**. Hız sınırı ("10/10 jobs", diğer şeritler) yüzünden 5 çağrı reddedildi (üretim yapılmadı, ücret yok); yönergedeki istisna gereği 1 kez yeniden gönderildi.
- `get_balance` sonrası: 2977,9 (paylaşılan bakiye; diğer şeritlerin harcamasını da içerir).
- Doygunluk: `hayvan` ailesinin kilidi kurulmadı. Ham hâlde 5 görsel yalnızca "ailesinden çok doygun" yüzünden KALDI (şekilleri tanınıyor), `style.py fit` ile indirildi: kelebek x0,67, ateş böceği x0,65, uğur böceği x0,48, deniz salyangozu x0,53, örümcek x0,59. Ham hâlleri `art/results/474b-red/*_ham.png`. Teslim edilenler düzeltilmiş hâller.
- Yan yana: `art/results/474b-yanyana.png`
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`)
| Dosya | Piksel | Pivot x | Pivot y (alttan) | Not |
|---|---|---|---|---|
| viv_butterfly.png | 24x24 | 12 | 3 | fit x0,67 |
| viv_caterpillar.png | 16x16 | 8 | 2 | ham |
| viv_firefly.png | 16x16 | 8 | 3 | fit x0,65 |
| viv_ladybird.png | 16x16 | 8 | 3 | fit x0,48 |
| viv_worm.png | 16x16 | 8 | 3 | ham |
| viv_spider.png | 16x16 | 8 | 1 | fit x0,59; ipte sarkan, alt boşluk 1 |
| viv_ground_beetle.png | 16x16 | 8 | 3 | ham |
| viv_periwinkle.png | 16x16 | 8 | 3 | fit x0,53 |

## Doğrulama
`validate_iso.py`: 8/8 geçti. Stil denetimi (`hayvan`, kilitsiz):

| Dosya | Palete yakın | Doygunluk | Sonuç |
|---|---|---|---|
| butterfly | %70 | x1,17 | UYARI (palet) |
| caterpillar | %56 | x1,32 | UYARI (palet, doygun) |
| firefly | %99 | x1,33 | UYARI (doygun) |
| ladybird | %100 | x1,02 | GEÇTİ |
| worm | %51 | x1,32 | UYARI (palet, doygun) |
| spider | %100 | x1,43 | UYARI (doygun) |
| ground_beetle | %76 | x0,94 | UYARI (palet) |
| periwinkle | %63 | x1,13 | UYARI (palet) |

Zemin karosu değil, döşeme testi uygulanmadı. Gölge pişirilmemiş. Hepsi Avrupa/Akdeniz türü.

## Sorunlar
- Ateş böceği küçük ve ayrıntısı zayıf, sarı kuyruk belirgin değil; kullanıcı gözüyle bakmalı.
- Alt kenar boşluğu 1-3 satır; pivot yukarıdaki tabloda, kod gerekirse taban kırpar.
