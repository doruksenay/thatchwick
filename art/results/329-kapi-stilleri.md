unity-import: pending

# 329 Kapı stilleri (bulut işçisi, şerit 1, 2026-11-18)

Yan yana sayfası: [329-yanyana.png](329-yanyana.png) (soldaki 3 referans; sonra kemerli, yarım, demir kuşaklı, çaprazlı; her biri se_nw/sw_ne)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; 8 çağrı x 5 gen = **40 generation** (araç açıklaması: 96px'e kadar 5).
- Kaynak: `iso_wall_door_sw_ne/se_nw.png` 102x115, 96x96 kırpma (ofset sw_ne (0,4), se_nw (6,4)); çıktı aynı ofsetle 102x115'e geri konuldu.
- Maske (kırpma koordinatı): dikdörtgen, yalnızca kapı kanadı. sw_ne x44 y41 17x39, se_nw x36 y41 17x39; kemerlide y36'dan 44 yükseklik (kasanın üstü kemere açık). Kasa ve duvar maske dışı.
- Kaynak PNG base64 ile gönderildi (ilk çağrı aktarım hatasıyla reddedildi, ücret yok; paletli yeniden kodlamayla gönderildi).
- `get_balance` sonrası kalan: **715.1** (paralel şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (hepsi 102x115)
| Dosya | Pivot (x, y ALTTAN) | Maske dışı değişen piksel |
|---|---|---|
| iso_wall_door_ledged_sw_ne.png | 32, 19 | 0 |
| iso_wall_door_ledged_se_nw.png | 70, 19 | 0 |
| iso_wall_door_dutch_sw_ne.png | 32, 19 | 0 |
| iso_wall_door_dutch_se_nw.png | 70, 19 | 0 |
| iso_wall_door_iron_sw_ne.png | 32, 19 | 0 |
| iso_wall_door_iron_se_nw.png | 70, 19 | 0 |
| iso_wall_door_arched_sw_ne.png | 32, 19 | 0 |
| iso_wall_door_arched_se_nw.png | 70, 19 | 0 |

## Doğrulama
- `validate_iso.py --kind object` (varsayılan): 8/8 geçti. `--max-colors 48` ile yalnızca arched_se_nw 49 renk (tarif bu sınırı istemiyor; teslim edildi).
- `style.py check --family yapi`: 8/8 GEÇTİ, palete yakın %100.

| Dosya | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|
| arched_se_nw | 3.9 | x0.95 | GEÇTİ |
| arched_sw_ne | 4.8 | x0.82 | GEÇTİ |
| dutch_se_nw | 3.5 | x1.05 | GEÇTİ |
| dutch_sw_ne | 4.2 | x1.00 | GEÇTİ |
| iron_se_nw | 3.5 | x1.05 | GEÇTİ |
| iron_sw_ne | 4.2 | x1.00 | GEÇTİ |
| ledged_se_nw | 3.5 | x1.07 | GEÇTİ |
| ledged_sw_ne | 4.6 | x1.00 | GEÇTİ |

## Sorunlar / görsel onay notları
- Yarım (dutch) kapıda üst/alt yarı ayrımı belirgin değil, üst yarı açık görünmüyor ⚠.
- Çaprazlı ve yarım kapı birbirine benziyor (ikisi de Z kuşaklı tahta) ⚠.
- Dikişsiz yan yana ve ışık yönü gözle denetlenmedi; kullanıcı sayfaya baksın.
