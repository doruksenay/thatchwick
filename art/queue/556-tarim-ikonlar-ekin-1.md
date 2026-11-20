---
id: "556"
title: "Tarım ikonları 1/6: ekin ürünü ve tohumu (16)"
status: running
claimed_at: 2026-11-20T15:23Z
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
  - Assets/Art/Iso/_aday/icon_item_lentil.png
  - Assets/Art/Iso/_aday/icon_item_seed_lentil.png
  - Assets/Art/Iso/_aday/icon_item_chickpea.png
  - Assets/Art/Iso/_aday/icon_item_seed_chickpea.png
  - Assets/Art/Iso/_aday/icon_item_millet.png
  - Assets/Art/Iso/_aday/icon_item_seed_millet.png
  - Assets/Art/Iso/_aday/icon_item_rice.png
  - Assets/Art/Iso/_aday/icon_item_seed_rice.png
  - Assets/Art/Iso/_aday/icon_item_poppy.png
  - Assets/Art/Iso/_aday/icon_item_seed_poppy.png
  - Assets/Art/Iso/_aday/icon_item_cotton.png
  - Assets/Art/Iso/_aday/icon_item_seed_cotton.png
  - Assets/Art/Iso/_aday/icon_item_melon.png
  - Assets/Art/Iso/_aday/icon_item_seed_melon.png
  - Assets/Art/Iso/_aday/icon_item_watermelon.png
  - Assets/Art/Iso/_aday/icon_item_seed_watermelon.png
created: 2026-11-20
---
# 556 Tarım ikonları 1/6: ekin ürünü ve tohumu (16)

## Amaç
Project belgesi 59 §5: yeni ekinlerin ürün ve tohum ikonları.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_lentil.png` | `small pile of brown-green lentils` |
| `icon_item_seed_lentil.png` | `small cloth seed pouch with lentil seeds` |
| `icon_item_chickpea.png` | `small pile of beige chickpeas` |
| `icon_item_seed_chickpea.png` | `small cloth seed pouch with chickpeas` |
| `icon_item_millet.png` | `golden millet seed head` |
| `icon_item_seed_millet.png` | `small cloth seed pouch with millet seeds` |
| `icon_item_rice.png` | `small pile of unhusked rice grains` |
| `icon_item_seed_rice.png` | `small cloth seed pouch with rice seeds` |
| `icon_item_poppy.png` | `dry poppy seed capsule with seeds` |
| `icon_item_seed_poppy.png` | `small cloth seed pouch with poppy seeds` |
| `icon_item_cotton.png` | `open cotton boll with white fluff` |
| `icon_item_seed_cotton.png` | `small cloth seed pouch with cotton seeds` |
| `icon_item_melon.png` | `netted yellow melon` |
| `icon_item_seed_melon.png` | `small cloth seed pouch with melon seeds` |
| `icon_item_watermelon.png` | `dark green striped watermelon` |
| `icon_item_seed_watermelon.png` | `small cloth seed pouch with watermelon seeds` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
