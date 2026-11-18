unity-import: pending

# 021r Portre makyaj, kırışıklık, eksik diş, sargı: sonuç

Durum: kısmi. 5/7 dosya teslim edildi; kırışıklık ve eksik diş stil denetiminde KALDI (teslim edilmedi). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, seed 0, tek deneme, 7 çağrı paralel, 5 gen/çağrı. Taban `upload:` ile (get_upload_url + urllib). Makyaj/eksik diş/sargı tabanı `portrait_base_oval_adult_64.png`; kırışıklık tabanı `portrait_base_round_elder_64.png` (oval_elder 021a'da teslim edilmediği için; gri sakallı, maske sakala dokunmuyor). Maskeler betikle, saf siyah/beyaz dikdörtgen (64x64): sürme x20-44 y20-26; allık x21-29 ve x35-43, y27-33; dudak x28-37 y31-36; kına x24-40 y11-18; kırışıklık x21-44 y12-27; eksik diş x28-37 y31-37; sargı x24-42 y10-17. Açıklama tarifteki gibi (`neutral grey, will be recoloured` + ortak ek).

## Harcama
7 x 5 = 35 gen (cost_est 35, cost_max 40). `get_balance` sonrası: 1078,7 (paralel şeritler dahil). Defter: `art/ledger/021r.md`.

## Dosyalar (portre, pivot yok; 64x64)
| Yol (Assets/Art/Iso/_aday/) | Renk | Opak px | Stil |
|---|---|---|---|
| portrait_layer_makeup_eyeliner_64.png | 22 | 173 | UYARI x1.30 |
| portrait_layer_makeup_blush_64.png | 9 | 123 | UYARI x1.41 (palete uzak) |
| portrait_layer_makeup_lip_64.png | 12 | 60 | UYARI x1.37 (palete uzak) |
| portrait_layer_makeup_henna_64.png | 10 | 136 | UYARI x1.43 (palete uzak) |
| portrait_layer_bandage_64.png | 12 | 151 | GEÇTİ x0.73 |

Teslim edilmeyen (`art/results/021r-red/`): wrinkles (%89 palete yakın, x1.56) ve missing_tooth (x1.53): KALDI, palete uzak ve çok doygun. Yeniden üretim gerekir (öneri: sonradan betikle gri tonlama).

## Stil denetimi
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| eyeliner | %100 | 9.5 | x1.30 | UYARI |
| blush | %100 | 15.4 | x1.41 | UYARI |
| lip | %100 | 12.1 | x1.37 | UYARI |
| henna | %100 | 13.6 | x1.43 | UYARI |
| wrinkles | %89 | 12.9 | x1.56 | KALDI |
| missing_tooth | %100 | 11.9 | x1.53 | KALDI |
| bandage | %100 | 6.2 | x0.73 | GEÇTİ |

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk 9-31 (≤ 32; `reduce_colors` gerekmedi). Maske dışı opak piksel 0 (7/7).
- Birleşim x4 (7 aday, KALDI olanlar dahil): `art/results/021r-birlesim-x4.png`: boşluk, kayma, çift dış hat yok. Yan yana: `art/results/021r-yanyana.png`.

## Sorunlar
1. "neutral grey" isteği doygunluğu düşürmedi (021n/021p ile aynı); makyaj parçaları zaten renkli olacağından UYARI kabul edilebilir.
2. Dudak katmanı mavi-gri dudak üretti; allık tabana oval pembe leke olarak oturuyor (kullanıcı değerlendirmesi).
3. Eksik diş katmanı ağzı açık diş sırası çiziyor; stil KALDI.
4. Kırışıklık katmanı tüm bölgeyi (361 px) dolduruyor, ten dahil.
