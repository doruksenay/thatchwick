unity-import: pending

# 556 Tarım ikonları 1/6: sonuç (durum: review, 13/16 teslim)

Yan yana sayfası: `art/results/556-yanyana.png` (arayuz ailesinde referans yok, yalnızca yeni görseller). Teslim edilmeyen 3 ham dosya: `art/results/556-red/`.

## Araç ve ayarlar
- **Tur 1:** `create_image_pixen` 32x32, `no_background`, `single color outline`, `low detail`, ikon başına 1 çağrı (316 yöntemi), 16 çağrı = 16 gen. 6 çağrı ilk gönderimde ve 1 çağrı sonra hız sınırına (10/10 eşzamanlı, paylaşılan hesap) takıldı, ücret yok; tarifin istisnasıyla bekleyip yeniden gönderildi.
- pixen 8 ikonu yüzlü ya da yaratık biçiminde üretti (chickpea, rice, cotton, seed_cotton, seed_lentil, seed_poppy, watermelon; seed_chickpea şüpheli) ve melon, millet, poppy `style.py check`te KALDI. Tarifteki yedek yöntemle bu 11 ikon `create_map_object` basic 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline`, sonuna "simple still-life object, no face..." eki ile yeniden üretildi: 11 çağrı x 1 gen = 11 gen (418c/418t'de doğrulanmış 1 gen/çağrı).
- Toplam **27 gen** (`cost_est` 16, `cost_max` 32: sınırda, 2 katı aşılmadı). `get_balance` sonrası: **1793,7** (paralel şeritler dahil).
- Renk indirgeme: ham çıktılar 16 renge median-cut (dither yok, alfa 0/255) indirildi, yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
| Dosya | Kaynak | Stil denetimi |
|---|---|---|
| icon_item_lentil | pixen | GEÇTİ |
| icon_item_seed_lentil | map_object | GEÇTİ |
| icon_item_chickpea | map_object | GEÇTİ |
| icon_item_seed_chickpea | map_object | GEÇTİ |
| icon_item_millet | map_object | UYARI (palete uzak) |
| icon_item_seed_millet | pixen | GEÇTİ |
| icon_item_rice | map_object | GEÇTİ |
| icon_item_seed_rice | pixen | GEÇTİ |
| icon_item_cotton | map_object | GEÇTİ |
| icon_item_seed_cotton | map_object | GEÇTİ |
| icon_item_seed_melon | pixen | GEÇTİ |
| icon_item_watermelon | map_object | UYARI (palete uzak) |
| icon_item_seed_watermelon | pixen | GEÇTİ |

## Teslim edilmeyenler (KALDI, palet dışı renkler)
- `icon_item_melon` (%66 palete yakın), `icon_item_poppy` (%69), `icon_item_seed_poppy` (%65). Şekilleri tanınıyor (file/kıvrımlı sarı kavun, pembe haşhaş kapsülü, kırmızı kese) ama renkleri aileden uzak. Neden doygunluk değil, `fit` arayuz ailesinde referans olmadığı için çalışmadı. İlk tur pixen sürümleri de yüzlü ya da KALDI idi (melon, poppy KALDI; seed_poppy yüzlü). Ham hâller `art/results/556-red/`.
- Tek deneme kuralı gereği yeniden üretilmedi. Öneri: ayrı iş (3 gen), ya da arayuz ailesine referans kilidi kurulunca `fit`.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (13/13) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 |
| `style.py check --family arayuz` | 11 GEÇTİ, 2 UYARI (millet, watermelon), KALDI yok (teslim edilenlerde) |
| Yüz/göz | Teslim edilenlerde yok (yan yana sayfasında göz kontrolü) |
| `validate_iso.py` | uygulanmadı (UI ikonu, izometrik değil) |

## Sorunlar
1. 3 ikon eksik (yukarıda). 2. UI dosyaları `Assets/Art/Iso/_aday/` altında; `.meta` yazılmadı.
3. seed_poppy ve seed_millet gibi keseler 316 kese ailesiyle biçim olarak tam aynı değil (map_object keseleri farklı perspektif); kullanıcı yan yana sayfasına bakmalı.
