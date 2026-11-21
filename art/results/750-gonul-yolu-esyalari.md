unity-import: pending

# 750 Gönül yolu eşyaları (rapor): 10/11 teslim, review

Yan yana sayfası: [750-yanyana.png](750-yanyana.png) (7 esya referansı + 11 yeni; mavi mendil de sayfada, teslim edilmedi). Teslim edilmeyen: `art/results/750-red/icon_item_handkerchief_blue.png`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- İkonlar (6): `create_image_pixen`, 32x32, `no_background`, `single color outline`, `low detail`, açıklamaya "simple still-life object, no face, no eyes" eklendi (591t ile aynı yöntem). Renk indirgeme uygulanmadı (ham çıktı).
- Dünya eşyaları (5): `create_map_object`, basic mod (732'deki masa üstü eşyalarla aynı; kılavuz elmas 64 px olduğundan 32 px tuvale sığmaz; davul 64x64 için de aynı basic mod, kılavuz/maske kullanılmadı). `high top-down`, `low detail`, `basic shading`, `single color outline`, 732'deki ortak ek.
- 11 çağrı x 1 gen = **11 generation** (cost_est 22, cost_max 40 içinde; hız sınırı hatası yok). `get_balance` sonrası: 9565 (paralel şeritlerin harcaması dahil).
- Max 3 eşzamanlı çağrı kuralına uyuldu. Tek deneme, yeniden üretim yok.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| icon_item_handkerchief_red.png | 32x32 | 16 | 0 |
| icon_item_handkerchief_white.png | 32x32 | 16 | 0 |
| icon_item_ring_silver.png | 32x32 | 16 | 0 |
| icon_item_ring_gold.png | 32x32 | 16 | 0 |
| icon_item_ribbon_red.png | 32x32 | 16 | 0 |
| iso_item_henna_bowl_se.png | 32x32 | 16 | 7 |
| iso_item_davul_se.png | 64x64 | 32 | 20 |
| iso_item_zurna_se.png | 32x32 | 16 | 7 |
| iso_item_clay_jug_small_se.png | 32x32 | 16 | 7 |
| iso_item_coffee_tray_se.png | 32x32 | 16 | 7 |

## Doğrulama
| Dosya | Renk | Yarı saydam | validate_iso | Stil |
|---|---|---|---|---|
| kına tası | 19 | 0 | geçti (x5-26, y5-25) | GEÇTİ (%96, x1.03) |
| davul | 45 | 0 | geçti (x9-55, y5-58) | GEÇTİ (%100, x0.91) |
| zurna | 13 | 0 | geçti (x2-29, y8-25) | GEÇTİ (%98, x0.99) |
| küçük testi | 18 | 0 | geçti (x7-26, y2-29) | UYARI (ailesinden doygun x1.28) |
| kahve tepsisi | 41 | 0 | geçti (x3-28, y5-27) | GEÇTİ (%100, x1.18) |

İkonlar (`--family arayuz`, referansı yok, yalnızca palet):
| Dosya | Palete yakın | Ort. uzaklık | Sonuç |
|---|---|---|---|
| mendil kırmızı | %99 | 9.3 | GEÇTİ |
| mendil beyaz | %100 | 4.9 | GEÇTİ |
| mendil mavi | %64 | 17.6 | KALDI (palet dışı renkler): TESLİM EDİLMEDİ |
| kurdele kırmızı | %100 | 9.7 | GEÇTİ |
| yüzük altın | %82 | 7.5 | GEÇTİ |
| yüzük gümüş | %85 | 7.2 | GEÇTİ |

## Sorunlar
- Mavi mendil: palet sorunu (doygunluk değil), `fit` arayuz ailesinde referans olmadığı için çalışmadı; max_attempts 1 olduğundan yeniden denenmedi. 1 gen ile yeniden üretim önerilir.
- Masa üstü eşyalarda kılavuz ayak izi doğrulaması yok, ölçek ve görünüm göz kontrolü bekliyor (davul 64x64 tuvalde, ayak izi kılavuzsuz). Gölge pişirilmemiş, ışık gözle sol-üstten.
- `items.json` eşlemesi kod oturumunda; mavi mendil dosyası olmadan o eşya eksik.
