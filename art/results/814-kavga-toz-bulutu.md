unity-import: pending

# 814 Kavga toz bulutu efekti ve parçaları

Durum: `review`. Ter damlası teslim edilmedi (stil denetimi KALDI).

## Araç ve ayarlar
- Bulut: `create_map_object` (basic, 128x96, high top-down, single color outline, medium shading) + `animate_object` v3, 8 kare, `keep_first_frame=false` (tam 8 kare). Aracın init/referans görseli seçeneği olmadığından tarifteki "kare 00 + varyasyon" yerine nesne animasyonu kullanıldı (tek nesne, 2 gen, tutarlı).
- Parçalar: `create_map_object` basic, 32x32 (aracın alt sınırı; tarifteki 24x24 ve 16x16 yapılamaz, yeniden boyutlandırma yapılmadı).
- Üretim sayısı: 8 `create_map_object` + 1 `animate_object`. Harcama kesin ölçülemedi (hesap 5 işçiyle paylaşıldı, bakiye 9615 → 9587 arası), tahmin ≤ 24; `cost_max` 40 aşılmadı.
- Kalan (`get_balance`): 9587 / 10000.
- Doygunluk düzeltmesi (`style.py fit`): bulut 8 kare (x0.76-0.88), yumruk (x0.76), yıldız (x0.63), toz (x0.64). Ham hâller `art/tmp814/` altında kaldı, commit edilmedi.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/fx/`)
Pivot: bulut için alt orta (x 64, y alttan 14, taban satırı hep y=81 sabit); parçalar merkez (x 16, y 16).

| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_fx_brawl_cloud_00..07.png | 128x96 | 64 | 14 |
| iso_fx_brawl_bit_fist.png | 32x32 | 16 | 16 |
| iso_fx_brawl_bit_boot.png | 32x32 | 16 | 16 |
| iso_fx_brawl_bit_star.png | 32x32 | 16 | 16 |
| iso_fx_brawl_bit_dust.png | 32x32 | 16 | 16 |
| iso_fx_brawl_bit_hat.png | 32x32 | 16 | 16 |
| iso_fx_anger_puff.png | 32x32 | 16 | 16 |

GIF: `art/results/814-bulut.gif`. Yan yana sayfası: `art/results/814-yanyana.png`.

## Stil denetimi (esya, teslim edilen hâller)
| Dosya | Sonuç |
|---|---|
| cloud_00..07 (fit) | GEÇTİ (x1.14-1.16) |
| bit_dust (fit) | GEÇTİ |
| bit_fist (fit) | UYARI (palete uzak) |
| bit_star (fit) | UYARI (palete uzak) |
| bit_boot | UYARI (ailesinden doygun x1.26) |
| bit_hat | GEÇTİ |
| anger_puff | UYARI (ailesinden soluk x0.41) |
| sweat_drop | KALDI (fit sonrası bile palet dışı renkler), teslim edilmedi; `art/results/814-red/` |

`validate_iso.py`: bulut 00 ve yumruk geçti; bulut kareleri arasında taban satırı (y=81) ve renk ailesi aynı.

## Sorunlar / sorular
- **Bulut şekli:** üstü sivri kıvrımlı, "krema/yumuşak dondurma" gibi okunabilir; kabarık yuvarlak bulut isteniyordu. Tek deneme kuralı gereği yeniden üretilmedi. Görsel onayda bakılmalı; olmazsa tarif "yuvarlak, üstü basık" diye sıkılaştırılıp yeni iş açılır.
- **Toz zerresi** düz bir top gibi (şeftali/yumurta); 3 toz kümesi ya da dağınık noktalar daha iyi olurdu.
- **sweat_drop** yeniden üretilmeli (öneri: yeni iş, daha düşük doygunluk "pale blue-grey" tarifiyle) ya da kodla çizilecek 8 renkli basit damla.
- Parça boyutları tarifin 24/16 px'inden büyük (32). Kodda ölçeklenerek kullanılabilir.
- `fx/` klasörü yoktu, oluşturuldu (adlandırma `iso_fx_*`).
