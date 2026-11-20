unity-import: pending

# 612b Köy meydanı yıkık hâller ve ocak (rapor)

Sonuç: **KISMİ**. 4 yıkık hâl teslim edildi (çark, çan kulesi, pazar yeri, köprü). **Ocak, kuyu yıkığı ve kiler yıkığı teslim edilmedi.** İş `review` (görsel karar kullanıcıda).
Yan yana sayfası: [612b-yanyana.png](612b-yanyana.png) (yalnızca yeni 4 görsel + 3 referans; onarılmış hâlle çift sayfa üretilmedi). Teslim edilmeyenler: `art/results/612b-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_image_pixflux`, isometric, high top-down, south-east, single color outline, basic shading, low detail, `no_background`. Onarılmış PNG'ler `upload:` ile `init_image_url` verildi. (`footprint_guide.py` kılavuzu 128 tuvale sığmadığından ocak için elle 128x64 elmas kılavuzu çizildi.) 1. tur güç 70 (6 yıkık) ve ocak için 120; 2. tur güç 30 (kuyu, kiler, çark, çan kulesi) ve ocak kılavuzsuz. Ocak için ek bir deneme (güç 200) boş döndü, ücretlendirilmedi. Edit/inpaint denenmedi: edit_image 10 gen (7 görsel için 70 > cost_max 32).
Toplam ≈ 13 generation (cost_est 16, cost_max 32). `get_balance` sonrası: 1005,60.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_square_millwheel_ruin_se.png (fit x0.71, 2. tur) | 128x160 | 64 | 21 |
| iso_square_belltower_ruin_se.png (2. tur) | 128x256 | 64 | 9 |
| iso_square_market_hall_ruin_se.png | 192x192 | 96 | 22 |
| iso_square_bridge_ruin_se.png | 192x128 | 96 | 21 |

Pivotlar tahmindir (opak alt kenar boşluğu, yatay orta).

## Doğrulama (`validate_iso --kind object --max-colors 48`) ve stil (`style.py check --family yapi`)
| Dosya | Renk | Alta boşluk | validate | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|---|---|
| çark yıkık | 13 | 21 | geçti | %100 | x1.15 (fit) | GEÇTİ (ham x1.62 KALDI) |
| çan kulesi yıkık | 18 | 9 | geçti | %100 | x0.78 | GEÇTİ |
| pazar yeri yıkık | 42 | 22 | geçti | %100 | x0.97 | GEÇTİ |
| köprü yıkık | 44 | 21 | geçti | %100 | x0.70 | GEÇTİ (60 renk sorunu çözüldü: 44) |

## Sorunlar
- **Taban elması aynı piksellerde değil.** Pixflux yeniden üretimi; çan kulesi ve çark farklı tabanla/ölçekle geldi (çan kulesi: çatısız, mazgallı taş kule, yeni taban; çark: eğik, kendi tabanı yok, kırık tahtalar). Köprü ve pazar yeri onarılmış hâle yakın kompozisyonda. Kullanıcı yan yana bakmalı; çan kulesi onarılmış hâlden farklı bir yapı gibi okunabilir.
- **Ocak teslim edilmedi:** 1. deneme neredeyse boş çıktı (küçük bir kalıntı), güç 200 boş döndü, kılavuzsuz 2. deneme turuncu kor/ateş içeriyor (sönük ölçütüne aykırı) ve elmasın tamamını doldurmuyor (etrafta tabureler). Ocak yıkığı da yapılamadı.
- **Kuyu yıkığı:** güç 70'te onarılmışla neredeyse aynı, güç 30'da direkler duruyor, set bozulmamış; açıkça yıkık okunmuyor. **Kiler yıkığı:** güç 70'te aynı, güç 30'da kemer ayakta; teslim edilmedi.
- Döşeme testi gerekmedi (zemin değil). Işık/ayak izi sayıyla ölçülmedi.
- `max_attempts: 2` tükendi. Öneri (yeni iş): kuyu/kiler/ocak için `edit_image` (10 gen/çağrı) ya da `inpaint_image_pro_flash` (5 gen) ile maskeli düzenleme.
