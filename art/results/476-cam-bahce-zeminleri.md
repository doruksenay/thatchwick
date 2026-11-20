unity-import: pending

# 476 Cam bahçe iç zeminleri: sonuç FAILED (teslim yok)

- Araç: `create_image_pixflux`, 160x96, view=side, no_background=false, tarifteki üç metin aynen. 3 generation (çağrı başına 1).
- Hız sınırı: ilk 3 çağrı "rate limit exceeded (10/10 jobs)" ile reddedildi (diğer şeritler dolduruyordu); bekleyip bir kez yeniden denendi, geçti. Reddedilenler ücretlendirilmedi.
- `get_balance` sonrası: 2964,9 / 5000 (diğer şeritlerin harcaması dahil).
- Yan yana sayfası: `art/results/476-yanyana.png`. Reddedilen ham dosyalar: `art/results/476-red/`.

## Stil denetimi (aile zemin, kilit kurulmadı: palet yalnızca uyarı)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk/aile | Sonuç |
|---|---|---|---|---|
| viv_bg_dry | %35 | 31.4 | x1.79 | KALDI |
| viv_bg_salt | %21 | 57.3 | x2.47 | KALDI |
| viv_bg_fresh | %47 | 22.4 | x1.40 | UYARI |

## Neden teslim edilmedi
Üçünde de tarifin "çerçevesiz, bitki yok, ön planda nesne yok" şartı tutmadı:
- dry: koyu çerçeve/kenar, arka planda dağ ve çalılar, ön planda taşlar.
- salt: iki yeşil su bitkisi, çerçeve.
- fresh: yoğun su bitkileri, belirgin kalın çerçeve.
Bu şekil sorunudur, `fit` ile çözülmez. İş akışı kuralı gereği tek deneme yapıldı (`max_attempts: 2` olsa da işçi kuralı yeniden denemeyi yasaklar).

## Öneri (`## Sorular`)
Ayrı bir deneme işiyle: açıklamadan "glass case" ifadesini kaldırıp `a flat empty background of ... , nothing in the foreground` benzeri anlatım, ya da kabul edilmiş `viv_bg_moist`'i `init_image` olarak (strength ~150) kullanıp yalnızca zemin ve su rengini değiştirmek.

Masaüstü kopyası: yerel.
