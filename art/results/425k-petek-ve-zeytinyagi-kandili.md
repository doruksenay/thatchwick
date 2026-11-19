unity-import: pending

# 425k Petekli bal ikonu ve zeytinyağı kandili (durum: review)

Yan yana sayfası: [425k-yanyana.png](425k-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
4 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (423k yöntemi), paralel; hız sınırı olmadı. 4 çağrı x 1 gen = **4 gen** (cost_est 4, cost_max 8). `get_balance` işten önce 3672,5 (sonrası paralel şeritlerle karıştığı için ayrıca okunmadı).
Kandil iso karelerine "isometric, light from top-left, no baked shadow" eklendi. Tüm açıklamalarda "no face, no eyes, no creature, no person".

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA)
| Yol | Opak kutu | Renk | Pivot x | Pivot y (alttan) |
|---|---|---|---|---|
| icon_item_honeycomb.png | x3-29, y5-27 | 15 | 16 | 0 |
| icon_item_olive_lamp.png | x5-27, y8-25 | 5 | 16 | 0 |
| iso_item_olive_lamp_se.png | x6-25, y7-23 | 13 | 16 | 8 (taban elması merkezi ~) |
| iso_item_olive_lamp_lit_se.png | x4-25, y3-27 | 23 | 16 | 4 |

## Doğrulama
| Dosya | Sonuç |
|---|---|
| iso_item_olive_lamp_se | validate_iso geçti; style check esya UYARI (palete uzak, doygunluk x1,21) |
| iso_item_olive_lamp_lit_se | validate_iso geçti; style check esya --light GEÇTİ (doygunluk x0,73, ışık pikseli %20 denetim dışı) |
| icon_item_honeycomb | 15 opak renk, yarı saydam 0; arayuz UYARI (kilit yok, palete yakın %27) |
| icon_item_olive_lamp | 5 renk; arayuz UYARI (kilit yok, %75) |

## Sorunlar
- Bal peteği ham çıktı 28 renkliydi; Pillow median-cut ile 15'e indirildi (dither yok, alfa korundu). Ham hâl `art/tmp425k/raw` (commit edilmez).
- Kandil ikonu üstten bakışlı, koyu kırmızı-kahve kaplı, kandil gibi okunuyor ama sade; yanık kare 23 renk (ikon sınırı iso için geçerli değil).
- Yanık ile sönük karenin gövde biçimi farklı (iki ayrı üretim); aynı kandil olarak eşleşme orta. Yan yana sayfasında gözle kontrol gerekir.
- Yüz/göz/figür: küçük görsellerde görünmüyor.
