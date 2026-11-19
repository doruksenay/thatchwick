unity-import: pending

# 419a Ürün ikonları B 1/3: sonuç (durum: review)

Yan yana sayfası: `art/results/419a-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni 19 görsel).

## Araç ve ayarlar
- **Tur 1:** `create_image_pixen`, 32x32, `no_background`, `single color outline`, `low detail`, 316 yöntemi; açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 19 çağrı x 1 gen = 19 gen. Hız sınırı (10/10 eşzamanlı iş, paylaşılan hesap) hatası alındı; çağrılar boşalınca yeniden gönderildi (tarifin tek istisnası), hatalı çağrılar harcama yapmadı.
- Göz kontrolü: 10 ikon yine yüz/göz/figürlü geldi (carrot, garlic, onion, parsnip, radish, spinach, seed_beet, seed_broad_bean, seed_lettuce) ya da yanlış nesneydi (seed_onion kavanoz içinde domates). Bunlar teslim edilmedi (`art/results/419a-red/pixen_*.png`).
- **Tur 2 (tarifteki 399 dersi):** bu 10 ikon `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline` ile yeniden üretildi (parsnip `side`, diğerleri `high top-down`). 10 çağrı; 401/417 sonuçlarına göre çağrı başı 1 gen (aracın kendi açıklamasında maliyet yazmıyor; `get_balance` farkı paylaşılan şeritlerden dolayı ölçülemedi). Toplam **29 gen** (`cost_est` 19, `cost_max` 38: sınır içinde).
- **Renk indirgeme:** ham çıktılar 33-57 renk; Pillow ile ≤16 renge median-cut (dither yok, alfa 0/255), yeniden boyama yok. Ham hâller commit edilmedi.
- `get_balance` sonrası: 3860,5 (paralel şeritlerin harcaması dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Yöntem | Renk |
|---|---|---|
| icon_item_parsnip | map_object | 14 |
| icon_item_seed_parsnip | pixen | 16 |
| icon_item_carrot | map_object | 5 |
| icon_item_seed_carrot | pixen | 14 |
| icon_item_onion | map_object | 11 |
| icon_item_seed_onion | map_object | 15 |
| icon_item_garlic | map_object | 15 |
| icon_item_seed_garlic | pixen | 16 |
| icon_item_beet | pixen | 16 |
| icon_item_seed_beet | map_object | 15 |
| icon_item_radish | map_object | 15 |
| icon_item_seed_radish | pixen | 15 |
| icon_item_lettuce | pixen | 13 |
| icon_item_seed_lettuce | map_object | 15 |
| icon_item_broad_bean | pixen | 15 |
| icon_item_seed_broad_bean | map_object | 13 |
| icon_item_barley | pixen | 15 |
| icon_item_seed_barley | pixen | 15 |
| icon_item_spinach | map_object | 15 |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (19/19) |
| Yarı saydam piksel | 0 (19/19) |
| Renk sayısı | ≤16 (19/19) |
| Yüz/göz/figür | teslim edilen 19 ikonda yok (göz kontrolü, sıfır tolerans); yüzlüler red klasöründe |
| `style.py check --family arayuz` | KALDI yok; GEÇTİ 4 (onion, seed_broad_bean, seed_garlic, seed_parsnip), UYARI 15 (kilit kurulmadı / palete uzak: barley, beet, broad_bean, carrot, garlic, lettuce, parsnip, radish, seed_barley, seed_beet, seed_carrot, seed_lettuce, seed_onion, seed_radish, spinach) |
| `validate_iso.py` | uygulanmadı (izometrik değil, UI ikonu) |

## Sorunlar
1. Teslim edilmeyen (yüzlü) pixen hâlleri: 10 adet, `art/results/419a-red/`.
2. Karışık yöntem (9 pixen + 10 map_object): küçük çizgi/gölgeleme farkı olabilir; `carrot` yalnızca 5 renk (düz). Özellikle `seed_*` kese biçimleri aileler arasında birebir aynı değil (seed_beet ve seed_lettuce kese içinden fidan çıkıyor), kullanıcı kararı gerekir.
3. `spinach`, `radish`, `broad_bean`, `lettuce` palete uzak uyarısı aldı (kilit yok), kabul kararı yan yana sayfasına bakılarak verilmeli.
4. Hız sınırı (10/10) yüzünden gönderim birkaç dalgaya bölündü.
