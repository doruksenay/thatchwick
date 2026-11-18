unity-import: pending

# 021o Portre yara izi: ısırık: sonuç

Durum: kısmi. 4/8 dosya teslim edildi; 4/8 stil denetiminde KALDI (teslim edilmedi). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, seed 0, tek deneme, 8 çağrı paralel. Taban `portrait_base_oval_adult_64.png` (`get_upload_url`, `image_url=upload:`). Maskeler betikle, saf siyah/beyaz, 021n ile aynı bölge dikdörtgenleri (yarı açık): forehead (24,8)-(40,17), left_brow (22,17)-(31,22), right_cheek (34,28)-(42,37), left_cheek (22,28)-(30,37), nose_bridge (29,19)-(35,30), chin (26,38)-(38,45), jaw_left (21,33)-(29,43), neck (26,43)-(38,50). Açıklama: `bite scar on the <bölge>, neutral light, neutral grey, will be recoloured` + ortak ek.

## Harcama
8 çağrı x 5 = 40 gen (cost_max 40). `get_balance` sonrası: 1183,7 (paralel şeritler dahil). Defter: `art/ledger/021o.md`.

## Dosyalar (portre, pivot yok; 64x64)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil |
|---|---|---|
| portrait_layer_scar_bite_left_brow_64.png | 14 | UYARI (doygun) x1.28 |
| portrait_layer_scar_bite_chin_64.png | 6 | UYARI (palete uzak, doygun) x1.48 |
| portrait_layer_scar_bite_jaw_left_64.png | 12 | UYARI (palete uzak, doygun) x1.31 |
| portrait_layer_scar_bite_neck_64.png | 12 | UYARI (palete uzak) x1.22 |

Teslim edilmeyen (`art/results/021o-red/`): forehead (x1.86), left_cheek (x1.52), nose_bridge (x1.51), right_cheek (x1.71): hepsi KALDI (palete uzak, çok doygun). Yeniden üretim gerekir.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 19 (≤ 32); reduce_colors gerekmedi.
- Maske dışı piksel: 0 (8/8).
- `validate_iso.py` (chin): geçti.
- Birleşim x4 (teslim edilen 4): `art/results/021o-birlesim-x4.png`. Yan yana: `art/results/021o-yanyana.png`.
- Not: görsel kontrol kullanıcıda; ısırık deseni (yarım daire diş izi) okunurluğu bakılmadı.

## Sorunlar
1. "neutral grey" isteği doygunluğu yine düşürmedi; 4/8 KALDI (021n'de 3/8). Yeniden üretimde daha dar maske ve "desaturated grey-pink" ifadesi önerilir.
