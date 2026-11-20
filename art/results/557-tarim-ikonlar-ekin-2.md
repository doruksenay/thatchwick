unity-import: pending

# 557 Tarım ikonları 2/6: sonuç (durum: review, 14/16 teslim)

Yan yana sayfası: `art/results/557-yanyana.png` (arayuz ailesinde referans yok, yalnızca yeni görseller). Teslim edilmeyen 2 ham dosya: `art/results/557-red/` (yüzlü pixen ham hâlleri de orada).

## Araç ve ayarlar
- **Tur 1:** `create_image_pixen` 32x32, `no_background`, `single color outline`, `low detail`, ikon başına 1 çağrı, açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 16 çağrı = 16 gen. 6 çağrı ilk gönderimde hız sınırına (10/10 eşzamanlı) takıldı, ücret yok; tarifin istisnasıyla bekleyip bir kez yeniden gönderildi.
- pixen 7 keseyi/ikonu yüzlü üretti (cucumber, seed_cucumber, seed_buckwheat, seed_cowpea, seed_hops, seed_sesame, seed_safflower, seed_eggplant; 8 adet). Tarifteki yedek yöntemle bu 8 ikon `create_map_object` basic 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` ile yeniden üretildi: 8 çağrı = 8 gen (ilk 6 çağrı ve 2 ek). Teslim edilenlerde yüz yok (göz kontrolü).
- Toplam **24 gen** (`cost_est` 16, `cost_max` 32: sınır içinde). `get_balance` sonrası: **1751,7** (paralel şeritler dahil).
- Renk indirgeme: ≤16 renge median-cut (dither yok, alfa 0/255); yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
| Dosya | Kaynak | Stil denetimi |
|---|---|---|
| icon_item_hops | pixen | UYARI (palete uzak) |
| icon_item_seed_hops | map_object | GEÇTİ |
| icon_item_cowpea | pixen | UYARI |
| icon_item_seed_cowpea | map_object | GEÇTİ |
| icon_item_buckwheat | pixen | GEÇTİ |
| icon_item_seed_buckwheat | map_object | GEÇTİ |
| icon_item_sesame | pixen | GEÇTİ |
| icon_item_seed_sesame | map_object | GEÇTİ |
| icon_item_safflower | pixen | GEÇTİ |
| icon_item_seed_safflower | map_object | UYARI |
| icon_item_seed_mustard | pixen | GEÇTİ |
| icon_item_eggplant | pixen | UYARI |
| icon_item_seed_eggplant | map_object | GEÇTİ |
| icon_item_seed_cucumber | map_object | UYARI |

## Teslim edilmeyenler
- `icon_item_cucumber`: faceless map_object sürümü KALDI (%30 palete yakın, aşırı doygun limon yeşili); pixen sürümü yüzlüydü. `fit` arayuz ailesinde referans olmadığı için çalışmadı. Tek deneme kuralı gereği yeniden üretilmedi.
- `icon_item_mustard`: pixen, KALDI (%50 palete yakın; turuncu toplar, hardal tanesinden çok portakal gibi). Ham hâl `557-red/`.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (14/14) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 |
| `style.py check --family arayuz` | 8 GEÇTİ, 6 UYARI, 2 KALDI (teslim edilmedi) |
| Yüz/göz | teslim edilenlerde yok |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Sorunlar
1. 2 ikon eksik (cucumber, mustard): ayrı iş ya da arayuz referans kilidi sonrası `fit` (2-3 gen).
2. `icon_item_safflower` kurutulmuş taç yaprak yerine çiçek biçiminde; `seed_cowpea` kesenin içinde beyaz bir nesne var; kullanıcı yan yana sayfasına bakmalı. Tohum keseleri 316 ailesiyle tam aynı biçimde değil (map_object perspektifi).
3. `tools/art/` değiştirilmedi; geçici dosyalar `art/tmp557/` altında.
