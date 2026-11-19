unity-import: pending

# 420a Ürün ikonları C 1/4: sonuç (durum: review)

Yan yana sayfası: `art/results/420a-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni 15 görsel).

## Araç ve ayarlar
- **Tur 1:** `create_image_pixen`, 32x32, `no_background`, `single color outline`, `low detail` (316 yöntemi); açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 15 çağrı x 1 gen = 15 gen. Hız sınırı (10/10 eşzamanlı iş, paylaşılan hesap) hatası alındı; çağrılar boşalınca yeniden gönderildi (tarifin tek istisnası), hatalı çağrılar harcama yapmadı.
- Göz kontrolünde 8 ikon yüz/göz/yaratıkla geldi (wool, compost, resin, hide, leather, saddle, stone, ice). Teslim edilmedi (`art/results/420a-red/pixen_*.png`).
- **Tur 2 (399 dersi):** bu 8 ikon `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline` ile yeniden üretildi (saddle `side`, diğerleri `high top-down`). 8 çağrı x 1 gen = 8 gen.
- Toplam **23 gen** (`cost_est` 15, `cost_max` 30: sınır içinde). `get_balance` sonrası: **3814,5** (paralel şeritlerin harcaması dahil).
- **Renk indirgeme:** pixen ham çıktıları ≤16 renge değil, 16'ya median-cut ile indirildi (dither yok, alfa 0/255); yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Yöntem | Renk |
|---|---|---|
| icon_item_nails | pixen | 16 |
| icon_item_feed | pixen | 16 |
| icon_item_wool | map_object | 9 |
| icon_item_compost | map_object | 10 |
| icon_item_resin | map_object | 14 |
| icon_item_withy | pixen | 14 |
| icon_item_salt | pixen | 15 |
| icon_item_hide | map_object | 9 |
| icon_item_leather | map_object | 7 |
| icon_item_saddle | map_object | 6 |
| icon_item_saddle_fine | pixen | 16 |
| icon_item_stone | map_object | 11 |
| icon_item_ice | map_object | 15 |
| icon_item_leather_bag | pixen | 15 |
| icon_item_halter | pixen | 15 |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (15/15) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (15/15) |
| `style.py check --family arayuz` | KALDI yok; 9 GEÇTİ (compost, feed, leather, leather_bag, nails, saddle, salt, stone, wool), 6 UYARI (halter, hide, ice, resin, saddle_fine, withy; kilit kurulmadı / palete uzak) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

Teslim edilmeyen: yok (15/15 teslim). Reddedilen ham pixen hâlleri `420a-red/` altında.

## Sorunlar
1. **Şüpheli ikonlar (kullanıcı gözüyle bakmalı):** `nails` yeşilimsi bir yığın gibi, çivi olarak zor tanınıyor; `feed` çuvalında yüz benzeri iki leke var; `saddle_fine` selle okunuyor ama pirinç süs belirsiz. Gerekirse `create_map_object` ile yeniden üretim önerilir (3 gen).
2. `leather` ve `saddle` map_object çıktıları sade (6-7 renk); diğer ikonlara göre daha az dokulu, stil birliği orta.
3. Ham pixen çıktıları yine kawaii eğilimliydi (8/15 yüzlü); sonraki ikon işlerinde baştan `create_map_object` kullanmak pixen turunu atlayıp tasarruf sağlar (yaklaşık 8 gen/iş).
