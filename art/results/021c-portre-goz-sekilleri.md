unity-import: pending

# 021c Portre göz şekilleri: sonuç

Durum: 8/8 teslim. Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, taban `portrait_base_oval_adult_64.png` (upload ile yüklendi), `Modify current layer, only changes`, dikdörtgen maske x18 y22 w28 h9 (kaş altından yanak üstüne), 5 gen/çağrı, seed 0, 8 çağrı paralel. Açıklama tarifteki gibi. Not: ak ve iris için ayrı maske kullanılmadı (tek maske, tek çağrı sınırı); iris `#808080` sabiti zorlanmadı.

## Harcama
8 x 5 = 40 gen (cost_max 40). `get_balance` sonrası: 1679,2.

## Dosyalar (64x64, pivot yok: portre)
| Yol (Assets/Art/Iso/_aday/) | Stil |
|---|---|
| portrait_layer_eyes_round_64.png | UYARI x1.44 |
| portrait_layer_eyes_almond_64.png | UYARI x1.41 |
| portrait_layer_eyes_narrow_64.png | UYARI x1.41 |
| portrait_layer_eyes_wide_64.png | UYARI x1.49 |
| portrait_layer_eyes_droopy_64.png | UYARI x1.42 |
| portrait_layer_eyes_hooded_64.png | UYARI x1.42 |
| portrait_layer_eyes_upturned_64.png | UYARI x1.46 |
| portrait_layer_eyes_small_64.png | UYARI x1.45 |

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk sayısı 13-22 (≤32). Maske dışı piksel 0 (betikle doğrulandı).
- Stil: hepsi UYARI (palete uzak ort. 12.2-13.1, ailesinden doygun), KALDI yok. Yan yana: `art/results/021c-yanyana.png`.
- Birleşim önizlemesi x4: `art/results/021c-onizleme-x4.png`. Kayma ya da çift dış hat yok.

## Sorunlar
1. Katman maske dikdörtgenini neredeyse tamamen opak doldurdu (250/252 piksel, ten dahil): göz bölgesi tabanı yeniden çiziyor.
2. Şekil farkları ince (özellikle wide/small/narrow); yan yana bakıp onay gerekir.
3. Ak/iris ayrı maske yapılmadı; iris pikselleri tam `#808080` olmayabilir (rampa takası için temizlik gerekebilir).
