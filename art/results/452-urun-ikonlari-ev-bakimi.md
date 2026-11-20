unity-import: pending

# 452 Ürün ikonları: tüy, kabuk, çakıl (4): sonuç (durum: review)

Yan yana sayfası: `art/results/452-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni 4 görsel).

## Araç ve ayarlar
- `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline`, `high top-down`. 420a dersi (pixen yüz/göz üretiyor) nedeniyle pixen turu atlandı. Açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 4 paralel çağrı x 1 gen = **4 gen** (`cost_est` 6, `cost_max` 10).
- `get_balance` sonrası: **3098,9** (paralel şeritlerin harcaması dahil).
- `snail_shell` 23 renkliydi: 16'ya median-cut (dither yok, alfa korunur) indirildi → 15 renk. Yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Renk | style.py |
|---|---|---|
| icon_item_down_feather | 8 | GEÇTİ |
| icon_item_heron_feather | 5 | UYARI (palete uzak) |
| icon_item_river_pebble | 15 | UYARI (kilit kurulmadı) |
| icon_item_snail_shell | 15 | UYARI (kilit kurulmadı) |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (4/4) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (4/4) |
| KALDI | yok |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

Teslim edilmeyen: yok.

## Sorunlar
1. `snail_shell`: kabukla birlikte salyangozun gövdesi de çizildi (göz/yüz seçilmiyor ama yaratık olarak okunabilir); tarif "kabuk" istiyor. Kullanıcı gözüyle bakmalı; gerekirse yalnızca kabuk olarak yeniden üretim (1 gen).
2. `down_feather` tüy yığınından çok beyaz bir kümeye benziyor, kabul edilebilir.
