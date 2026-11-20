unity-import: pending

# 445b Yeni duvar malzemeleri B (bulut işçisi, şerit 1, 2026-11-19)

Yan yana sayfası: [445b-yanyana.png](445b-yanyana.png) (ilk üç görsel referans; sonra varsayılan duvar sw_ne/se_nw, ardından yeni 6 dosya)

## Araç ve ayarlar
- 326 yöntemi: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`. 6 çağrı x 5 = **30 generation** (araç açıklaması: 96 px'e kadar 5), aynı anda gönderildi. `get_balance` sonrası kalan **3312,9** (paralel şeritler dahil). Tek deneme, hız sınırı hatası yok.
- Kaynak `iso_wall_wall_sw_ne/se_nw.png`, 96x96 pencere sw_ne (0,4), se_nw (6,4). Maske betikle üretildi (326'daki maskeler LFS işaretçisi olduğu için yeniden yazıldı): ön yüz opak pikselleri, x 35..60 ve y >= 32; çerçeve ve üst kapak dışarıda. Maske dışı piksel farkı 0 (6/6). Yükleme `get_upload_url` ile.
- Açıklamalar: `dark basalt block wall`, `fine red sandstone ashlar wall`, `white marble ashlar wall, thin grey veins` (+ flat few-tone pixel art, no shadow, light from top-left).
- Masaüstü kopyası: yerel.

## Dosyalar
| Dosya | Boyut | Pivot (x, y ALTTAN) | Renk |
|---|---|---|---|
| iso_wall_wall_black_stone_sw_ne.png | 102x115 | 32, 19 | 28 |
| iso_wall_wall_black_stone_se_nw.png | 102x115 | 70, 19 | 29 |
| iso_wall_wall_red_ashlar_sw_ne.png | 102x115 | 32, 19 | 32 |
| iso_wall_wall_red_ashlar_se_nw.png | 102x115 | 70, 19 | 32 |
| iso_wall_wall_marble_sw_ne.png | 102x115 | 32, 19 | 25 |
| iso_wall_wall_marble_se_nw.png | 102x115 | 70, 19 | 31 |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: altısı geçti (yarı saydam 0). `style.py check --family yapi`:

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| black_stone_sw_ne | %100 | 6.3 | x0.52 | UYARI (ailesinden soluk) |
| black_stone_se_nw | %100 | 4.6 | x0.68 | GEÇTİ |
| red_ashlar_sw_ne | %100 | 4.9 | x1.06 | GEÇTİ |
| red_ashlar_se_nw | %100 | 6.5 | x1.12 | GEÇTİ |
| marble_sw_ne | %100 | 7.9 | x0.65 | GEÇTİ |
| marble_se_nw | %100 | 8.5 | x0.62 | GEÇTİ |

## Sorunlar / görsel onay notları
- Kenar çerçevesi (3 px) ve üst kapak kaynak (kerpiç/krem çatkı) renginde kaldı; malzeme yalnızca ön yüzde, çerçeve ahşap görünür (325/326 ile aynı sınırlama).
- Kaynaktaki çapraz çatkı kısmen kalmış olabilir (özellikle mermer sw_ne'de ince çizgiler çapraz gibi okunabilir); gözle bakılmalı.
- Döşeme testi duvar için uygulanmadı; dikiş yalnızca yan yana sayfadan gözle bakıldı.
- Dönem kuralları: ortaçağ sonrası icat ve Yeni Dünya malzemesi yok.
