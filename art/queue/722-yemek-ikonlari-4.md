---
id: "722"
title: "Yemek ikonları 4: hamur işi, tatlı ve azık (13)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 35
family: arayuz
style_gate: false
cost_est: 13
cost_max: 26
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_paste_pilaf.png
  - Assets/Art/Iso/_aday/icon_item_milk_wheat.png
  - Assets/Art/Iso/_aday/icon_item_duck_kaygana.png
  - Assets/Art/Iso/_aday/icon_item_cokelek.png
  - Assets/Art/Iso/_aday/icon_item_honey_milk.png
  - Assets/Art/Iso/_aday/icon_item_chestnut_bread.png
  - Assets/Art/Iso/_aday/icon_item_walnut_bun.png
  - Assets/Art/Iso/_aday/icon_item_honey_nuts.png
  - Assets/Art/Iso/_aday/icon_item_cherry_tart.png
  - Assets/Art/Iso/_aday/icon_item_chestnut_candy.png
  - Assets/Art/Iso/_aday/icon_item_pear_hosaf.png
  - Assets/Art/Iso/_aday/icon_item_olive_bread.png
  - Assets/Art/Iso/_aday/icon_item_dried_fruit_ration.png
created: 2026-11-20
---
# 722 Yemek ikonları 4: hamur işi, tatlı ve azık (13)

## Amaç
Tam tarif listesinin kalan yemekleri.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Çiğ et ikonlarında kan ve hayvan parçası (baş, ayak) çizilmez; temiz kesilmiş parça.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): bilinçli istisnalar kahve, siyah çay, domates (yalnızca salça için; karar 60a-E1), avokado.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_paste_pilaf.png` | `bowl of red-orange bulgur pilaf` |
| `icon_item_milk_wheat.png` | `wooden bowl of creamy white wheat pudding with a drizzle of honey` |
| `icon_item_duck_kaygana.png` | `thick golden pancake folded on a wooden plate` |
| `icon_item_cokelek.png` | `small heap of white crumbly curd on a cloth` |
| `icon_item_honey_milk.png` | `clay cup of warm milk with a golden swirl` |
| `icon_item_chestnut_bread.png` | `round brown loaf with chestnut pieces showing` |
| `icon_item_walnut_bun.png` | `braided golden bun studded with walnut halves` |
| `icon_item_honey_nuts.png` | `small clay bowl of hazelnuts glazed with honey` |
| `icon_item_cherry_tart.png` | `round tart with dark red cherries` |
| `icon_item_chestnut_candy.png` | `small pile of glossy candied chestnuts` |
| `icon_item_pear_hosaf.png` | `clay bowl of stewed pear halves in pale syrup` |
| `icon_item_olive_bread.png` | `slice of bread spread with dark olive paste` |
| `icon_item_dried_fruit_ration.png` | `small cloth bundle of dried fruit and nuts` |

## Doğrulama
316 ölçütleri; `python tools/art/style.py check --family arayuz <dosya> ...`. Toplu önizleme x4, parşömen ve koyu zeminde (`art/results/722-ikonlar-x4.png`).

## Notlar
`icon_item_<id>` adıyla geldikleri için kod kendiliğinden bulur. Eşya kimlikleri kodda bu adlarla açıldı (58 §8, 58a §2-3, 60a §2.2).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
