unity-import: pending

# 021b Portre yüz tabanları (long, heart): sonuç

Durum: kısmi. 5/8 dosya teslim edildi; 3/8 teslim edilmedi (2 stil KALDI, 1 sakallı).
Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`create_image_pixflux`, 64x64, `no_background`, `direction=south`, medium detail, basic shading, palet = `iso_char_farmer_a2_56_cand_se.png`, 1 gen/çağrı; tarifteki açıklama + 020 ortak eki. Yeniden denemede (long adult, long elder) "smooth bare chin and bare upper lip, NO stubble NO goatee" eklendi, seed 11.
İlk çağrılardan biri eşzamanlı iş sınırına (8/8, diğer şeritler) takıldı; faturalanmadı, bekleyip yeniden gönderildi.

## Harcama
10 gen (8 taban + long adult ve long elder için 1'er yeniden deneme). `get_balance` sonrası: 1722,2 (paralel şeritler dahil). Defter: `art/ledger/021b.md`.

## Dosyalar (portre, pivot yok; 64x64)
| Yol | Boyut | Renk | Sonuç |
|---|---|---|---|
| Assets/Art/Iso/_aday/portrait_base_long_youth_64.png | 64x64 | 18 | UYARI (doygun x1.48), teslim |
| Assets/Art/Iso/_aday/portrait_base_long_adult_64.png | 64x64 | 14 | GEÇTİ (2. deneme; 1. deneme sakallı), teslim |
| Assets/Art/Iso/_aday/portrait_base_heart_child_64.png | 64x64 | 12 | GEÇTİ, teslim |
| Assets/Art/Iso/_aday/portrait_base_heart_youth_64.png | 64x64 | 22 | GEÇTİ, teslim |
| Assets/Art/Iso/_aday/portrait_base_heart_elder_64.png | 64x64 | 21 | UYARI (doygun x1.49), teslim |

Teslim edilmeyenler (`art/results/021b-red/`): long_child (KALDI, doygun x1.68), heart_adult (KALDI, doygun x1.71), long_elder (iki denemede de gri sakallı; ayrıca UYARI). Üçü de yeniden üretilmeli.

## Stil denetimi (`style.py check --family karakter`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| heart_adult | %99 | 11.3 | x1.71 | KALDI |
| heart_child | %99 | 8.1 | x1.18 | GEÇTİ |
| heart_elder | %99 | 9.8 | x1.49 | UYARI |
| heart_youth | %100 | 9.8 | x1.20 | GEÇTİ |
| long_adult | %99 | 8.4 | x1.19 | GEÇTİ |
| long_child | %97 | 11.3 | x1.68 | KALDI |
| long_elder | %93 | 10.2 | x1.37 | UYARI |
| long_youth | %100 | 9.0 | x1.48 | UYARI |

Yan yana (teslim edilen 5 + 2 referans): `art/results/021b-yanyana.png`.

## Doğrulama
- Tüm dosyalar 64x64, yarı saydam piksel 0, renk sayısı ≤ 22 (≤32).
- Maske yok (yalnızca taban), maske dışı piksel denetimi uygulanmaz.
- Tabana birleşim önizlemesi yapılmadı: bu işte katman yok.
- Sakalsızlık (gözle): long_adult 2. denemede temiz; long_elder iki denemede de sakallı; teslim edilen diğerleri temiz.

## Sorunlar
1. Gri ten isteği doygunluğu her zaman düşürmüyor: long child ve heart adult palete uzak/çok doygun (2 KALDI).
2. Long elder sakalı iki denemede kalkmadı; öneri: yeni iş (başka tarif ya da sakalı inpaint ile sildirme).
3. Omuz kıyafeti çoğunda koyu kahve yelek; "linen collar" beklentisinden farklı, katman akışında engel değil.
