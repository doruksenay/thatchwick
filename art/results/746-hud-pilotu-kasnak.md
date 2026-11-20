unity-import: pending

# 746 HUD pilotu, yön C: Kasnak ve kumaş yama

**Sonuç: kısmi. Üç sayfa üretildi ve teslim edildi (`_aday`), ancak parçalar native boyutta kesilemedi; `status: failed`.**

## Çağrılar (tek deneme, harcama 21 gen)
| Çağrı | Araç | Ayar | Gen |
|---|---|---|---|
| Sayfa 1 | create_ui_asset | 256x192 (araç alt sınırı 192; tarifteki 256x128 mümkün değil), max_colors 32, seed 0, arayuz paleti (128) | 10 |
| Sayfa 2 | create_ui_asset | 192x192 (alt sınır 192), aynı ayarlar | 10 |
| Sayfa 3 | create_image_pixflux | 128x64, saydam, seed 0, flat shading, tek renk kontur | 1 |

get_balance sonrası: 756,6 (paralel işçiler de harcıyor; bu işin payı 21).

## Dosyalar
| Yol | Boyut |
|---|---|
| Assets/Art/Iso/_aday/ui_hud2_kasnak_sheet1.png | 256x192 |
| Assets/Art/Iso/_aday/ui_hud2_kasnak_sheet2.png | 192x192 |
| Assets/Art/Iso/_aday/ui_hud2_kasnak_sheet3.png | 128x64 |

Yan yana: `art/results/746-yanyana.png`. Tek tek parça dosyası, 9-slice testi ve ikon x3 sayfası YOK.

## Stil denetimi
| Dosya | Palete yakın | Sonuç |
|---|---|---|
| sheet1 | %100 | GEÇTİ |
| sheet2 | %100 | GEÇTİ |
| sheet3 | %100 (ort. uzaklık 7,6) | GEÇTİ |

## Sorunlar (neden parça kesilmedi)
- **Sayfa 1:** `create_ui_asset` ayrı parçalar değil, tek bir birleşik panel çizdi (kasnak, bant, kapsül, çerçeve bir arada, tek kumaş zemin üstünde). Parçalar arası saydam boşluk yok; P1, P2, P3, P6, P8 temiz kesilemez.
- **Sayfa 2:** Parçalar okunaklı ve güzel (Yapı çekiç+mala, Defter kitap, basılı koyu hâller, bordo ünlem rozeti) ama yaklaşık 2x büyük (düğme ~56 px, rozet ~26 px); tarif ölçek düşürmeyi yasakladığı için 32x32 / 10x10 teslim edilmedi.
- **Sayfa 3:** pixflux ızgarayı uygulamadı: ikonlar çok küçük, birkaç kopya/boş yer tutucu, 15 ikon seçilemiyor; P7, P9, P10 okunaksız.
- Palet içinde saygı: tüm renkler arayuz paletinden.

## Öneri (## Sorular)
- Parça başına `create_ui_asset` (elements ile tek parça, ör. 192x192 en küçük tuval) ya da `create_image_pro` ile native boyuta yakın üretim; ikonlar için `create_image_pixen` (16/20 px tuval, 1 gen) 15 ayrı çağrı olarak (~15 gen). Karar kullanıcıda.
- Masaüstü kopyası: yerel.
