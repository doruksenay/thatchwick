unity-import: pending

# 330 Pencere stilleri (bulut işçisi, şerit 2, 2026-11-18)

Yan yana sayfası: [330-yanyana.png](330-yanyana.png)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0` (008b ile aynı). 10 çağrı x 5 = **50 generation** (araç açıklaması: 96 px'e kadar 5/çağrı, `cost_est` ile uyumlu). `get_balance` sonrası: **685,1** (hesap diğer şeritlerle paylaşılıyor).
- Kaynak: mazgal, kafesli, geniş, kurşun çerçeveli için 008b `iso_wall_window_*`; bodrum için `iso_wall_wall_*` (düz duvar). 102x115 tuvalden 96x96 kırpıldı (sw_ne ofset (0,4), se_nw (6,4)), sonuç aynı ofsetle 102x115'e geri konuldu. Maske (kırpma koordinatı, dikdörtgen): sw_ne 40,36 19x27 (geniş 35,34 26x31); se_nw 45,41 16x26 (geniş 41,39 22x30); bodrum sw_ne 42,24 16x9, se_nw 38,20 14x9.
- Çağrılar sw_ne'de base64, se_nw ve bodrumda `get_upload_url` yüklemesiyle gönderildi (ilk base64 aktarımı bozuldu; o çağrı üretim yapmadan reddedildi, harcama yok).
- Masaüstü kopyası: yerel (bulutta yapılmadı).

## Dosyalar
Hepsi 102x115; pivot kaynakla aynı: sw_ne (32,19), se_nw (70,19) (alttan).
| Dosya | Renk | Yarı saydam |
|---|---|---|
| iso_wall_window_slit_sw_ne / se_nw | 25 / 26 | 0 |
| iso_wall_window_lattice_sw_ne / se_nw | 26 / 27 | 0 |
| iso_wall_window_wide_sw_ne / se_nw | 28 / 29 | 0 |
| iso_wall_window_leaded_sw_ne / se_nw | 32 / 28 | 0 |
| iso_wall_window_cellar_sw_ne / se_nw | 28 / 33 | 0 |

## Doğrulama
- `validate_iso.py --kind object --max-colors 48`: 10/10 GEÇTİ (dosya başına çağrıldı).
- `style.py check --family yapi`: 10/10 GEÇTİ (palete yakın %99-100, ort. uzaklık 2.3-3.8, doygunluk x0.79-1.08).
- Silüet: bodrum dosyaları kaynakla birebir aynı silüet; diğerlerinde fark yalnızca 008b'nin saydam açıklığının (203 px sw_ne, 117 px se_nw) cam/kafes/kepenkle dolması. Dış hat ve dikiş varsayılan pencereyle aynı; döşeme testi (zemin değil, duvar) ve dikiş parlaklık oranı ölçülmedi.

## Sorunlar / görsel onay notları
- Mazgal se_nw: açıklığın sağında açık renk ek parça var (kepenk/pervaz?). Mazgal sw_ne temiz.
- Geniş sw_ne: çerçeve yatay kirişe taşıyor gibi; kepenkler iki yana taşıyor.
- Bodrum pencereleri küçük (yaklaşık 10x4 px) ve üst kenara yakın; demir çubuklar okunuyor ama ince.
- Hiçbir dosya gözle tam doğrulanmadı; kullanıcı yan yana sayfasına baksın.
- Öneri: yükleme+kırpma için `tools/art`'a yardımcı betik eklenebilir (bu işte `art/tmp330/` altında çözüldü).
