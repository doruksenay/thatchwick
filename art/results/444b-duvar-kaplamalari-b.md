unity-import: pending

# 444b Yeni duvar kaplamaları B (bulut işçisi, şerit 2, 2026-11-19)

Yan yana sayfası: [444b-yanyana.png](444b-yanyana.png)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, "Modify current layer", `no_background=true`, `seed=0` (328 ile aynı yöntem). cost_max 30 için iki ekseni **tek 192x96 tuvalde** birleştirdim (sol: sw_ne 96x96 pencere, ofset (0,4); sağ: se_nw, ofset (6,4)). Çağrı 6 gen (10 ayrı çağrı 50 gen olurdu). Maske: ön yüz içi (direkler ve üst kapak hariç), betikle üretildi. 5 çağrı x 6 = **30 gen**. Çıktı ikiye bölünüp 102x115 tuvale geri konuldu.
- `get_balance` sonrası: 3382,9 (diğer şeritlerin harcaması dahil). Hız sınırı hatası yok.
- Alçak ve yarım hâller üretilmedi (iş çıktılarında yok; `tools/art/wall_variants.py` bu depoda yok). Masaüstü kopyası: yerel.

## Dosyalar (hepsi 102x115; pivot x 32 sw_ne / 70 se_nw, y 19 alttan)
| Dosya | Doğrulama | Stil | Doygunluk |
|---|---|---|---|
| rose_wash sw_ne / se_nw | GEÇTİ / GEÇTİ | GEÇTİ / GEÇTİ | x1.04 / x1.16 |
| green_wash sw_ne / se_nw | GEÇTİ / GEÇTİ | GEÇTİ / GEÇTİ | x0.81 / x0.96 |
| red_band sw_ne / se_nw | GEÇTİ / GEÇTİ | UYARI (ailesinden doygun) / GEÇTİ | x1.34 / x1.23 |
| black_panel sw_ne / se_nw | GEÇTİ / GEÇTİ | GEÇTİ / GEÇTİ | x0.97 / x1.00 |
| checker_paint sw_ne / se_nw | GEÇTİ / GEÇTİ | GEÇTİ / GEÇTİ | x1.09 / x1.17 |

10 dosya `Assets/Art/Iso/` altına teslim edildi.

## Sorunlar
- Dikiş testi gözle yapılmadı; yan yana sayfasına bakılmalı.
- Alçak ve yarım varyantlar ayrı işte (402 yöntemi) üretilmeli.
