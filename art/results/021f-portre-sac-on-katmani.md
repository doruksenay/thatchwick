unity-import: pending

# 021f Portre saç ön katmanı: sonuç

Durum: tamam. 10/10 dosya teslim edildi (KALDI yok, 2 UYARI). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, 5 gen/çağrı, 10 çağrı (8 eşzamanlı + 2 sonradan; hız sınırı hatası yok). Taban `portrait_base_oval_adult_64.png` (`image_url=upload:`). Maskeler betikle, saf siyah/beyaz: kısa saçlar (short_*, bowl, buzz) (8,2)-(55,27); omuza inenler (medium_*, shoulder_curly, long_*) aynı alan + yanlarda (8,28)-(21,62) ve (42,28)-(55,62). Açıklama: `<ad> hairstyle, neutral grey hair, front layer` + ortak ek. 4 dosya 32 renkten fazlaydı: tek `reduce_colors` çağrısı (32 renk, 0,1 gen), alfa değişmedi.

## Harcama
10 x 5 + 0,1 = 50,1 gen (cost_max 50; renk indirgeme 0,1 küçük adım, sapma yok). `get_balance` sonrası: 1534,1 (paralel şeritler dahil). Defter: `art/ledger/021f.md`.

## Dosyalar (portre, pivot yok; 64x64)
| Yol (Assets/Art/Iso/_aday/portrait_layer_hair_*_64.png) | Stil | Renk |
|---|---|---|
| bowl | GEÇTİ x0.70 | 23 |
| buzz | UYARI (palete uzak, x1.22) | 21 (reduce_colors, 34'ten) |
| long_straight | GEÇTİ x0.66 | 29 |
| long_wavy | GEÇTİ x0.76 | 26 (reduce_colors, 36'dan) |
| medium_straight | GEÇTİ x0.88 | 31 |
| medium_wavy | GEÇTİ x0.87 | 29 |
| short_crop | UYARI (palete uzak, x1.01) | 29 |
| short_messy | GEÇTİ x0.82 | 22 (reduce_colors, 34'ten) |
| short_side_part | GEÇTİ x0.98 | 27 |
| shoulder_curly | GEÇTİ x1.00 | 24 (reduce_colors, 35'ten) |

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 31, maske dışı piksel 0.
- Yan yana: `art/results/021f-yanyana.png`. Taban birleşimi x3: `art/results/021f-birlesim-x3.png`.
- Birleşim: boşluk, kayma, çift dış hat yok. Çok uzun saçlar (long_*) yüz kenarını doğal çerçeveliyor.

## Sorunlar
1. medium_straight kulak hizasında bitiyor, bob'a yakın; medium_wavy yanlarda ince dağınık pikseller var.
2. buzz ve short_crop UYARI (palete uzak, doygunluk normal).
3. Tüm parçalar oval_adult tabanında üretildi; diğer yüz biçimlerinde hizalama ayrıca denetlenmeli.
