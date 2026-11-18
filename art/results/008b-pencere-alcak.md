unity-import: pending

# 008b Duvar pencere ve alçak duvar (bulut işçisi, 2026-11-18)

Yan yana sayfası: [008b-yanyana.png](008b-yanyana.png)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0` (tarifteki gibi). Maske dışı baytlar korunur.
- Kaynak 102x115 tuvalden 96x96 kırpıldı (5 gen/çağrı), çıktı aynı ofsetle 102x115'e geri konuldu: pencere sw_ne (0,4), se_nw (6,4); alçak sw_ne (0,19), se_nw (6,19).
- Alçak ön hazırlık (betik, yalnızca silme): ön yüzde `y >= b(x)-9` ve kesim yüzü paralelkenarının altı kaldı; geri kalan silindi. Maskeler tarifteki geometriden betikle üretildi, 1 px genişletildi.
- 4 çağrı x 5 = **20 generation**. `get_balance` sonrası kalan: **1823.8**. Tek deneme, tekrar yok.
- Masaüstü kopyası: yerel (bulutta yapılmadı).

## Dosyalar
| Dosya | Boyut | Pivot (x, y ALTTAN) | Renk | Yarı saydam | Maske dışı değişen |
|---|---|---|---|---|---|
| Assets/Art/Iso/iso_wall_window_sw_ne.png | 102x115 | 32, 19 | 23 | 0 | 0 |
| Assets/Art/Iso/iso_wall_window_se_nw.png | 102x115 | 70, 19 | 24 | 0 | 0 |
| Assets/Art/Iso/iso_wall_wall_low_sw_ne.png | 102x115 | 32, 19 | 13 | 0 | 0 |
| Assets/Art/Iso/iso_wall_wall_low_se_nw.png | 102x115 | 70, 19 | 18 | 0 | 0 |

## Doğrulama
- `validate_iso.py --kind object --max-colors 48`: dördü GEÇTİ.
- `style.py check --family yapi`: dördü GEÇTİ (palete yakın %100).

| Dosya | Ortalama uzaklık | Doygunluk | Sonuç |
|---|---|---|---|
| window_sw_ne | 2.7 | x0.83 | GEÇTİ |
| window_se_nw | 2.4 | x1.08 | GEÇTİ |
| wall_low_sw_ne | 5.5 | x0.84 | GEÇTİ |
| wall_low_se_nw | 1.3 | x1.12 | GEÇTİ |

## Pencere açıklığı (ön yüz, 14x21 = 294 px)
- sw_ne: 187 piksel saydam (açıklığın çoğu gerçek delik).
- se_nw: yalnızca 88 piksel saydam ⚠ (açıklığın büyük kısmı çerçeve/pervaz ya da koyu dolgu olarak boyanmış). Düzeltme yapılmadı.
- Cam ya da kepenk görsel olarak denetlenmedi; kullanıcı yan yana sayfasına baksın.

## Alçak duvar
- En üst opak satır: sw_ne y=62, se_nw y=63. Ön yüz tabanından 10+6 px sınırının üstünde tek tük piksel var ⚠ (sw_ne'de (52,62) civarında tek piksel, maskenin 1 px genişleme bandında üretilmiş). Düzeltme yapılmadı.
- Işık ve pişmiş gölge ile döşeme/komşu dikişi gözle denetlenmedi; kullanıcı onayına bırakıldı. Oda önizlemesi (masaüstü) yapılmadı.

## Sorunlar
- Yok (hata ya da maliyet sapması yok). Yukarıdaki ⚠ notlar görsel onay içindir.
