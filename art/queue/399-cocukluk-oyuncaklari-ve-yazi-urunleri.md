---
id: "399"
title: Çocukluk oyuncakları (7) ve yazı/mum/malt ürün ikonları (6)
status: accepted
claimed_at: 2026-11-18T14:32Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgeleri 30 ve 34 (18.11.2026, "hepsini onaylıyorum")
priority: 20
family: esya
style_gate: false
cost_est: 13
cost_max: 18
depends_on: ["301", "316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_slingshot_se.png
  - Assets/Art/Iso/_aday/iso_item_jump_rope_se.png
  - Assets/Art/Iso/_aday/iso_item_hoop_se.png
  - Assets/Art/Iso/_aday/iso_item_whittling_knife_se.png
  - Assets/Art/Iso/_aday/iso_item_marbles_se.png
  - Assets/Art/Iso/_aday/iso_item_kite_se.png
  - Assets/Art/Iso/_aday/iso_item_juggling_balls_se.png
  - Assets/Art/Icons/icon_tallow.png
  - Assets/Art/Icons/icon_parchment.png
  - Assets/Art/Icons/icon_oak_gall.png
  - Assets/Art/Icons/icon_ink.png
  - Assets/Art/Icons/icon_blank_book.png
  - Assets/Art/Icons/icon_malt.png
created: 2026-11-18
---
# 399 Çocukluk oyuncakları (7) ve yazı/mum/malt ürün ikonları (6)

## Amaç
Belge 30 (çocukluk eşyaları ve alışkanlık sistemi) ve 34 (eksik zincirler: ışık, içki, kağıt ve yazı). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Oyuncaklar çocuk odasında ya da dışarıda durur; ustalaşılınca karakterin kişisel eşyası olur.

## Tarif
### Oyuncaklar (yöntem, ayarlar, teslim 301 ile aynı; takım Köylü: kaba açık çam, kil, ip, ham keten)
| # | items.json id | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.slingshot` | `iso_item_slingshot_se.png` | yuva K | 16x20 | `forked wooden slingshot with a leather pouch` |
| 2 | `item.jump_rope` | `iso_item_jump_rope_se.png` | yuva K | 20x12 | `coiled hemp skipping rope with wooden handles` |
| 3 | `item.hoop` | `iso_item_hoop_se.png` | zemin 1x1 (dış) | 32x32 | `bent wooden hoop leaning with a short driving stick` |
| 4 | `item.whittling_knife` | `iso_item_whittling_knife_se.png` | yuva K | 20x12 | `small iron whittling knife beside a half-carved wooden stick and shavings` |
| 5 | `item.marbles` | `iso_item_marbles_se.png` | yuva K | 16x12 | `small cloth pouch with a few fired clay marbles` |
| 6 | `item.kite` | `iso_item_kite_se.png` | zemin 1x1 (dış) | 32x40 | `diamond linen kite on a wooden cross frame with a rag tail, propped on the ground` |
| 7 | `item.juggling_balls` | `iso_item_juggling_balls_se.png` | yuva K | 16x12 | `three small stitched leather juggling balls` |

### Ürün ikonları (yöntem, tuval ve palet 316 ile aynı; tek nesne, 32x32)
- `icon_tallow.png`: beyazımsı iç yağı topağı, küçük toprak kapta
- `icon_parchment.png`: kenarları düzensiz, açık krem parşömen tabakası, hafif kıvrık
- `icon_oak_gall.png`: üç yuvarlak kahverengi meşe mazısı
- `icon_ink.png`: küçük kil hokka, koyu kahve-siyah mürekkep, yanında kaz tüyü kalem
- `icon_blank_book.png`: deri kaplı, iple bağlı kapalı defter
- `icon_malt.png`: kısa filizli, kurutulmuş arpa taneleri küçük yığın

## Doğrulama
`python tools/art/validate_iso.py <oyuncak dosyaları>`, `python tools/art/style.py check --family esya <oyuncak dosyaları>`, ikonlar için `--family arayuz`. Dönem kuralları (CLAUDE.md, Project belgesi 20): kauçuk top, cam bilye, kâğıt uçurtma yok (kil bilye, keten uçurtma, deri top).

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
Karar (kullanıcı, 18.11.2026): accepted (kısmi). Kabul: sapan, atlama ipi, uçurtma, parşömen.
Yeniden yapılacak (iş 401): çember (sopa yok), bilye (bilye seçilmiyor), boş defter (sandık gibi, renk uyumsuz)
ve teslim edilmeyen oyma bıçağı, jonglör topları, iç yağı, mazı, mürekkep, malt.
