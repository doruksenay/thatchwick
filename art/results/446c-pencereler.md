unity-import: pending

# 446c Pencere varyantları (bulut işçisi, şerit 1, 2026-11-19)

Yan yana sayfası: [446c-yanyana.png](446c-yanyana.png) (soldaki 3 referans; sonra grille se_nw, grille sw_ne, shutter blue se_nw, blue sw_ne, green se_nw, green sw_ne, red se_nw, red sw_ne)

## Araç ve ayarlar
- 330 ile aynı yöntem: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; 8 çağrı x 5 = **40 generation** (araç açıklaması: 96 px'e kadar 5; cost_est ile uyumlu). Çağrılar paralel gönderildi, hız sınırı hatası olmadı. `get_balance` sonrası: **3242.9**.
- Kaynak: `iso_wall_window_sw_ne/se_nw.png` (102x115), 96x96 kırpma (ofset sw_ne (0,4), se_nw (6,4)), `get_upload_url` ile yüklendi. Maske (kırpma koordinatı): sw_ne 40,36 19x27; se_nw 45,41 16x26 (330 ile aynı). Sonuç, kaynak kopyasının üstüne aynı ofsetle yapıştırıldı (102x115).
- Geçici dosyalar `art/tmp446c/` altında. Masaüstü kopyası: yerel.

## Dosyalar (102x115; pivot x, y ALTTAN)
| Dosya | Pivot |
|---|---|
| iso_wall_window_shutter_{red,blue,green}_sw_ne.png | 32, 19 |
| iso_wall_window_shutter_{red,blue,green}_se_nw.png | 70, 19 |
| iso_wall_window_grille_sw_ne.png | 32, 19 |
| iso_wall_window_grille_se_nw.png | 70, 19 |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: 8/8 geçti.

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| grille_se_nw | %100 | 2.6 | x1.02 | GEÇTİ |
| grille_sw_ne | %100 | 3.1 | x0.77 | GEÇTİ |
| shutter_blue_se_nw | %91 | 5.4 | x1.09 | GEÇTİ |
| shutter_blue_sw_ne | %88 | 6.6 | x0.83 | GEÇTİ |
| shutter_green_se_nw | %95 | 3.7 | x1.06 | GEÇTİ |
| shutter_green_sw_ne | %100 | 4.3 | x0.81 | GEÇTİ |
| shutter_red_se_nw | %95 | 3.6 | x1.14 | GEÇTİ |
| shutter_red_sw_ne | %99 | 4.1 | x0.95 | GEÇTİ |

## Sorunlar / görsel onay notları
- grille_sw_ne: pencere iç alanı neredeyse beyaz (81 px, 245-254) ve yan yana sayfasında çevreden belirgin açık; palet denetiminden geçti ama gözle kontrol edilmeli. grille_se_nw koyu ve temiz.
- Kepenkler okunuyor (kırmızı/mavi/yeşil); se_nw yeşil biraz soluk. Maske dışı piksel değişmedi; zemin döşeme testi yok (duvar).
- Gözle tam doğrulanmadı; kullanıcı yan yana sayfasına baksın.
