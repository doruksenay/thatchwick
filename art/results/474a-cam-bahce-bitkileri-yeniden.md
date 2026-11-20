unity-import: pending
# 474a Cam bahçe bitkileri yeniden deneme (10)

- Araç: create_image_pixflux x10 (paralel), 32x48, view=side, outline=lineless, no_background=true; 1 gen/çağrı = 10 gen. 4 çağrı hız sınırına (10/10 jobs) takıldı, 30-60 sn sonra bir kez yeniden denendi. Kalan (get_balance): 2978,9 (diğer şeritlerin harcaması da içinde). Masaüstü kopyası: yerel.
- Yan yana: art/results/474a-yanyana.png
- Doygunluk: moss x0.65, duckweed x0.61, hornwort x0.41, sea_lettuce x0.41 (`style.py fit`); ham hâller art/results/474a-red/.

| Dosya | Boyut | Pivot x / y (alttan) | Stil denetimi | Sonuç |
|---|---|---|---|---|
| viv_moss.png | 32x48 | 16 / 6 | UYARI (fit x0.65 sonrası) | teslim |
| viv_fern.png | 32x48 | 16 / 4 | UYARI | teslim |
| viv_violet.png | 32x48 | 16 / 2 | UYARI | teslim |
| viv_water_moss.png | 32x48 | - | UYARI (fit sonrası) | teslim edilmedi: pikselsiz/fotoğraf görünümü, kareyi tamamen dolduruyor (alt boşluk 0), stil dışı |
| viv_houseleek.png | 32x48 | 16 / 11 | UYARI (doygun x1.48) | teslim |
| viv_thyme.png | 32x48 | 16 / 2 | UYARI | teslim |
| viv_sea_lettuce.png | 32x48 | 16 / 2 | UYARI (fit x0.41 sonrası) | teslim |
| viv_duckweed.png | 32x48 | 16 / 9 | UYARI (palete yakın %11) | teslim |
| viv_hornwort.png | 32x48 | 16 / 3 | UYARI (fit sonrası) | teslim |
| viv_lily.png | 32x48 | 16 / 3 | UYARI (doygun x1.48) | teslim |

validate_iso (--kind object): 10 dosya geçti. Teslim: Assets/Art/Iso/_aday/ (9 dosya). 'bitki' ailesinin kilidi kurulmadı, palet uyumu uyarıdır.

## Sorunlar
- viv_water_moss yeniden üretilmedi (tek deneme); yeni iş gerekir (ör. "moss on a stone, flat shading, lineless" ile).
- Houseleek ve nilüfer ailesinden doygun; duckweed palete uzak (%11) ama şekil tanınıyor (yüzen yapraklar). Kabul kararı kullanıcıda.
