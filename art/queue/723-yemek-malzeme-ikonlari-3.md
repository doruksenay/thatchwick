---
id: "723"
title: "Yeni malzeme ikonları 3: sucuk, kuru otlar, risaleler (14)"
status: review
claimed_at: 2026-11-20T15:56Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 35
family: arayuz
style_gate: false
cost_est: 14
cost_max: 28
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_sucuk.png
  - Assets/Art/Iso/_aday/icon_item_vine_leaf.png
  - Assets/Art/Iso/_aday/icon_item_gut_string.png
  - Assets/Art/Iso/_aday/icon_item_horn_comb.png
  - Assets/Art/Iso/_aday/icon_item_mint_dried.png
  - Assets/Art/Iso/_aday/icon_item_thyme_dried.png
  - Assets/Art/Iso/_aday/icon_item_dill_dried.png
  - Assets/Art/Iso/_aday/icon_item_sage_dried.png
  - Assets/Art/Iso/_aday/icon_item_lavender_dried.png
  - Assets/Art/Iso/_aday/icon_item_cinnamon.png
  - Assets/Art/Iso/_aday/icon_item_picnic_cloth.png
  - Assets/Art/Iso/_aday/icon_item_treatise_table.png
  - Assets/Art/Iso/_aday/icon_item_treatise_waters.png
  - Assets/Art/Iso/_aday/icon_item_treatise_sweets.png
created: 2026-11-20
---
# 723 Yeni malzeme ikonları 3: sucuk, kuru otlar, risaleler (14)

## Amaç
Parti 2-10 kod oturumunda açılan, önceki işlerde olmayan malzemeler (Project belgesi 64-yemek).

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Çiğ et ikonlarında kan ve hayvan parçası (baş, ayak) çizilmez; temiz kesilmiş parça.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): bilinçli istisnalar kahve, siyah çay, domates (yalnızca salça için; karar 60a-E1), avokado.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_sucuk.png` | `curved dark red dry sausage tied with string` |
| `icon_item_vine_leaf.png` | `small stack of fresh green vine leaves` |
| `icon_item_gut_string.png` | `coil of pale twisted gut string` |
| `icon_item_horn_comb.png` | `carved pale horn comb` |
| `icon_item_mint_dried.png` | `small hanging bunch of dried mint tied with string` |
| `icon_item_thyme_dried.png` | `small hanging bunch of dried thyme tied with string` |
| `icon_item_dill_dried.png` | `small hanging bunch of dried dill tied with string` |
| `icon_item_sage_dried.png` | `small hanging bunch of dried grey-green sage tied with string` |
| `icon_item_lavender_dried.png` | `small hanging bunch of dried purple lavender tied with string` |
| `icon_item_cinnamon.png` | `three brown cinnamon sticks tied together` |
| `icon_item_picnic_cloth.png` | `folded checked linen cloth` |
| `icon_item_treatise_table.png` | `small bound manuscript with a painted dish on the cover` |
| `icon_item_treatise_waters.png` | `small bound manuscript with a painted fish on the cover` |
| `icon_item_treatise_sweets.png` | `small bound manuscript with a painted honeycomb on the cover` |

## Doğrulama
316 ölçütleri; `python tools/art/style.py check --family arayuz <dosya> ...`. Toplu önizleme x4, parşömen ve koyu zeminde (`art/results/723-ikonlar-x4.png`).

## Notlar
`icon_item_<id>` adıyla geldikleri için kod kendiliğinden bulur. Eşya kimlikleri kodda bu adlarla açıldı (58 §8, 58a §2-3, 60a §2.2).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
