---
id: "557"
title: "Tarım ikonları 2/6: ekin ürünü ve tohumu (16)"
status: review
claimed_at: 2026-11-20T15:28Z
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
  - Assets/Art/Iso/_aday/icon_item_cucumber.png
  - Assets/Art/Iso/_aday/icon_item_seed_cucumber.png
  - Assets/Art/Iso/_aday/icon_item_hops.png
  - Assets/Art/Iso/_aday/icon_item_seed_hops.png
  - Assets/Art/Iso/_aday/icon_item_cowpea.png
  - Assets/Art/Iso/_aday/icon_item_seed_cowpea.png
  - Assets/Art/Iso/_aday/icon_item_buckwheat.png
  - Assets/Art/Iso/_aday/icon_item_seed_buckwheat.png
  - Assets/Art/Iso/_aday/icon_item_sesame.png
  - Assets/Art/Iso/_aday/icon_item_seed_sesame.png
  - Assets/Art/Iso/_aday/icon_item_safflower.png
  - Assets/Art/Iso/_aday/icon_item_seed_safflower.png
  - Assets/Art/Iso/_aday/icon_item_mustard.png
  - Assets/Art/Iso/_aday/icon_item_seed_mustard.png
  - Assets/Art/Iso/_aday/icon_item_eggplant.png
  - Assets/Art/Iso/_aday/icon_item_seed_eggplant.png
created: 2026-11-20
---
# 557 Tarım ikonları 2/6: ekin ürünü ve tohumu (16)

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
| `icon_item_cucumber.png` | `short green cucumber` |
| `icon_item_seed_cucumber.png` | `small cloth seed pouch with cucumber seeds` |
| `icon_item_hops.png` | `pale green hop cones` |
| `icon_item_seed_hops.png` | `small cloth seed pouch with a hop rhizome cutting` |
| `icon_item_cowpea.png` | `long green cowpea pods` |
| `icon_item_seed_cowpea.png` | `small cloth seed pouch with cowpeas` |
| `icon_item_buckwheat.png` | `small pile of dark triangular buckwheat groats` |
| `icon_item_seed_buckwheat.png` | `small cloth seed pouch with buckwheat seeds` |
| `icon_item_sesame.png` | `small pile of pale sesame seeds in a tiny bowl` |
| `icon_item_seed_sesame.png` | `small cloth seed pouch with sesame seeds` |
| `icon_item_safflower.png` | `dried orange safflower petals` |
| `icon_item_seed_safflower.png` | `small cloth seed pouch with safflower seeds` |
| `icon_item_mustard.png` | `small pile of yellow-brown mustard seeds` |
| `icon_item_seed_mustard.png` | `small cloth seed pouch with mustard seeds` |
| `icon_item_eggplant.png` | `glossy dark purple elongated eggplant` |
| `icon_item_seed_eggplant.png` | `small cloth seed pouch with eggplant seeds` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
