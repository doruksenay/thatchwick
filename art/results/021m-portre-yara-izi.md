unity-import: pending

# 021m Portre yara izi: çizik: sonuç

Durum: kısmi. 5/8 dosya teslim edildi, 3/8 stil denetiminde KALDI (teslim edilmedi).
Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, 5 gen/çağrı, seed 0, tek deneme, 8 çağrı paralel (hız sınırı hatası olmadı). Taban `portrait_base_oval_adult_64.png` (`get_upload_url`, `image_url=upload:...`). Maske dikdörtgen parametreleriyle (mask_x/y/width/height; saf siyah/beyaz ile aynı etki): forehead (22,8,21x10), left_brow (15,16,15x8), right_cheek (37,28,12x13), left_cheek (16,28,12x13), nose_bridge (28,20,9x13), chin (25,44,15x11), jaw_left (15,36,13x15), neck (24,52,17x11).
Açıklama: `scratch scar on the <bölge>, neutral light, neutral grey, will be recoloured` + ortak ek.

## Harcama
8 x 5 = **40 gen** (cost_max 40). `reduce_colors` gerekmedi (en çok 19 renk). `get_balance` sonrası: 1293,7 (paralel şeritler dahil). Defter: `art/ledger/021m.md`.

## Dosyalar (portre, pivot yok; 64x64, yarı saydam piksel 0)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil | Durum |
|---|---|---|---|
| portrait_layer_scar_scratch_left_brow_64.png | 16 | UYARI (doygun x1.29) | teslim |
| portrait_layer_scar_scratch_left_cheek_64.png | 7 | UYARI (palete uzak, doygun x1.44) | teslim |
| portrait_layer_scar_scratch_chin_64.png | 13 | UYARI (palete uzak) | teslim |
| portrait_layer_scar_scratch_jaw_left_64.png | 15 | GEÇTİ | teslim |
| portrait_layer_scar_scratch_neck_64.png | 11 | GEÇTİ | teslim |
| portrait_layer_scar_scratch_forehead_64.png | 12 | **KALDI** (palete uzak, doygun x1.77) | teslim edilmedi: `art/results/021m-red/` |
| portrait_layer_scar_scratch_right_cheek_64.png | 10 | **KALDI** (palete uzak, doygun x1.56) | teslim edilmedi: `art/results/021m-red/` |
| portrait_layer_scar_scratch_nose_bridge_64.png | 19 | **KALDI** (palete uzak, doygun x1.54) | teslim edilmedi: `art/results/021m-red/` |

Yan yana: `art/results/021m-yanyana.png`. Taban birleşim önizlemesi (x4; sıra: forehead, left_brow, right_cheek, left_cheek, nose_bridge, chin, jaw_left, neck): `art/results/021m-onizleme-x4.png`.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 19 (≤ 32). Maske dışı piksel: 0 (8/8).
- `validate_iso.py`: portre katmanı, yalnızca boyut/alfa anlamlı; elle çalıştırılmadı.
- Birleşim önizlemesi üretildi; görsel inceleme kullanıcıda. Döşeme testi: uygulanmaz.

## Sorunlar
1. Üç parça KALDI (doygunluk, ten tonlu piksellerle dolu maske); teslim edilmedi, tek deneme kuralı gereği yeniden denenmedi.
2. Çıktılar yalnızca çizik çizgisini değil maske içini ten rengiyle de dolduruyor (021k sakal ile aynı durum); piksel sayıları yüksek. Çizik çizgisinin ayırt edilirliğini kullanıcı gözüyle denetlemeli.
3. Çizikler nötr açık pembe-gri istenmişti; stil UYARI'ları buradan.
