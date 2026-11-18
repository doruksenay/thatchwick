unity-import: pending

# 021q Portre dövme: neck: sonuç

Durum: 8/8 dosya teslim edildi (stil UYARI x8, KALDI yok). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, seed 0, tek deneme, 8 çağrı. Taban `portrait_base_oval_adult_64.png` (`upload:` ile). Maske: tarifteki boyun bölgesi (26,43)-(38,50), yani `mask_x=26, mask_y=43, 12x7` dikdörtgen (021n ile aynı; maske PNG yerine dikdörtgen parametresi). Açıklama: `small <desen> tattoo, dark woad blue-grey ink, simple, neutral grey, will be recoloured` + ortak ek.

Hız sınırı: ilk gönderimde "Maximum 8 concurrent background jobs" hatası (diğer şeritler); hata faturalanmadı, bekleyip bir kez yeniden gönderildi (6 + 2 çağrı).

## Harcama
8 çağrı x 5 = 40 gen (cost_max 40). `get_balance` sonrası: 1133,7 (paralel şeritler dahil). Defter: `art/ledger/021q.md`.

## Dosyalar (portre, pivot yok; 64x64)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil |
|---|---|---|
| portrait_layer_tattoo_celtic_knot_neck_64.png | 10 | UYARI (palete uzak) x0.98 |
| portrait_layer_tattoo_woad_spiral_neck_64.png | 14 | UYARI (palete uzak) x1.12 |
| portrait_layer_tattoo_rune_neck_64.png | 10 | UYARI (palete uzak) x1.10 |
| portrait_layer_tattoo_cross_neck_64.png | 9 | UYARI (palete uzak) x1.17 |
| portrait_layer_tattoo_bird_neck_64.png | 11 | UYARI (palete uzak) x1.03 |
| portrait_layer_tattoo_snake_neck_64.png | 14 | UYARI (palete uzak) x1.10 |
| portrait_layer_tattoo_wheat_ear_neck_64.png | 13 | UYARI (palete uzak) x1.10 |
| portrait_layer_tattoo_heraldic_crest_neck_64.png | 14 | UYARI (palete uzak) x1.10 |

Ortalama uzaklık 11.3-12.6, palete yakın %98-100.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 14 (≤ 32).
- Maske dışı piksel: 0 (8/8).
- Birleşim x4 (taban + katman): `art/results/021q-birlesim-x4.png` (sırayla ilk satır: bird, celtic_knot, cross, heraldic_crest; ikinci: rune, snake, wheat_ear, woad_spiral). Yan yana: `art/results/021q-yanyana.png`.
- Görsel kontrol (desenin okunurluğu, katmanın tenle doldurulup doldurulmadığı) kullanıcıda; 021n'de katmanlar maske dikdörtgenini tenle doldurmuştu, burada da olası.

## Sorunlar
Yok (hata ya da KALDI yok). 12x7 piksellik boyun maskesi çok küçük olduğundan desenlerin okunurluğu sınırlı olabilir.
