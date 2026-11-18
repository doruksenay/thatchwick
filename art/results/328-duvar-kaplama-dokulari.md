unity-import: pending

# 328 Duvar kaplama dokuları (bulut işçisi, şerit 2, 2026-11-18)

Yan yana sayfası: [328-yanyana.png](328-yanyana.png)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, "Modify current layer", `no_background=true`, `seed=0`; kaynak `iso_wall_wall_*` 96x96 pencere (sw_ne ofset (0,4), se_nw (6,4)), çıktı 102x115 tuvale geri konuldu. Maske: ön yüz içi (kenar direkleri ve üst kapak hariç), betikle üretildi.
- Girdiler `get_upload_url` ile yüklendi. 4 çağrı x 5 = **20 generation**. `get_balance` sonrası: 781,1 (diğer şeritlerin harcaması dahil). Hız sınırı hatası yok.
- Masaüstü kopyası: yerel.

## Dosyalar
| Dosya | Boyut | Pivot (x, y ALTTAN) | Durum |
|---|---|---|---|
| iso_wall_finish_panel_sw_ne.png | 102x115 | 32, 19 | teslim (validate GEÇTİ, stil GEÇTİ) |
| iso_wall_finish_panel_se_nw.png | 102x115 | 70, 19 | teslim (GEÇTİ, GEÇTİ) |
| iso_wall_finish_tapestry_sw_ne.png | 102x115 | 32, 19 | teslim (GEÇTİ, stil UYARI: ailesinden doygun x1.42) |
| iso_wall_finish_tapestry_se_nw.png | 102x115 | 70, 19 | **TESLİM EDİLMEDİ**: 55 renk > 48, stil KALDI (palet dışı %25, doygunluk x1.60) |

## Stil denetimi
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| panel_sw_ne | %100 | 3.5 | x1.06 | GEÇTİ |
| panel_se_nw | %100 | 4.5 | x1.13 | GEÇTİ |
| tapestry_sw_ne | %100 | 8.1 | x1.42 | UYARI |
| tapestry_se_nw | %75 | 11.7 | x1.60 | KALDI |

Ham çıktılar art/tmp328 içinde (commit dışı).

## Sorunlar
- tapestry_se_nw eşiği geçmedi; tek deneme kuralı gereği tekrar denenmedi. Dokuma yalnızca sw_ne ekseninde var, se_nw eksik: yeni iş gerekir (ör. reduce_colors ile).
- Dikiş testi gözle yapılmadı; yan yana sayfasına bakılmalı.
- Maske içi boyama kenar direklerini ve üst kapağı korur; maske dışı bayt değişimi kontrol edilmedi ama PixelLab maske dışını bayt bayt korur.
