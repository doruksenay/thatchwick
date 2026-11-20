unity-import: pending

# 477a Ürün ikonları: beceri tamamlama 1/4: sonuç (durum: review)

Yan yana sayfası: `art/results/477a-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni 17 görsel).

## Araç ve ayarlar
- `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline`, `high top-down` (420a dersi: pixen yüzlü çıktı veriyor; baştan map_object). Her açıklamanın sonuna "simple still-life object, no face, no eyes, no creature, no person". 18 çağrı x 1 gen = **18 gen** (cost_est 18, cost_max 36).
- Hız sınırı (10 eşzamanlı) 2 çağrıda görüldü; hatalı çağrılar harcama yapmadı, boşalınca bir kez yeniden gönderildi (tarifin tek istisnası).
- Renk indirgeme: 31/37/27/27/19/18/17 renkli 7 ikon (cocoon, lavender, tar, truffle, seed_vetch, coffee_spent, fish_meal) median-cut ile 15 renge indirildi (dither yok, alfa korundu); yeniden boyama yok.
- `get_balance` sonrası: **2731,9** (paylaşılan hesap, paralel şeritlerin harcaması dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
aged_cheese, bone, bone_meal, cocoon, coffee_spent, dill, fenugreek, fine_lumber, fish_meal, folk_almanac, lavender, mulberry_leaf, seed_herb_bed, seed_vetch, tar, thyme, truffle (hepsi `icon_item_<ad>.png`).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (17/17) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (17/17) |
| `style.py check --family arayuz` | KALDI yok; GEÇTİ: bone_meal, cocoon, coffee_spent, fenugreek, fine_lumber, seed_vetch, tar, truffle. UYARI (kilit kurulmadı / palete uzak): aged_cheese, bone, dill, fish_meal, folk_almanac, lavender, mulberry_leaf, seed_herb_bed, thyme |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Teslim edilmeyen
- `item.saffron_corm`: çıktı soğan değil, sarı lale/çiçek gibi (şekil yanlış). Ham hâl `477a-red/raw_saffron_corm.png`. Önerilen: ayrı iş, tarif "three small round brown corm bulbs with papery skin, no flowers" (1 gen).

## Sorunlar
1. `fish_meal`: çuvalın üstündeki koyu leke göz/şişe boynu gibi okunuyor; kullanıcı gözüyle bakmalı.
2. `cocoon` küçük ve ayrık, koza olarak zor tanınıyor (renk indirgemesi sonrası).
3. `bone` yalnızca 3 renk, sade.
4. Kaynak: tarifteki tek deneme kuralı gereği yeniden üretim yapılmadı.
