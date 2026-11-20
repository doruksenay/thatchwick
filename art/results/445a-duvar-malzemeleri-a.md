unity-import: pending

# 445a Yeni duvar malzemeleri A (bulut işçisi, şerit 0, 2026-11-19)

Yan yana sayfası: [445a-yanyana.png](445a-yanyana.png) (ilk üç görsel referans; sonra brick se_nw/sw_ne, rubble_lime se_nw/sw_ne, timber_black se_nw/sw_ne, timber_red se_nw/sw_ne)

## Araç ve ayarlar
- 326 yöntemi: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`. Kaynak `iso_wall_wall_sw_ne/se_nw.png`, 96x96 pencere sw_ne (0,4), se_nw (6,4); maske betikle (ön yüz opak pikselleri, 3 px yan çerçeve ve üst kapak dışarıda; `art/tmp445/`); çıktı aynı ofsetle 102x115'e geri konuldu, maske dışı değişim 0.
- 8 çağrı x 5 = **40 generation** (araç açıklaması: 96 px'e kadar 5), aynı anda gönderildi, hız sınırı hatası yok. `get_balance` sonrası kalan **3327,9** (paralel şeritler dahil).
- Yükleme `get_upload_url` ile. Masaüstü kopyası: yerel.
- Açıklamalar tarifteki gibi (+ "flat few-tone pixel art, no shadow, light from top-left").

## Dosyalar
| Dosya | Boyut | Pivot (x, y ALTTAN) |
|---|---|---|
| iso_wall_wall_rubble_lime_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_wall_rubble_lime_se_nw.png | 102x115 | 70, 19 |
| iso_wall_wall_brick_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_wall_brick_se_nw.png | 102x115 | 70, 19 |
| iso_wall_wall_timber_red_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_wall_timber_red_se_nw.png | 102x115 | 70, 19 |
| iso_wall_wall_timber_black_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_wall_timber_black_se_nw.png | 102x115 | 70, 19 |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: sekizi geçti. `style.py check --family yapi`:

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| brick_sw_ne | %100 | 6.8 | x1.24 | GEÇTİ |
| brick_se_nw | %100 | 8.6 | x1.28 | UYARI (ailesinden doygun) |
| rubble_lime_sw_ne | %100 | 6.6 | x0.67 | GEÇTİ |
| rubble_lime_se_nw | %100 | 7.3 | x0.66 | GEÇTİ |
| timber_red_sw_ne | %100 | 7.0 | x1.15 | GEÇTİ (fit sonrası) |
| timber_red_se_nw | %98 | 9.6 | x1.34 | UYARI (ailesinden doygun) |
| timber_black_sw_ne | %100 | 7.1 | x0.92 | GEÇTİ |
| timber_black_se_nw | %100 | 3.4 | x1.02 | GEÇTİ |

## Sorunlar / görsel onay notları
- timber_red_sw_ne ham hâli x1.56 ile KALDI (çok doygun); `style.py fit` ile doygunluk x0.74 indirildi, yeniden denetimde geçti. Teslim edilen fit'li hâl; ham hâl `art/results/445a-red/` içinde. Şekil değişmedi.
- timber_black_sw_ne: dolgu beyaz yerine neredeyse boş/açık görünüyor, se_nw ile tutarsız olabilir ⚠ (kullanıcı gözüyle bakmalı). timber_black çatkısı çok ince.
- Kenar çerçevesi (3 px) kaynak renkte kaldığı için malzemeyle küçük renk farkı olabilir (325/326 ile aynı).
- Döşeme testi duvar için uygulanmadı; dikiş yalnızca yan yana sayfadan gözle bakıldı.
