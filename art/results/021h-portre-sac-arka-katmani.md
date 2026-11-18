unity-import: pending

# 021h Portre saç arka katmanı: sonuç

Durum: tamam. 10/10 dosya teslim edildi (KALDI yok, 2 UYARI). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, 5 gen/çağrı, seed 0, tek deneme. Taban `portrait_base_oval_adult_64.png` (`image_url=upload:`). Açıklama: `<ad> hairstyle seen from behind the head, neutral grey, back layer only` + `neutral grey, will be recoloured` + ortak ek. Maskeler betikle, saf siyah/beyaz; hepsinden kafa elipsi (21,7)-(42,45) çıkarıldı:
- kısa (crop, messy, side_part, bowl, buzz): (8,0)-(55,30)
- uzun (medium_*, shoulder_curly, long_*): aynı + yan şeritler (8,31)-(21,62) ve (42,31)-(55,62)

Hız sınırı: 8 eşzamanlı iş sınırına (diğer şeritler dahil) takıldım; reddedilen çağrılar faturalanmadı, iş bitince bir kez yeniden gönderildi.

## Harcama
10 x 5 = 50 gen (cost_max 50). `get_balance` öncesi 1533,7; sonrası ayrıca okunmadı (defter: `art/ledger/021h.md`).

## Dosyalar (portre, pivot yok; 64x64, yarı saydam piksel 0, maske dışı piksel 0)
| Yol (Assets/Art/Iso/_aday/portrait_layer_hair_*_back_64.png) | Renk | Stil |
|---|---|---|
| short_crop | 15 | GEÇTİ x0.70 |
| short_messy | 20 | UYARI (soluk x0.51) |
| short_side_part | 18 | GEÇTİ x0.73 |
| bowl | 22 | UYARI (palete uzak, soluk x0.29) |
| buzz | 16 | GEÇTİ x0.67 |
| medium_straight | 27 | GEÇTİ x0.89 |
| medium_wavy | 27 | GEÇTİ x0.72 |
| shoulder_curly | 21 | GEÇTİ x0.62 |
| long_straight | 28 | GEÇTİ x0.77 |
| long_wavy | 26 | GEÇTİ x0.60 |

## Doğrulama
- Hepsi 64x64, yarı saydam 0, renk ≤ 28 (reduce_colors gerekmedi), maske dışı 0.
- Yan yana: `art/results/021h-yanyana.png`. Taban birleşimi x3: `art/results/021h-birlesim-x3.png` (sıra: crop, messy, side_part, bowl, buzz / medium_straight, medium_wavy, shoulder_curly, long_straight, long_wavy; arka katman tabanın altında).
- Birleşimde boşluk, kayma, çift dış hat yok.

## Sorunlar
1. Çıktılarda gri olmayan turuncu/kahve pikseller var (kulak ve omuz/giysi kalıntısı; maske tabanın kulak ve giysi alanını da kapsadığı için üretildi). Çoğu taban altında kalıyor; maske kulak/omuz tarafında daraltılmalı.
2. short_crop, bowl, buzz çok ince (başın çevresinde yalnızca kenar şeridi); ön katmanla birlikte anlam kazanıyor.
3. long_straight iki ince şerit; medium_straight omuza kadar inmiyor. Yalnızca oval_adult tabanında üretildi.
