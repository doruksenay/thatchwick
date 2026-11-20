unity-import: pending

# 418t Ürün ikonu düzeltmeleri (durum: review)

Yan yana sayfası: [418t-yanyana.png](418t-yanyana.png) (arayuz ailesinde kilit yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 19 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (418b yöntemi), her açıklamanın sonunda "simple still-life object, no face, no eyes, no creature, no person". 19 gen (cost_est 19, cost_max 35).
- Hız sınırı: 4 çağrı (elderflower, cress, nails, animal_salve) 429 aldı, harcama yapmadı; bekleyip bir kez yeniden gönderildi.
- Renk indirgeme: >17 renkli 10 dosya Pillow median-cut ile 16 renge indirildi, dither yok, alfa 0/255. Ham hâller `art/tmp418t/raw/` (commit edilmedi).
- `get_balance` sonrası: **2572,9**.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan)
icon_item_{turnip, cabbage, egg, mushroom, nut, honey, pumpkin, milk, carrot, radish, elderflower, cress, nails, feed, hunting_bow, potion_focus, animal_salve, iron_bar, cezve}_v2.png: 19 dosya, hepsi teslim.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (19/19) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤17 (19/19) |
| Yüz/göz/figür | yok (19/19); eski yüzlü hâllerin hepsi gitti |
| `style.py check --family arayuz` | KALDI yok; GEÇTİ 5 (cezve, hunting_bow, milk, mushroom, nut), UYARI 14 (kilit kurulmadı / palete uzak) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Sorunlar (kullanıcı gözle karar versin)
1. `cezve`: bakır, ama moka demlik silüetine yakın (belden daralan gövde). "Alta doğru genişleyen" şartı zayıf.
2. `iron_bar`: gri-mavi yerine belirgin mor-lavanta tonda ("mor olmamalı" ihlali olabilir).
3. `nails`: 4-6 yerine yalnızca 2 çapraz çivi, sönük mor-gri.
4. `honey`: bez ve ip yok, geniş kase gibi. `animal_salve`: kavanoz yerine kase, kapak yok, yaprak yanında.
5. `pumpkin` su kabağı olarak okunuyor; `mushroom`, `turnip`, `radish`, `carrot` (mor), `egg`, `cabbage`, `milk`, `feed`, `nut`, `potion_focus`, `hunting_bow` beklentiye uygun.
Tek deneme kuralı gereği yeniden üretilmedi; reddedilenler için yeni iş açılabilir.
