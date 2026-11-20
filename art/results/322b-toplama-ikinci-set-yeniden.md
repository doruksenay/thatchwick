unity-import: pending

# 322b Toplama düğümleri ikinci set yeniden (rapor)

Sonuç: **6/7 teslim, mürver (elderflower) üretilemedi.** Yan yana: [322b-yanyana.png](322b-yanyana.png) (322'nin reddedilen çıktısı için [322-yanyana.png](322-yanyana.png)). Masaüstü kopyası: yerel.

## Yöntem
- Kullanıcı 322'deki sarımsak, ısırgan, çilek, kuşburnu, kozalak ve tereyi iyi bulduğundan bunlar yeniden üretilmedi (0 gen). `_aday/` altındaki 322 dosyaları `_v2` adıyla `cp` ile kopyalandı; içerik aynıdır.
- Mürver: `create_map_object` stil eşleme, 301 yöntemi (referanssız kılavuz + maske, 80x72, `--n 1 --height 30`), high top-down, low detail, basic shading, single color outline. 2 deneme x 1 gen = 2 gen (`max_attempts` 2 doldu).
  1. Deneme: büyük beyaz kala/zambak çiçekleri, sarı göbekli: istenen şemsiye öbek değil.
  2. Deneme (açıklama "kıvrık krem noktalar, yaprak, petal yok" olarak değişti): çiçeksiz, yuvarlak yeşil yapraklı ağaç.
  Ham çıktılar: `art/results/322b-red/elder_try1_raw.png`, `elder_try2_raw.png`. Teslim edilmedi.
- `get_balance` iş sonu: 2671,90. Bu işin payı 2 gen.

## Dosyalar (pivot x=40, y ALTTAN=24)
| Yol | Boyut | Renk | Yarı saydam |
|---|---|---|---|
| iso_forage_wild_garlic_se_v2.png | 80x72 | 48 | 0 |
| iso_forage_nettle_se_v2.png | 80x72 | 44 | 0 |
| iso_forage_strawberry_se_v2.png | 80x72 | 25 | 0 |
| iso_forage_rosehip_se_v2.png | 80x72 | 19 | 0 |
| iso_forage_pinecone_se_v2.png | 80x72 | 25 | 0 |
| iso_forage_cress_se_v2.png | 80x72 | 12 | 0 |

validate_iso: hepsi geçti. Döşeme testi zemin olmadığından uygulanmadı.

## Stil denetimi (bitki; kilit kurulmadı, palet uyarıdır)
| Parça | Sonuç | Palete yakın | Doygunluk |
|---|---|---|---|
| garlic | UYARI | %38 | x1.16 |
| nettle | UYARI | %41 | x1.15 |
| strawberry | UYARI | %100 | x1.15 |
| rosehip | UYARI | %59 | x1.15 |
| pinecone | GEÇTİ | %84 | x1.17 |
| cress | UYARI | %20 | x1.16 |

## Sorular / öneri
- Mürver stil eşlemeyle iki kez tutmadı. Öneri: `create_object_pro_flash` ya da `create_1_direction_object` ile "elderflower umbel" ya da ayrı bir 322c işi; yaban havucu/ağaç yerine "flat cream corymb heads on dark green shrub" için referans görseli.
