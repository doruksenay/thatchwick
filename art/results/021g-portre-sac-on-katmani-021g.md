unity-import: pending

# 021g Portre saç ön katmanı: sonuç

Durum: tamam. 10/10 dosya teslim edildi (KALDI yok; 6 UYARI). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, 5 gen/çağrı. Taban `portrait_base_oval_adult_64.png` (`get_upload_url` ile yüklendi, `image_url=upload:...`). Açıklama: `<ad> hairstyle, neutral grey hair, front layer` + `neutral grey, will be recoloured` + ortak ek. Seed 0, tek deneme. Maskeler betikle, saf siyah/beyaz:
- kısa biçimler (bun_high, topknot, receding, bald_fringe, coif_hair): (12,0)-(51,28)
- uzun biçimler (long_curly, braid_single, braids_two, bun_low, ponytail): aynı üst blok + yan şeritler (8,29)-(22,58) ve (41,29)-(55,58)

Hız sınırı: 8 eşzamanlı iş sınırı; son 2 çağrı ilk işler bitince gönderildi (hata yok).
Renk: braid_single (35), braids_two (35), topknot (36), receding (37) için `reduce_colors` 32 (4 çağrı x 0,1).

## Harcama
10 x 5 + 4 x 0,1 = **50,4 gen** (cost_max 50; renk indirgeme kesirleri tavanı 0,4 aşıyor, sapma kuralının çok altında). `get_balance` sonrası: 1533,7 (paralel şeritler dahil). Defter: `art/ledger/021g.md`.

## Dosyalar (portre, pivot yok; 64x64, yarı saydam piksel 0)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil |
|---|---|---|
| portrait_layer_hair_long_curly_64.png | 27 | UYARI (soluk x0.50) |
| portrait_layer_hair_braid_single_64.png | 30 | GEÇTİ x0.69 |
| portrait_layer_hair_braids_two_64.png | 30 | GEÇTİ x0.67 |
| portrait_layer_hair_bun_high_64.png | 24 | UYARI (palete uzak) |
| portrait_layer_hair_bun_low_64.png | 26 | GEÇTİ x0.86 |
| portrait_layer_hair_ponytail_64.png | 24 | GEÇTİ x0.64 |
| portrait_layer_hair_topknot_64.png | 30 | UYARI (palete uzak) |
| portrait_layer_hair_receding_64.png | 28 | UYARI (palete uzak, x1.20) |
| portrait_layer_hair_bald_fringe_64.png | 18 | UYARI (palete uzak) |
| portrait_layer_hair_coif_hair_64.png | 25 | UYARI (palete uzak, soluk x0.59) |

Yan yana: `art/results/021g-yanyana.png`. Taban birleşim önizlemesi (x3): `art/results/021g-onizleme-x3.png`.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 30 (≤ 32). `validate_iso.py`: geçti (yalnızca boyut/alfa anlamlı).
- Maske dışı piksel 0 (10/10, üretim çıktısında).
- Birleşim: kayma ve çift dış hat görünmüyor; kısa biçimler alın ve şakakta tabana oturuyor.
- Döşeme testi: portre katmanı, uygulanmaz.

## Sorunlar
1. **Maske fazla geniş seçildi:** satır 28'e kadar uzanan maske göz ve yüz tenini de kapsadı, model gözleri/teni yeniden çizdi. Katmandan yüz bölgesi (x 20-43, y ≥ 22) saydam yapılarak silindi (yalnızca maske; yeniden boyama yok). Sonuç saç + yan/omuz bölgesi. Alın çizgisinin hemen altında kısa biçimlerde (y 22-28) saç çıkıntısı varsa bu kırpmayla kesilmiş olabilir. Sonraki saç işlerinde maske satır ~21'de bitmeli (başın üst %45'i).
2. Saçlar gri tonlu istendiği halde bazıları kahverengi/gri karışık; yeniden renklendirme sırasında kontrol edilmeli. long_curly ailesinden soluk.
3. bun_high/topknot topuzları üst kenara (y 0) dayanıyor; kırpılma yok ama pay yok.
4. "Arka katman" parçaları (021h/i) ve omuz uzunluğu biçimleri için yan şeritlerin saç içinde kaldığı doğrulanmalı.
