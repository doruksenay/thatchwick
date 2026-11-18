unity-import: pending

# 021n Portre yara izi: yanık: sonuç

Durum: kısmi. 5/8 dosya teslim edildi; 3/8 stil denetiminde KALDI (teslim edilmedi). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, seed 0, tek deneme, 8 çağrı paralel. Taban `portrait_base_oval_adult_64.png` (`get_upload_url`, `image_url=upload:`). Maskeler betikle, saf siyah/beyaz, bölge dikdörtgenleri (x0,y0)-(x1,y1) yarı açık: forehead (24,8)-(40,17), left_brow (22,17)-(31,22), right_cheek (34,28)-(42,37), left_cheek (22,28)-(30,37), nose_bridge (29,19)-(35,30), chin (26,38)-(38,45), jaw_left (21,33)-(29,43), neck (26,43)-(38,50). Açıklama: `burn scar on the <bölge>, neutral light, neutral grey, will be recoloured` + ortak ek.

## Harcama
8 çağrı x 5 = 40 gen (cost_max 40). `get_balance` sonrası: 1253,7 (paralel şeritler dahil). Defter: `art/ledger/021n.md`.

## Dosyalar (portre, pivot yok; 64x64)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil |
|---|---|---|
| portrait_layer_scar_burn_left_brow_64.png | 12 | GEÇTİ x1.16 |
| portrait_layer_scar_burn_nose_bridge_64.png | 13 | UYARI (palete uzak) x1.21 |
| portrait_layer_scar_burn_chin_64.png | 9 | UYARI (palete uzak, doygun) x1.43 |
| portrait_layer_scar_burn_jaw_left_64.png | 11 | UYARI (palete uzak, doygun) x1.38 |
| portrait_layer_scar_burn_neck_64.png | 15 | UYARI (palete uzak) x1.14 |

Teslim edilmeyen (`art/results/021n-red/`): forehead (x1.64), left_cheek (x1.51), right_cheek (x1.82): hepsi KALDI (palete uzak, çok doygun). Yeniden üretim gerekir.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 15 (≤ 32).
- Maske dışı piksel: 0 (8/8).
- Birleşim x4 (5 teslim edilen): `art/results/021n-birlesim-x4.png`. Yan yana: `art/results/021n-yanyana.png`.
- Not: katmanlar maske dikdörtgenini tenle birlikte dolduruyor; görsel kontrol kullanıcıda.

## Sorunlar
1. "neutral grey" isteği yanık için de doygunluğu düşürmedi (turuncu/kahve); 3/8 KALDI.
2. Yanık kabarık leke yerine çoğunlukla dikdörtgen yama gibi çıktı; yeniden üretimde daha dar maske önerilir.
