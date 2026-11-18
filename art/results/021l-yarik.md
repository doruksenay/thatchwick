unity-import: pending

# 021l Portre yara izi (yarık): sonuç

Durum: kısmi. 8 çağrıdan 4 dosya teslim edildi, 4 dosya stil denetiminde KALDI (teslim edilmedi, `art/results/021l-red/`).
Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, 5 gen/çağrı, seed 0, tek deneme, 8 çağrı paralel (hız sınırı olmadı). Taban `portrait_base_oval_adult_64.png` (`get_upload_url`). Maskeler betikle, saf siyah/beyaz dikdörtgen (x0,y0)-(x1,y1; görüntü solu = "left"):
forehead (24,12)-(41,22), left_brow (22,22)-(32,27), right_cheek (35,29)-(43,38), left_cheek (21,29)-(29,38), nose_bridge (29,24)-(35,32), chin (27,42)-(37,48), jaw_left (20,36)-(28,46), neck (25,48)-(39,54).
Açıklama: `slash scar on the <bölge>, neutral light, neutral grey, will be recoloured` + ortak ek.

## Harcama
8 x 5 = **40 gen** (cost_max 40). `reduce_colors` gerekmedi (en çok 18 renk). `get_balance` sonrası: 1303,7 (paralel şeritler dahil). Defter: `art/ledger/021l.md`.

## Dosyalar (portre, pivot yok; 64x64, yarı saydam piksel 0)
| Dosya | Renk | Stil (palete yakın / uzaklık / doygunluk) | Durum |
|---|---|---|---|
| portrait_layer_scar_slash_chin_64.png | 7 | %100 / 11.9 / x1.32 UYARI | teslim |
| portrait_layer_scar_slash_jaw_left_64.png | 11 | %100 / 10.1 / x1.13 UYARI | teslim |
| portrait_layer_scar_slash_left_cheek_64.png | 11 | %100 / 12.3 / x1.47 UYARI | teslim |
| portrait_layer_scar_slash_neck_64.png | 9 | %100 / 9.1 / x0.96 GEÇTİ | teslim |
| portrait_layer_scar_slash_forehead_64.png | 18 | %98 / 12.4 / x1.61 KALDI | teslim edilmedi |
| portrait_layer_scar_slash_left_brow_64.png | 14 | %100 / 12.6 / x1.56 KALDI | teslim edilmedi |
| portrait_layer_scar_slash_nose_bridge_64.png | 15 | %100 / 13.6 / x1.60 KALDI | teslim edilmedi |
| portrait_layer_scar_slash_right_cheek_64.png | 13 | %100 / 12.9 / x1.59 KALDI | teslim edilmedi |

Yan yana: `art/results/021l-yanyana.png`. Taban birleşim önizlemesi (x3; sıra: chin, forehead, jaw_left, left_brow, left_cheek, neck, nose_bridge, right_cheek): `art/results/021l-birlesim-x3.png`.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk <= 18 (<= 32). Maske dışı piksel: 0 (8/8).
- Birleşim önizlemesi üretildi; kayma/çift dış hat gözle kontrolü kullanıcıda.
- Döşeme testi: portre katmanı, uygulanmaz.

## Sorunlar
1. 4 bölge (alın, sol kaş, burun sırtı, sağ yanak) aşırı doygun (x1.56-1.61) çıktı, KALDI; üretim dikdörtgeni ten dolgusunu da yeniden çizdiği için ten doygunlaşıyor (021k ile aynı örüntü). Öneri: daha dar maske ya da `desaturated pale pink-grey` açıklamasıyla yeniden iş.
2. Maske dikdörtgen; çıktılar yalnızca yarayı değil çevre teni de içeriyor. Yara okunurluğunu kullanıcı gözle değerlendirmeli.
