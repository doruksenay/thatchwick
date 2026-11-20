unity-import: pending

# 473b Cam bahçe canlıları B

**Sonuç: 4/8 teslim, 4 stil denetiminde KALDI (teslim edilmedi).** Harcama 8 generation, kalan 3003,9 (get_balance). Masaüstü kopyası: yerel.

Yan yana sayfası: [473b-yanyana.png](473b-yanyana.png) (7 referans + 8 yeni; 471 zemini üstünde 2x doğrulama yapılmadı, sayfa esya referanslarıyla).

## Araç ve ayarlar
`create_image_pixen`, `no_background=true`, `view=side`, `direction=east`, 1 gen/çağrı, 8 paralel çağrı. Tarifteki `create_image_pixflux` 16x16 ve 24x24'ü reddeder (alan tabanı 32x32); pixen aynı maliyette bu boyutları destekler, bu yüzden değiştirildi. Pixflux denemeleri hata verdi, harcama yok. `esya` ailesi için kilit kurulu değil (palet uyumu uyarı niteliğinde).

## Dosyalar (pivot alttan ortada: x = genişlik/2, y = 0)
| Dosya | Boyut | Pivot | Stil | Not |
|---|---|---|---|---|
| `Assets/Art/Iso/_aday/viv_frog.png` | 24x24 | 12, 0 | UYARI (palete uzak %87) | teslim |
| `viv_lizard.png` | 24x24 | 12, 0 | GEÇTİ | teslim |
| `viv_hermit.png` | 24x24 | 12, 0 | UYARI (palete uzak %81) | teslim |
| `viv_cricket.png` | 16x16 | 8, 0 | GEÇTİ (`fit` ile doygunluk x0.60'a indirildi; ham x1.92) | teslim; ham: `473b-red/viv_cricket_ham.png` |
| viv_spider | 16x16 | — | KALDI (palet dışı %61) | teslim edilmedi |
| viv_ground_beetle | 16x16 | — | KALDI (palet dışı, x2.00; `fit` sonrası hâlâ %79) | teslim edilmedi |
| viv_periwinkle | 16x16 | — | KALDI (palet dışı %68) | teslim edilmedi |
| viv_shrimp | 16x16 | — | KALDI (palet dışı %54; `fit` sonrası %66) | teslim edilmedi |

Reddedilenler ve `fit` çıktıları `art/results/473b-red/` içinde. Şekiller yan yana sayfasında tanınıyor; sorun yalnızca palet. Kullanıcı isterse kabul edebilir ya da yeniden üretim işi açılabilir.

## Doğrulama
`validate_iso.py`: 8/8 geçti. Zemin değil, döşeme testi uygulanmadı. Gölge pişirilmemiş, ışık sol-üst.

## Sorunlar
- Yönergedeki `pixflux` tarifi 16/24 px için uygulanamaz; sonraki cam bahçe tarifleri `pixen` yazmalı.
- Eksik 4 canlı için yeniden üretim gerekir (473a ile aynı sorun çıkabilir).
