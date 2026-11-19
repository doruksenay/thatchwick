unity-import: pending

# 423b Yaşlı erkek SE: küreği sil

Sonuç: kürek silindi, `iso_char_elder_m_r84_idle_se_v3.png` teslim edildi (2. denemede). İş dosyası `max_attempts: 2` olduğu için ikinci deneme yapıldı.

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`; 5 gen/çağrı (çağrı öncesi maliyet okundu: 5).
- Kaynak: `iso_char_elder_m_r84_idle_se.png` (415). Tuval 96x96'ya alttan genişletildi (ofset x 6, y 0; üstte kırpma yok), çıktı aynı ofsetle 84x84'e kırpıldı. Maske kutusu (kaynak koordinatı) x 24-46, y 60-84 (tuvalde x 30-52). Yükleme `upload:` ile (Python multipart).
- Deneme 1 (seed 0, tarifteki istem): kürek bıçağı kaldı, teslim edilmedi (121 piksel değişti, bıçak yerinde).
- Deneme 2 (seed 7, istem: "... no blade, no handle, nothing held, nothing below the hand but empty space ..."): kürek gitti, sol bacak ve bot çizildi.
- Palet: maske içi yeni renkler 415 paletindeki (31 renk) en yakın renge eşlendi (40 piksel). Sonuç 31 renk.

## Harcama
2 x 5 = **10 gen** (`cost_max` 12 içinde). `get_balance` sonrası kalan: 3499,9.

## Dosya
| Dosya | Boyut | Pivot |
|---|---|---|
| Assets/Art/Iso/_aday/anim/iso_char_elder_m_r84_idle_se_v3.png | 84x84 | (42,2) alttan |

## Doğrulama
- `validate_iso --kind object --max-colors 32`: geçti (31 renk, yarı saydam 0).
- Maske dışı piksel farkı: 0 (ham çıktıda kaynağa karşı).
- `style.py check --family karakter`:

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| se_v3 | %95 | 7.2 | x0.86 | GEÇTİ |

- Yan yana sayfası: `art/results/423b-yanyana.png`. Önce/sonra (4 kat; 415 se, 423 reddedilen kürekli se, v3 se): `art/results/423b-oncesonra.png`.

## Sorunlar
- Maske kutusu bacak/bot piksellerini yeniden çizdi (sol bot artık 415'ten farklı, sargılı); görsel kontrol kullanıcıda.
- Eski dosya değişmedi; kabulde `_v3` adı değiştirilir ya da eskisi kaldırılır.
- Masaüstü kopyası: yerel.
