unity-import: pending

# 612 Köy meydanı onarımı yapıları (rapor)

Sonuç: **KISMİ**. 8 görsel teslim edildi (6 onarılmış yapı + 2 iskele); **yıkık hâllerin tamamı ve ocak teslim edilmedi**. İş `failed` bırakıldı (tek deneme kuralı); eksikler için 612b yazılmalı.
Yan yana sayfası: [612-yanyana.png](612-yanyana.png) (yalnızca yeni 8 görsel + 3 referans; yıkık/onarılmış çift yok). Teslim edilmeyenler: `art/results/612-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_image_pixflux`, isometric, `high top-down`, south-east, `single color outline`, `basic shading`, `low detail`, `no_background`; tuval tarifteki gibi. Referanssız (stil eşlemesi yok). Yıkık hâller: onarılmış görsel `get_upload_url` ile yüklenip `init_image_url` verildi (güç 200: kuyu, ocak, kiler, çark; 150: çan kulesi, pazar yeri, köprü).
Çağrı: 9 taban + 7 yıkık = 16 generation (cost_est 18, cost_max 36). 429 ("10/10 jobs", paylaşımlı şeritler) birkaç kez geldi; çağrılar bekleyip yeniden denendi (iskeleler iki kez). `get_balance` sonrası: 2062,70 (paralel şeritler de harcıyor).
Büyük iskele (192x256 > 40000 px) düz beyaz arka planla geldi; köşelerden taşma doldurmasıyla şeffaflaştırıldı, içerik yeniden boyanmadı.
Doygunluk: kuyu x0.61 ve değirmen çarkı x0.49 `style.py fit` ile ailesine indirildi (ham hâlleri KALDI, düzeltilmişleri GEÇTİ ve teslim edildi).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_square_well_se.png | 128x128 | 64 | 16 |
| iso_square_cellar_se.png | 128x96 | 64 | 6 |
| iso_square_millwheel_se.png | 128x160 | 64 | 20 |
| iso_square_belltower_se.png | 128x256 | 64 | 7 |
| iso_square_market_hall_se.png | 192x192 | 96 | 15 |
| iso_square_bridge_se.png | 192x128 | 96 | 17 |
| iso_square_scaffold_small_se.png | 128x192 | 64 | 23 |
| iso_square_scaffold_large_se.png | 192x256 | 96 | 8 |

Pivotlar tahmindir (opak alt kenar boşluğu, yatay orta); `footprint_guide.py` kullanılmadığı için ayak izi elması ölçülmedi.

## Doğrulama (`validate_iso --kind object --max-colors 48`)
| Dosya | Renk | Yarı saydam | Alta boşluk | Sonuç |
|---|---|---|---|---|
| kuyu (fit) | 31 | 0 | 16 | geçti |
| kiler | 35 | 0 | 6 | geçti |
| çark (fit) | 11 | 0 | 20 | geçti |
| çan kulesi | 34 | 0 | 7 | geçti |
| pazar yeri | 40 | 0 | 15 | geçti |
| köprü | 20 | 0 | 17 | geçti |
| iskele küçük | 24 | 0 | 23 | geçti |
| iskele büyük | 46 | 0 | 8 | geçti |
| ocak (fit, red) | 53 | 0 | 14 | BAŞARISIZ (renk) |
| köprü yıkık (red) | 60 | 0 | 19 | BAŞARISIZ (renk) |

## Stil denetimi (`style.py check --family yapi`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| kuyu (ham) | %88 | 12.1 | x1.87 | KALDI; fit sonrası GEÇTİ (%94, x1.16) |
| ocak (ham) | %98 | 11.4 | x1.70 | KALDI; fit GEÇTİ ama 53 renk |
| kiler | %91 | 10.7 | x1.31 | UYARI (doygun) |
| çark (ham) | %90 | 12.6 | x2.35 | KALDI; fit GEÇTİ (%100, x1.13) |
| çan kulesi | %100 | 5.5 | x0.64 | GEÇTİ |
| pazar yeri | %100 | 6.5 | x1.07 | GEÇTİ |
| köprü | %100 | 6.3 | x0.78 | GEÇTİ |
| iskele küçük | %100 | 5.9 | x0.93 | GEÇTİ |
| iskele büyük | %100 | 5.2 | x0.65 | GEÇTİ |
| köprü yıkık (red) | %100 | 8.1 | x1.21 | GEÇTİ (renk sayısı yüzünden red) |

## Sorunlar
- **Yıkık hâller tutmadı.** `init_image` ile (güç 150-200) altı yıkık çıktı (kuyu, ocak, kiler, çark, çan kulesi, pazar yeri) onarılmış hâlden neredeyse ayırt edilemiyor. Köprü yıkık hâli plank köprü olarak okunuyor ama 60 renk. Hiçbiri teslim edilmedi. Öneri (612b): güç ~50-80 ile, ya da `edit_image`/`inpaint_image_pro_flash` ile; taban elması aynı kalmalı.
- Ocak: kazanın altında kor/alev benzeri kırmızı pikseller var (tarif "alev çizilmez"); fit hâli 53 renk. Yeniden üretilmeli.
- Kuyu ve kiler tuvalde küçük kalıyor; 2x2 elmasını doldurmuyorlar (kılavuzsuz üretim). Kiler ve değirmen çarkı kendi çim/beton tabanını taşıyor (pişmiş zemin).
- Döşeme testi gerekmedi (zemin değil). Işık yönü ve ayak izi doluluğu sayıyla ölçülmedi; kullanıcı yan yana sayfasına bakmalı.
- Yıkık hâl için ikinci deneme yapılmadı (`max_attempts: 1`).
