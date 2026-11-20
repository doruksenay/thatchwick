---
id: "558"
title: "Tarım ikonları 3/6: ekin ürünü ve tohumu (16)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 35
family: arayuz
style_gate: false
cost_est: 16
cost_max: 32
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_okra.png
  - Assets/Art/Iso/_aday/icon_item_seed_okra.png
  - Assets/Art/Iso/_aday/icon_item_chard.png
  - Assets/Art/Iso/_aday/icon_item_seed_chard.png
  - Assets/Art/Iso/_aday/icon_item_celery.png
  - Assets/Art/Iso/_aday/icon_item_seed_celery.png
  - Assets/Art/Iso/_aday/icon_item_parsley.png
  - Assets/Art/Iso/_aday/icon_item_seed_parsley.png
  - Assets/Art/Iso/_aday/icon_item_rocket.png
  - Assets/Art/Iso/_aday/icon_item_seed_rocket.png
  - Assets/Art/Iso/_aday/icon_item_cauliflower.png
  - Assets/Art/Iso/_aday/icon_item_seed_cauliflower.png
  - Assets/Art/Iso/_aday/icon_item_asparagus.png
  - Assets/Art/Iso/_aday/icon_item_seed_asparagus.png
  - Assets/Art/Iso/_aday/icon_item_artichoke.png
  - Assets/Art/Iso/_aday/icon_item_seed_artichoke.png
created: 2026-11-20
---
# 558 Tarım ikonları 3/6: ekin ürünü ve tohumu (16)

## Amaç
Project belgesi 59 §5.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_okra.png` | `green ridged okra pods` |
| `icon_item_seed_okra.png` | `small cloth seed pouch with okra seeds` |
| `icon_item_chard.png` | `bunch of chard leaves with white stalks` |
| `icon_item_seed_chard.png` | `small cloth seed pouch with chard seeds` |
| `icon_item_celery.png` | `celery root with leafy stalks` |
| `icon_item_seed_celery.png` | `small cloth seed pouch with celery seeds` |
| `icon_item_parsley.png` | `bunch of parsley` |
| `icon_item_seed_parsley.png` | `small cloth seed pouch with parsley seeds` |
| `icon_item_rocket.png` | `bunch of rocket leaves` |
| `icon_item_seed_rocket.png` | `small cloth seed pouch with rocket seeds` |
| `icon_item_cauliflower.png` | `white cauliflower head with leaves` |
| `icon_item_seed_cauliflower.png` | `small cloth seed pouch with cauliflower seeds` |
| `icon_item_asparagus.png` | `bundle of green asparagus spears tied with string` |
| `icon_item_seed_asparagus.png` | `small cloth seed pouch with an asparagus crown` |
| `icon_item_artichoke.png` | `green globe artichoke bud` |
| `icon_item_seed_artichoke.png` | `small cloth seed pouch with artichoke seeds` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
