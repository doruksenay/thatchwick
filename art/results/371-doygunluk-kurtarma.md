unity-import: pending

# 371 Reddedilen adayları doygunluk eşlemesiyle kurtarma

- Araç: `tools/art/style.py fit --family <aile>` (yerel, ücretsiz). PixelLab çağrılmadı: 0 generation. `get_balance` sonrası: 4999 (Tier 2, 5000/döngü; bütçe belgesi hâlâ Tier 1 yazıyor, kullanıcı planı yükseltmiş görünüyor).
- 32 aday işlendi, **31 teslim**, 1 teslim edilmedi (şenlik ateşi).
- Yan yana sayfaları: `art/results/371-yanyana.png` (bitki: 4 referans + 29 yeni), `art/results/371-yanyana-esya.png` (eşya: 7 referans + 3 yeni; ateş fit'li hâliyle, teslim edilmedi).
- Pivot: bitki satırları (40, 24) alttan, mezar ve fener (32, 24). Dosya boyutları aşağıda. Zemin parçası olmadığı için döşeme testi yapılmadı (hepsi nesne). `validate_iso.py --kind object`: 32 dosyanın hepsi geçti.
- 48 renk üstü olanlar en sık 48 renge en yakın komşu nicemlemeyle indirildi (titreme yok), sonra yeniden denetlendi.
- Kaynak eşlemesi: 320-red (huş genç, çam olgun, mantar yatağı), 319-red (elma/armut/kiraz x3, erik meyveli), 311-red (kuru dal, kovan, mantar), 322-red (yabani sarımsak, tere), 310-red (lahana 1-3, pırasa 2, bezelye 1 ve 3, balkabağı 1-3, şalgam 1 ve 3), 313-red (mezar 1, fener, ateş).

## Doğrulama tablosu

Doygunluk sütunu: fit öncesi → sonrası (aile ortalamasına oran). Bitki ailesinin kilidi kurulmadığı için palet uyarıları yalnızca uyarıdır.

| Çıktı | Boyut | Doygunluk (önce → sonra) | Palete yakın | Renk | Kapı | Teslim |
|---|---|---|---|---|---|---|
| iso_tree_birch_young_se | 96x112 | x0.99 → x1.16 | %36 | 31 | UYARI | evet |
| iso_tree_pine_grown_se | 96x112 | x0.89 → x1.15 | %68 | 17 | UYARI | evet |
| iso_tree_apple_young_se | 96x112 | x0.52 → x1.15 | %45 | 49→48 | UYARI | evet |
| iso_tree_apple_grown_se | 96x112 | x0.42 → x1.15 | %21 | 23 | UYARI | evet |
| iso_tree_apple_fruit_se | 96x112 | x0.51 → x1.16 | %40 | 33 | UYARI | evet |
| iso_tree_pear_young_se | 96x112 | x0.61 → x1.15 | %30 | 51→48 | UYARI | evet |
| iso_tree_pear_grown_se | 96x112 | x0.57 → x1.15 | %61 | 37 | UYARI | evet |
| iso_tree_pear_fruit_se | 96x112 | x0.39 → x1.15 | %55 | 35 | UYARI | evet |
| iso_tree_cherry_young_se | 96x112 | x0.65 → x1.14 | %67 | 18 | UYARI | evet |
| iso_tree_cherry_grown_se | 96x112 | x0.51 → x1.15 | %65 | 27 | UYARI | evet |
| iso_tree_cherry_fruit_se | 96x112 | x0.41 → x1.16 | %58 | 56→48 | UYARI | evet |
| iso_tree_plum_fruit_se | 96x112 | x1.00 → x0.72 | %77 | 29 | UYARI | evet |
| iso_item_mushroom_bed_se | 64x40 | x0.66 → x1.18 | %99 | 23 | GEÇTİ | evet |
| iso_forage_dry_branch_se | 80x72 | x0.42 → x1.24 | %98 | 47 | GEÇTİ | evet |
| iso_forage_hive_se | 80x72 | x0.62 → x1.15 | %100 | 51→48 | GEÇTİ | evet |
| iso_forage_mushroom_se | 80x72 | x0.58 → x1.14 | %99 | 55→48 | UYARI (palete uzak) | evet |
| iso_forage_wild_garlic_se | 80x72 | x0.40 → x1.15 | %38 | 59→48 | UYARI | evet |
| iso_forage_cress_se | 80x72 | x0.43 → x1.16 | %20 | 12 | UYARI | evet |
| iso_crop_cabbage_1_se | 80x72 | x1.00 → x1.14 | %51 | 19 | UYARI | evet |
| iso_crop_cabbage_2_se | 80x72 | x0.47 → x1.14 | %23 | 33 | UYARI | evet |
| iso_crop_cabbage_3_se | 80x72 | x0.48 → x1.15 | %89 | 59→48 | UYARI (palete uzak) | evet |
| iso_crop_leek_2_se | 80x72 | x0.87 → x1.14 | %76 | 37 | UYARI | evet |
| iso_crop_pea_1_se | 80x72 | x0.96 → x1.17 | %59 | 37 | UYARI | evet |
| iso_crop_pea_3_se | 80x80 | x0.54 → x1.16 | %60 | 59→48 | UYARI | evet |
| iso_crop_pumpkin_1_se | 80x72 | x0.48 → x1.12 | %100 | 58→48 | UYARI (palete uzak) | evet |
| iso_crop_pumpkin_2_se | 80x72 | x0.66 → x1.15 | %66 | 41 | UYARI | evet |
| iso_crop_pumpkin_3_se | 80x72 | x0.58 → x1.21 | %65 | 47 | UYARI | evet |
| iso_crop_turnip_1_se | 80x72 | x0.69 → x1.15 | %60 | 37 | UYARI | evet |
| iso_crop_turnip_3_se | 80x72 | x0.73 → x1.17 | %57 | 46 | UYARI | evet |
| iso_item_grave_se (aile esya) | 64x64 | x1.00 → x0.63 | %95 | 53→48 | UYARI (palete uzak) | evet |
| iso_deco_lantern_se (aile esya) | 64x88 | x0.59 → x1.16 | %99 | 37 | GEÇTİ | evet |
| iso_deco_bonfire_se (aile esya) | 80x72 | ham x1.41, fit sonrası x1.16 | ham %83, fit %83 | 34 | **KALDI** (ham de fit'li de: palet dışı renkler) | **hayır** |

Not: "Doygunluk önce" değerleri `fit` çıktısındaki dönüşüm katsayısıdır (kaynak ÷ hedef ters oranı), tablodaki "sonra" ise `check` sonucudur. Erik meyveli ve mezar hedefin altına (x0.72, x0.63) indi: kaynakları zaten aile ortalamasına yakındı, `fit` bunları aşağı çekmiş. Kullanıcı sayfada görsel olarak karar versin.

## Teslim edilmeyen

- **iso_deco_bonfire_se:** ham hâli x1.41 doygun ve palet dışı, KALDI. Fit sonrası alev rengi korunup doygunluk x1.16'ya inse de palet uzaklığı (21,6 → 16,7) hâlâ KALDI. Kural gereği teslim edilmedi; fit'li hâli `art/results/371-red/iso_deco_bonfire_se_fit.png` altında, kullanıcı yan yana sayfasına (`371-yanyana-esya.png`) bakıp karar versin. Gerekirse yeniden üretim işi yazılmalı.

## Sorunlar ve öneriler

- Zemin karosu ya da opak çim elması kontrolü yalnızca denetleyici ve doğrulayıcı üzerinden yapıldı; yan yana sayfasında gözle bakılması iyi olur.
- Çimen karosu üstünde 4x4 sayfa yapılmadı (yalnızca yan yana sayfası üretildi).
- Bitki ailesinin kilidi hâlâ kurulmamış (kabul referans < 3); palet uyarıları bu yüzden.
- Teslim edilenler `Assets/Art/Iso/_aday/` altında, `.meta` yazılmadı; var olan hiçbir dosya ezilmedi (`cp -n` eşdeğeri).
- Masaüstü kopyası: yerel.
