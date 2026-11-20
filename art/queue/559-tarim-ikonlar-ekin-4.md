---
id: "559"
title: "Tarım ikonları 4/6: çiçek, baharat tohumu ve boya otları (16)"
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
  - Assets/Art/Iso/_aday/icon_item_rose_petals.png
  - Assets/Art/Iso/_aday/icon_item_seed_rose.png
  - Assets/Art/Iso/_aday/icon_item_calendula.png
  - Assets/Art/Iso/_aday/icon_item_seed_calendula.png
  - Assets/Art/Iso/_aday/icon_item_cumin.png
  - Assets/Art/Iso/_aday/icon_item_seed_cumin.png
  - Assets/Art/Iso/_aday/icon_item_coriander.png
  - Assets/Art/Iso/_aday/icon_item_seed_coriander.png
  - Assets/Art/Iso/_aday/icon_item_anise.png
  - Assets/Art/Iso/_aday/icon_item_seed_anise.png
  - Assets/Art/Iso/_aday/icon_item_nigella.png
  - Assets/Art/Iso/_aday/icon_item_seed_nigella.png
  - Assets/Art/Iso/_aday/icon_item_purslane.png
  - Assets/Art/Iso/_aday/icon_item_dry_pea.png
  - Assets/Art/Iso/_aday/icon_item_coarse_fibre.png
  - Assets/Art/Iso/_aday/icon_item_linseed.png
created: 2026-11-20
---
# 559 Tarım ikonları 4/6: çiçek, baharat tohumu ve boya otları (16)

## Amaç
Project belgeleri 59 §5 ve 58a §2. Baharat ürününün ikonu işlenmiş baharat kesesidir.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_rose_petals.png` | `handful of pink rose petals` |
| `icon_item_seed_rose.png` | `a thorny rose cutting with roots` |
| `icon_item_calendula.png` | `orange calendula flower heads` |
| `icon_item_seed_calendula.png` | `small cloth seed pouch with curled calendula seeds` |
| `icon_item_cumin.png` | `tiny open cloth spice pouch of brown cumin seeds` |
| `icon_item_seed_cumin.png` | `small cloth seed pouch with cumin seeds` |
| `icon_item_coriander.png` | `tiny open cloth spice pouch of round coriander seeds` |
| `icon_item_seed_coriander.png` | `small cloth seed pouch with coriander seeds` |
| `icon_item_anise.png` | `tiny open cloth spice pouch of grey-green anise seeds` |
| `icon_item_seed_anise.png` | `small cloth seed pouch with anise seeds` |
| `icon_item_nigella.png` | `tiny open cloth spice pouch of black nigella seeds` |
| `icon_item_seed_nigella.png` | `small cloth seed pouch with nigella seeds` |
| `icon_item_purslane.png` | `bunch of fleshy purslane` |
| `icon_item_dry_pea.png` | `small pile of dried yellow split peas` |
| `icon_item_coarse_fibre.png` | `rough bundle of coarse bast fibre` |
| `icon_item_linseed.png` | `small pile of glossy brown linseed` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
