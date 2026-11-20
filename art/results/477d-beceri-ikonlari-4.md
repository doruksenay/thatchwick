unity-import: pending

# 477d Ürün ikonları: beceri tamamlama 4/4: sonuç (durum: review)

Yan yana sayfası: `art/results/477d-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni 15 görsel).

## Araç ve ayarlar
- `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline`; kap/yiyecek/kitap `high top-down`, araçlar (wheelbarrow, horse_cart, sled, covered_cart) `side` (420a/477a dersi: pixen yüzlü çıktı veriyor). Açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 16 çağrı x 1 gen = **16 gen** (cost_est 16, cost_max 32).
- Hız sınırı: ilk dalgada 5 çağrı "rate limit exceeded" aldı (harcama yok); bir iş bitince bir kez yeniden gönderildi (tarifin tek istisnası).
- Renk indirgeme: 7 ikon (bread_sourdough 19, cough_syrup 22, insect_repellent 27, pomegranate 31, pomegranate_sherbet 17, sleep_syrup 34, wedding_bread 19 renk) median-cut ile 16 renge indirildi (dither yok, alfa 0/255); yeniden boyama yok.
- `get_balance` sonrası: **2521,9** (paylaşılan hesap, paralel şeritler dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
aged_vinegar, bread_sourdough, cough_syrup, covered_cart, dynasty_chronicle, insect_repellent, pomegranate, pomegranate_sherbet, saffron_remedy, sled, sleep_syrup, sourdough, wedding_bread, wheelbarrow, yufka (hepsi `icon_item_<ad>.png`).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (15/15) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (15/15) |
| `style.py check --family arayuz` | KALDI yok; GEÇTİ: bread_sourdough, covered_cart, saffron_remedy, sled, sourdough, wheelbarrow, yufka. UYARI (kilit kurulmadı / palete uzak): aged_vinegar, cough_syrup, dynasty_chronicle, insect_repellent, pomegranate (%42), pomegranate_sherbet, sleep_syrup (%19), wedding_bread |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Teslim edilmeyen
- `item.horse_cart`: çıktı araba değil, tek başına bir at (prompt'ta "no horse" yazılmasına rağmen). Ham hâl `art/results/477d-red/horse_cart_horse.png`. Önerilen: ayrı iş, tarif "a small empty two-wheeled wooden cart with two long shafts, side view, no animal" (1 gen).

## Sorunlar
1. `wedding_bread`: örgülü ekmek yerine tepsideki altın topçuklar gibi okunuyor; kullanıcı gözüyle bakmalı.
2. `saffron_remedy` ve `sourdough` sade kil kaplar; içerik (altın sıvı / köpüklü maya) zor ayırt ediliyor, ikisi birbirine benziyor.
3. `insect_repellent`: kavanoz ağzında mor yumru ve yeşil dal; "otlar ve reçine" okunuyor ama reçine belirsiz.
4. `sleep_syrup` palet uzaklığı yüksek (mavi cam); `pomegranate` doygunluk yüksek. Görsel olarak sorun yok, kilit kurulunca yeniden bakılmalı.
