unity-import: pending
# 472 Cam bahçe bitkileri (8)

- Araç: create_image_pixflux x8 (paralel), 32x48, view=side, outline=lineless, no_background=true; 1 gen/çağrı, toplam 8. Kalan (get_balance): 3012,9 (paralel şeritlerin harcaması da içinde). Masaüstü kopyası: yerel.
- Yan yana: art/results/472-yanyana.png

| Dosya | Boyut | Pivot x / y (alttan) | Stil denetimi | Sonuç |
|---|---|---|---|---|
| viv_seaweed.png | 32x48 | 16 / 2 (alt boşluk 2 satır) | UYARI (soluk x0.60) | teslim: Assets/Art/Iso/_aday/ |
| viv_fern.png | 32x48 | - | KALDI (x1.61; fit x0.72 sonrası da palet dışı) | teslim edilmedi, art/results/472-red/ |
| viv_houseleek.png | 32x48 | - | KALDI (palet dışı, %66 yakın) | teslim edilmedi |
| viv_moss.png | 32x48 | - | KALDI (palet dışı, %50) | teslim edilmedi |
| viv_sea_lettuce.png | 32x48 | - | KALDI (x2,36; fit sonrası da palet dışı) | teslim edilmedi |
| viv_thyme.png | 32x48 | - | KALDI (palet dışı, %44) | teslim edilmedi |
| viv_violet.png | 32x48 | - | KALDI (palet dışı, %29) | teslim edilmedi |
| viv_water_moss.png | 32x48 | - | KALDI (palet dışı, %58) | teslim edilmedi |

validate_iso: seaweed geçti (6 renk, y 4-45). Zemin döşeme testi uygulanmaz (nesne).

## Sorunlar
- `esya` paleti kahverengi ağırlıklı; yeşil/mor bitkiler palet dışı kalıyor. `fit` yalnızca doygunluğu indirir, 7 görsel yine KALDI. Tek deneme kuralı gereği yeniden üretilmedi; kalanlar için yeni iş (ör. bitkiye uygun aile ya da palet genişletmesi) gerekir.
- Kabul kararı kullanıcıda; yan yana sayfasına bakılmalı, ham hâller 472-red'de.
