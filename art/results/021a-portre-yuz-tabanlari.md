unity-import: pending

# 021a Portre yüz tabanları: sonuç

Durum: kısmi. 11/12 dosya teslim edildi; oval_elder stil denetiminde KALDI (teslim edilmedi). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`create_image_pixflux`, 64x64, no_background, south, medium detail, basic shading, palet = `iso_char_farmer_a2_56_cand_se.png`, 1 gen/çağrı. Açıklama tarifteki gibi (+ 020 ortak eki); square için "strong jaw" eklendi.
Yeniden denemeler: sakallı çıkan round_adult, oval_elder, round_elder, square_elder ve yarı yüzü mavi bozuk çıkan square_adult için (ikinci denemede "smooth bare chin, shaved face, absolutely no facial hair" ile); stil KALDI olan oval_child ve square_youth için seed=7 ile.

## Harcama
Toplam 19 gen (cost_max 36): 12 ilk + 7 yeniden deneme. `get_balance` sonrası: 1719,2 (diğer şeritlerin harcaması dahil). Defter: `art/ledger/021a.md`.

## Dosyalar (portre, pivot yok; 64x64, yarı saydam piksel 0)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil | Not |
|---|---|---|---|
| portrait_base_oval_child_64.png | 18 | UYARI x1.26 | 2. denemede |
| portrait_base_oval_youth_64.png | 21 | UYARI x1.41 | |
| portrait_base_oval_adult_64.png | 20 | UYARI x1.40 | sakalsız |
| portrait_base_round_child_64.png | 16 | UYARI x1.30 | |
| portrait_base_round_youth_64.png | 22 | UYARI x1.42 | |
| portrait_base_round_adult_64.png | 16 | GEÇTİ x1.23 | 2. denemede sakalsız |
| portrait_base_round_elder_64.png | 24 | UYARI x1.34 | **SAKALLI (gri sakal)** |
| portrait_base_square_child_64.png | 19 | GEÇTİ x1.18 | |
| portrait_base_square_youth_64.png | 17 | GEÇTİ x1.04 | 2. denemede |
| portrait_base_square_adult_64.png | 19 | UYARI x1.35 | 2. denemede sakalsız |
| portrait_base_square_elder_64.png | 23 | UYARI x1.31 | **SAKALLI (gri sakal)** |

Teslim edilmeyen: `art/results/021a-red/portrait_base_oval_elder_64_red.png` (sakalsız ama KALDI: palete %92, doygunluk x1.62; 2 deneme tükendi).

## Doğrulama
- Hepsi 64x64, yarı saydam 0, renk ≤ 24 (≤ 32). `validate_iso.py` çıktı vermedi (yalnızca boyut/alfa anlamlı).
- Yan yana: `art/results/021a-yanyana.png`; x4 önizleme: `art/results/021a-onizleme-x4.png`.
- Maske kullanılmadı (üretim doğrudan), maske dışı piksel denetimi yok.
- Tabana birleşim önizlemesi: bu işte katman yok, yapılmadı.

## Sorunlar
1. **Yaşlı tabanlarda sakal:** round_elder ve square_elder iki denemede de gri sakallı çıktı. Sakal katmanıyla çakışır. Öneri: inpaint ile sakalı sildir ya da yeniden üret.
2. **oval_elder** teslim edilmedi (stil KALDI); yeniden üretim ya da inpaint gerekir.
3. Çoğu taban ailenin ortalamasından 1.2-1.4x doygun (UYARI); 020 sorun 2 ile aynı.
4. Yaka/kıyafet tabanlar arasında tutarsız (yelek, gömlek, kürklü yaka); katman birleşiminde yaka alanı maskelenmeli.
