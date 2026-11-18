unity-import: pending

# 018a Yürüme pilotu: erkek 84 px, SE

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = `iso_char_man_r84_cand_se.png` (upload), yalnızca SE, `man_r84` (c63fe39d). Grup: 7a0ba266-b6d7-467a-bccc-8a11a54a3c14.
- Harcanan: 1 generation (tahmin 2; çağrı makbuzu 1/yön). Tek deneme. `get_balance` sonrası: 4265,9 (diğer şeritler aynı anda harcıyor, bu sayı yalnızca bu işe ait değil).
- Ham çıktı 84x124 (9 kare). Yalnızca kaydırma yapıldı (yatay ofset +1, her karenin en alt opak satırı durma karesinin satırı y=81'e); 84x84'e kırpma sırasında kırpılan opak piksel 0, ölçekleme yok. Palet: durma karesinin 32 rengi, en yakın RGB, alfa ikili.
- Kapanış: kare 0 ile 8 IoU 0,919 (özdeş değil) -> 9 kare de teslim edildi (00-08); döngüde 08'in durma pozuna yakın olduğu not edilir, kullanıcı karar versin. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: art/results/018a-yanyana.png. GIF: art/results/018a-yurume.gif.
- masaüstü kopyası: yerel

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/anim/iso_char_man_r84_walk_se_00..08.png | 84x84 | 42 | 2 |
| Assets/Art/Iso/_aday/anim/iso_char_man_r84_idle_se.png | 84x84 (durma karesi kopyası) | 42 | 2 |

## Ölçüm
| Ölçüt | Değer |
|---|---|
| Opak boy (y) | 4..81 (kare başına tepe y 2-10; kırpılma yok) |
| Ardışık IoU 0-1..7-8 | 0,69 / 0,86 / 0,82 / 0,71 / 0,75 / 0,75 / 0,85 / 0,75 |
| Kapanış IoU (0-8) | 0,919 |
| Palet dışı oran | %0 (32 renk) |
| Tepe kırpılması | yok |
| validate_iso (kare 00) | geçti, 32 renk, alt boşluk 2 |
| style.py check karakter | 9/9 GEÇTİ (palete yakın %100, doygunluk x1,19-1,25; aile kilidi kurulmadı, uyarı düzeyi) |

## Sorunlar
- Kimlik: yan yana sayfasından yüz ve giysi rengi sapması kullanıcı gözüyle onaylanmalı (tablo otomatik ölçmedi). Ardışık IoU 0,69-0,86: hareket büyük, 8 kareyle sıçrama olabilir; GIF'e bakılmalı.
- Tek idle kare yalnızca durma karesinin kopyasıdır.
