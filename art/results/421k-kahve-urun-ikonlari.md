unity-import: pending

# 421k Kahve zinciri ürün ikonları (10): sonuç (durum: review)

Yan yana sayfası: `art/results/421k-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni 10 görsel).

## Araç ve ayarlar
- `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (419a dersi: pixen yüz üretiyordu, doğrudan map_object). Açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 10 çağrı paralel, çağrı başı 1 gen = **10 gen** (`cost_est` 10, `cost_max` 20).
- Ham çıktılar Pillow ile ≤16 renge median-cut (dither yok, alfa 0/255); yeniden boyama yok. Ham hâller commit edilmedi.
- `get_balance` sonrası: 3695,5.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Renk | style.py |
|---|---|---|
| icon_item_sapling_coffee | 7 | GEÇTİ |
| icon_item_coffee_cherry | 15 | UYARI (palet dışı) |
| icon_item_coffee_dried | 13 | GEÇTİ |
| icon_item_coffee_green | 15 | UYARI (palet dışı) |
| icon_item_coffee_roasted | 14 | GEÇTİ |
| icon_item_coffee_roasted_fine | 7 | GEÇTİ |
| icon_item_coffee_ground | 15 | GEÇTİ |
| icon_item_coffee_ground_fine | 15 | UYARI (palete uzak) |
| icon_item_coffee | 15 | GEÇTİ |
| icon_item_cezve | 7 | UYARI (palete uzak; mor-gri metal) |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (10/10) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (10/10) |
| Yüz/göz/figür | yok (göz kontrolü, 10/10) |
| `style.py check --family arayuz` | KALDI yok; GEÇTİ 6, UYARI 4 (kilit kurulmadı) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Sorunlar
- Cezve morumsu gri; sıcak bakır/pirinç tonu istenirse yeniden üretim gerekir. Fincan tabaklı geldi (tarif yalnızca fincan). `coffee_cherry` çok doygun kırmızı.
