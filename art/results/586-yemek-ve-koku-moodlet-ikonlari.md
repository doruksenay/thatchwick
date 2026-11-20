unity-import: pending

# 586 Yemek, koku ve boş kalma moodlet ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/586-yanyana.png` (arayuz ailesinde referans yok; 0 referans + 14 yeni `_32`). Toplu önizleme x4 (alfabetik; sol 16x16, sağ `_32`): `art/results/586-ikonlar-x4.png` (parşömen), `art/results/586-ikonlar-x4-koyu.png`. Önizlemede reddedilen 4 ikon da var.

## Araç ve ayarlar
- 212/497 yöntemi: `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, ikon başına 1 çağrı; 18 çağrı = **18 gen** (cost_est 18). Hız sınırına takılmadı; çağrılar iki dalga (10 + 8) gönderildi.
- 32x32 ham: alfa ikili, 12 renge (Pillow mediancut, dither yok). 16x16: en yakın komşu indirme, alfa ikili.
- `get_balance` sonrası: 1149,6 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Teslim edilen (14 ikon; `Assets/Art/Iso/_aday/`, ikon: pivot x merkez, pivot y 0 alttan)
ui_icon_moodlet_{astonished,ate_together,bored,burnt_smell,cat_ate_it,embarrassed,family_warmth,hard_to_swallow,idle_too_long,laughed_a_lot,me_time,mouth_watering,picnic,taking_it_easy}.png (16x16) ve `_32.png` (32x32).

## Teslim edilmeyen (stil denetimi KALDI, `art/results/586-red/`)
| İkon | Neden |
|---|---|
| good_smell | KALDI (palet dışı, %78); `fit` arayuz ailesinde referans olmadığı için çalışmadı |
| mothers_cooking | 16x16 KALDI (%80); `fit` çalışmadı |
| just_us_two | KALDI (%70); şekil de tarife uymuyor (iki fincan yok, leke) |
| compliment_to_cook | KALDI (%46); şekil yanlış (mavi yaratık + büyüteç; kepçe yok) |
Bu 4 ikon yeni bir işle (tek ikon, farklı tarif) yeniden üretilmeli.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 16x16 ve 32x32 |
| Yarı saydam | 0 |
| Renk | 16x16: 8-12, `_32`: 10-12 (≤12) |
| `validate_iso.py` | ikonlara uygulanmaz, atlandı |
| Stil denetimi (teslim edilen 28 dosya) | KALDI 0; GEÇTİ 15, UYARI (palete uzak) 13: bored, cat_ate_it, family_warmth, hard_to_swallow, idle_too_long, laughed_a_lot (16 ve 32) |
| good_smell/burnt_smell, bored/idle_too_long ayrımı | good_smell teslim edilmedi; bored (kum saati) ile idle_too_long (solmuş çiçek) şekilce açıkça ayrı |

## Sorunlar
1. 4/18 ikon reddedildi (yukarıda).
2. Birkaç ikon yüz/kafa olarak okunuyor (embarrassed, astonished, hard_to_swallow); tarifteki "kaş/ter damlası" 16'da zor seçiliyor, `_32` önerilir.
3. `me_time` ve `picnic` makul; `cat_ate_it` balık çok açık seçilmiyor, pati izi küçük.
4. Kabul sonrası `Assets/Resources/UI/Icons/` kopyası kullanıcı kararından sonra.
