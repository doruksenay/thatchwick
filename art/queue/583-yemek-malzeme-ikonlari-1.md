---
id: "583"
title: "Yeni malzeme ikonları 1/2: etler, yumurta, süt, mezbaha yan ürünleri (19)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 35
family: arayuz
style_gate: false
cost_est: 19
cost_max: 38
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_beef.png
  - Assets/Art/Iso/_aday/icon_item_lamb.png
  - Assets/Art/Iso/_aday/icon_item_goat_meat.png
  - Assets/Art/Iso/_aday/icon_item_pork.png
  - Assets/Art/Iso/_aday/icon_item_rabbit_meat.png
  - Assets/Art/Iso/_aday/icon_item_chicken_meat.png
  - Assets/Art/Iso/_aday/icon_item_duck_meat.png
  - Assets/Art/Iso/_aday/icon_item_goose_meat.png
  - Assets/Art/Iso/_aday/icon_item_pigeon_meat.png
  - Assets/Art/Iso/_aday/icon_item_venison.png
  - Assets/Art/Iso/_aday/icon_item_boar_meat.png
  - Assets/Art/Iso/_aday/icon_item_duck_egg.png
  - Assets/Art/Iso/_aday/icon_item_goose_egg.png
  - Assets/Art/Iso/_aday/icon_item_goat_milk.png
  - Assets/Art/Iso/_aday/icon_item_goat_cheese.png
  - Assets/Art/Iso/_aday/icon_item_offal.png
  - Assets/Art/Iso/_aday/icon_item_gut.png
  - Assets/Art/Iso/_aday/icon_item_horn.png
  - Assets/Art/Iso/_aday/icon_item_rennet.png
created: 2026-11-20
---
# 583 Yeni malzeme ikonları 1/2: etler, yumurta, süt, mezbaha yan ürünleri (19)

## Amaç
Et, yumurta ve süt türlere ayrılıyor (58 §8); mezbaha yan ürünleri (60a §2.2).

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Çiğ et ikonlarında kan ve hayvan parçası (baş, ayak) çizilmez; temiz kesilmiş parça.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): bilinçli istisnalar kahve, siyah çay, domates (yalnızca salça için; karar 60a-E1), avokado.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_beef.png` | `thick cut of dark red raw beef with a strip of fat` |
| `icon_item_lamb.png` | `small raw lamb leg cut, pink with a white bone end` |
| `icon_item_goat_meat.png` | `lean dark raw goat meat cut` |
| `icon_item_pork.png` | `raw pork cut with a thick white fat cap` |
| `icon_item_rabbit_meat.png` | `small pale raw rabbit meat portion` |
| `icon_item_chicken_meat.png` | `plucked raw chicken drumstick and thigh` |
| `icon_item_duck_meat.png` | `raw duck breast with yellowish skin` |
| `icon_item_goose_meat.png` | `large raw goose leg with thick skin` |
| `icon_item_pigeon_meat.png` | `very small plucked raw pigeon portion` |
| `icon_item_venison.png` | `deep red lean raw venison cut` |
| `icon_item_boar_meat.png` | `dark raw wild boar cut with coarse fat` |
| `icon_item_duck_egg.png` | `pale blue-green duck egg` |
| `icon_item_goose_egg.png` | `very large white goose egg` |
| `icon_item_goat_milk.png` | `small clay jug of white milk with a goat-horn handle shape` |
| `icon_item_goat_cheese.png` | `small round white goat cheese` |
| `icon_item_offal.png` | `small clay dish with liver slices` |
| `icon_item_gut.png` | `coiled pale sausage casing on a wooden peg` |
| `icon_item_horn.png` | `single curved cow horn` |
| `icon_item_rennet.png` | `small clay pot sealed with cloth and a tied dried pouch beside it` |

## Doğrulama
316 ölçütleri; `python tools/art/style.py check --family arayuz <dosya> ...`. Toplu önizleme x4, parşömen ve koyu zeminde (`art/results/583-ikonlar-x4.png`).

## Notlar
`icon_item_<id>` adıyla geldikleri için kod kendiliğinden bulur. Eşya kimlikleri kod oturumunda bu adlarla açılır (58 §8, 58a §2-3, 60a §2.2).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
