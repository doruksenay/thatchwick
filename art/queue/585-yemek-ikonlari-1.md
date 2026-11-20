---
id: "585"
title: "Yemek ikonları 1: beceri tablosunun vaat ettikleri ve kesinleşenler (16)"
status: running
claimed_at: 2026-11-20T15:41Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 35
family: arayuz
style_gate: false
cost_est: 16
cost_max: 32
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_fish_fry.png
  - Assets/Art/Iso/_aday/icon_item_venison_roast.png
  - Assets/Art/Iso/_aday/icon_item_kome.png
  - Assets/Art/Iso/_aday/icon_item_sarma_leaf.png
  - Assets/Art/Iso/_aday/icon_item_saffron_pilaf.png
  - Assets/Art/Iso/_aday/icon_item_saffron_bun.png
  - Assets/Art/Iso/_aday/icon_item_gozleme.png
  - Assets/Art/Iso/_aday/icon_item_pestil.png
  - Assets/Art/Iso/_aday/icon_item_soup_tarhana.png
  - Assets/Art/Iso/_aday/icon_item_soup_wild_garlic.png
  - Assets/Art/Iso/_aday/icon_item_lamb_tandir.png
  - Assets/Art/Iso/_aday/icon_item_goose_roast.png
  - Assets/Art/Iso/_aday/icon_item_rabbit_stew.png
  - Assets/Art/Iso/_aday/icon_item_fish_grill.png
  - Assets/Art/Iso/_aday/icon_item_borek_cheese.png
  - Assets/Art/Iso/_aday/icon_item_spinach_saute.png
created: 2026-11-20
---
# 585 Yemek ikonları 1: beceri tablosunun vaat ettikleri ve kesinleşenler (16)

## Amaç
Beceri tablosunda yazıp oyunda olmayan altı yemek (57 §3.5) ve içeriği kesinleşen on yemek (58 §7). Kalan yemek ikonları tam tarif listesi (58b) çıkınca ayrı işlerle gelir.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Çiğ et ikonlarında kan ve hayvan parçası (baş, ayak) çizilmez; temiz kesilmiş parça.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): bilinçli istisnalar kahve, siyah çay, domates (yalnızca salça için; karar 60a-E1), avokado.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_fish_fry.png` | `two golden fried fish on a wooden plate` |
| `icon_item_venison_roast.png` | `sliced roast venison with a thyme sprig on a wooden plate` |
| `icon_item_kome.png` | `string of walnut sweets coated in dark grape must` |
| `icon_item_sarma_leaf.png` | `small pile of dark green stuffed vine leaf rolls` |
| `icon_item_saffron_pilaf.png` | `bowl of bright yellow saffron pilaf` |
| `icon_item_saffron_bun.png` | `round golden yellow festive bun` |
| `icon_item_gozleme.png` | `folded thin flatbread with a browned blistered surface` |
| `icon_item_pestil.png` | `rolled sheet of amber fruit leather` |
| `icon_item_soup_tarhana.png` | `clay bowl of orange-red thick soup` |
| `icon_item_soup_wild_garlic.png` | `clay bowl of bright green soup` |
| `icon_item_lamb_tandir.png` | `tender slow-cooked lamb pieces on a wooden plate` |
| `icon_item_goose_roast.png` | `whole roast goose on a platter` |
| `icon_item_rabbit_stew.png` | `clay bowl of pale stew with carrot pieces` |
| `icon_item_fish_grill.png` | `single grilled fish with dark grill marks` |
| `icon_item_borek_cheese.png` | `golden layered pastry square with white cheese showing` |
| `icon_item_spinach_saute.png` | `clay dish of dark green wilted spinach with an egg on top` |

## Doğrulama
316 ölçütleri; `python tools/art/style.py check --family arayuz <dosya> ...`. Toplu önizleme x4, parşömen ve koyu zeminde (`art/results/585-ikonlar-x4.png`).

## Notlar
`icon_item_<id>` adıyla geldikleri için kod kendiliğinden bulur. Eşya kimlikleri kodda bu adlarla açıldı (58 §8, 58a §2-3, 60a §2.2).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
