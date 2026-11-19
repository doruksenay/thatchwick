unity-import: pending

# 446b Kapı varyantları B (bulut işçisi, şerit 0, 2026-11-19)

Yan yana sayfası: [446b-yanyana.png](446b-yanyana.png) (soldaki 3 referans; sonra varsayılan kapı se_nw/sw_ne, black se_nw/sw_ne, carved se_nw/sw_ne, studded se_nw/sw_ne; sayfa sıralaması dosya adına göredir)

## Araç ve ayarlar
- 329 ile aynı yöntem: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; 6 çağrı x 5 gen = **30 generation** (araç açıklaması: 96px'e kadar 5, cost_est ile uyumlu).
- Kaynak: `iso_wall_door_sw_ne/se_nw.png`, 96x96 kırpma (ofset sw_ne (0,4), se_nw (6,4)); maske yalnızca kapı kanadı (sw_ne x44 y41 17x39, se_nw x36 y41 17x39). Çıktı aynı ofsetle 102x115'e geri konuldu; maske dışı değişen piksel 0.
- İlk base64 çağrısı aktarımda bozuldu (ücret yok); kırpmalar `get_upload_url` ile yüklenip `upload:<id>` olarak verildi. Hız sınırı hatası olmadı.
- `get_balance` sonrası kalan: **3182.9** (paralel şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Teslim edilenler (102x115)
| Dosya | Pivot (x, y ALTTAN) | Renk |
|---|---|---|
| iso_wall_door_black_sw_ne.png | 32, 19 | 24 |
| iso_wall_door_black_se_nw.png | 70, 19 | 36 |
| iso_wall_door_studded_sw_ne.png | 32, 19 | 32 |
| iso_wall_door_studded_se_nw.png | 70, 19 | 37 |
| iso_wall_door_carved_sw_ne.png | 32, 19 | 24 |
| iso_wall_door_carved_se_nw.png | 70, 19 | 29 |

## Doğrulama
`validate_iso.py`: 6/6 geçti. `style.py check --family yapi`: 6/6 GEÇTİ.

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| black_se_nw | %100 | 3.4 | x0.85 | GEÇTİ |
| black_sw_ne | %100 | 3.7 | x0.91 | GEÇTİ |
| carved_se_nw | %100 | 3.3 | x1.03 | GEÇTİ |
| carved_sw_ne | %100 | 2.8 | x0.94 | GEÇTİ |
| studded_se_nw | %99 | 2.9 | x1.09 | GEÇTİ |
| studded_sw_ne | %100 | 3.6 | x0.94 | GEÇTİ |

## Sorunlar / görsel onay notları
- Oymalı (carved) kapı yan yana sayfasında küçük ve düz görünüyor; geometrik panel zayıf ⚠. Karartılmış ve çivili kapı kalas dokusuyla tanınıyor.
- Dikişsiz yan yana ve ışık yönü gözle denetlenmedi; kullanıcı sayfaya baksın.
- Geçici `art/tmp446b/` klasörü commit edilmedi (git yok sayıyor olmalı; aksi halde silinecek).
