---
id: "584"
title: "Yeni malzeme ikonları 2/2: salça, ekşiler, işlenmiş baharatlar (18)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 35
family: arayuz
style_gate: false
cost_est: 18
cost_max: 36
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_bone_broth.png
  - Assets/Art/Iso/_aday/icon_item_tomato.png
  - Assets/Art/Iso/_aday/icon_item_seed_tomato.png
  - Assets/Art/Iso/_aday/icon_item_tomato_paste.png
  - Assets/Art/Iso/_aday/icon_item_pomegranate_molasses.png
  - Assets/Art/Iso/_aday/icon_item_verjuice.png
  - Assets/Art/Iso/_aday/icon_item_sumac.png
  - Assets/Art/Iso/_aday/icon_item_sour_plum_paste.png
  - Assets/Art/Iso/_aday/icon_item_spice_mint.png
  - Assets/Art/Iso/_aday/icon_item_spice_thyme.png
  - Assets/Art/Iso/_aday/icon_item_spice_dill.png
  - Assets/Art/Iso/_aday/icon_item_spice_bay.png
  - Assets/Art/Iso/_aday/icon_item_spice_sage.png
  - Assets/Art/Iso/_aday/icon_item_spice_fenugreek.png
  - Assets/Art/Iso/_aday/icon_item_saffron_threads.png
  - Assets/Art/Iso/_aday/icon_item_garlic_braid.png
  - Assets/Art/Iso/_aday/icon_item_spice_blend.png
  - Assets/Art/Iso/_aday/icon_item_picnic_basket.png
created: 2026-11-20
---
# 584 Yeni malzeme ikonları 2/2: salça, ekşiler, işlenmiş baharatlar (18)

## Amaç
Salça ve dönem ekşileri (58a §3, karar 60a-E1); işlenmiş baharatlar ve aile karışımı (58a §2); piknik sepeti (58 §10).

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Çiğ et ikonlarında kan ve hayvan parçası (baş, ayak) çizilmez; temiz kesilmiş parça.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): bilinçli istisnalar kahve, siyah çay, domates (yalnızca salça için; karar 60a-E1), avokado.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_bone_broth.png` | `small clay pot of clear golden broth with a bone` |
| `icon_item_tomato.png` | `single ripe red tomato with a green stem` |
| `icon_item_seed_tomato.png` | `small cloth seed pouch with a tomato seed drawn on it` |
| `icon_item_tomato_paste.png` | `small clay crock of thick dark red paste with a wooden spoon` |
| `icon_item_pomegranate_molasses.png` | `small dark glass bottle of thick deep red syrup with half a pomegranate` |
| `icon_item_verjuice.png` | `small clay jug with a bunch of unripe green grapes` |
| `icon_item_sumac.png` | `tiny open cloth spice pouch of deep red-purple ground sumac` |
| `icon_item_sour_plum_paste.png` | `rolled sheet of dark sour plum leather` |
| `icon_item_spice_mint.png` | `tiny open cloth spice pouch of dried green mint flakes with a mint leaf` |
| `icon_item_spice_thyme.png` | `tiny open cloth spice pouch of grey-green dried thyme with a sprig` |
| `icon_item_spice_dill.png` | `tiny open cloth spice pouch of dried dill with a feathery frond` |
| `icon_item_spice_bay.png` | `tiny open cloth spice pouch with crushed bay leaves and one whole leaf` |
| `icon_item_spice_sage.png` | `tiny open cloth spice pouch of dried sage with a soft grey leaf` |
| `icon_item_spice_fenugreek.png` | `tiny open cloth spice pouch of yellow-brown ground fenugreek` |
| `icon_item_saffron_threads.png` | `tiny folded paper packet with a pinch of red saffron threads` |
| `icon_item_garlic_braid.png` | `short braid of garlic bulbs` |
| `icon_item_spice_blend.png` | `small tied cloth pouch with mixed coloured spices and a wax seal` |
| `icon_item_picnic_basket.png` | `lidded woven basket with a linen cloth` |

## Doğrulama
316 ölçütleri; `python tools/art/style.py check --family arayuz <dosya> ...`. Toplu önizleme x4, parşömen ve koyu zeminde (`art/results/584-ikonlar-x4.png`).

## Notlar
`icon_item_<id>` adıyla geldikleri için kod kendiliğinden bulur. Eşya kimlikleri kod oturumunda bu adlarla açılır (58 §8, 58a §2-3, 60a §2.2).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
