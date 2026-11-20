---
id: "720"
title: "Yemek ikonları 2: çorbalar ve etli yemekler (13)"
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
  - Assets/Art/Iso/_aday/icon_item_soup_cabbage.png
  - Assets/Art/Iso/_aday/icon_item_soup_beet.png
  - Assets/Art/Iso/_aday/icon_item_soup_crayfish.png
  - Assets/Art/Iso/_aday/icon_item_soup_tripe.png
  - Assets/Art/Iso/_aday/icon_item_beef_yahni.png
  - Assets/Art/Iso/_aday/icon_item_pork_kavurma.png
  - Assets/Art/Iso/_aday/icon_item_pigeon_stuffed.png
  - Assets/Art/Iso/_aday/icon_item_liver_fry.png
  - Assets/Art/Iso/_aday/icon_item_goat_stew.png
  - Assets/Art/Iso/_aday/icon_item_duck_plum.png
  - Assets/Art/Iso/_aday/icon_item_boar_stew.png
  - Assets/Art/Iso/_aday/icon_item_meatballs.png
  - Assets/Art/Iso/_aday/icon_item_sucuk_egg.png
created: 2026-11-20
---
# 720 Yemek ikonları 2: çorbalar ve etli yemekler (13)

## Amaç
Tam tarif listesinin (Project belgesi 64-yemek) çorbaları ve etli yemekleri. Kimlikler kodda açıldı.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Çiğ et ikonlarında kan ve hayvan parçası (baş, ayak) çizilmez; temiz kesilmiş parça.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): bilinçli istisnalar kahve, siyah çay, domates (yalnızca salça için; karar 60a-E1), avokado.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_soup_cabbage.png` | `clay bowl of pale green cabbage soup` |
| `icon_item_soup_beet.png` | `clay bowl of deep magenta beet soup` |
| `icon_item_soup_crayfish.png` | `clay bowl of orange soup with a red crayfish claw` |
| `icon_item_soup_tripe.png` | `clay bowl of creamy white soup with a garlic clove beside it` |
| `icon_item_beef_yahni.png` | `clay bowl of dark brown beef stew with onion pieces` |
| `icon_item_pork_kavurma.png` | `wooden plate of browned fried pork cubes` |
| `icon_item_pigeon_stuffed.png` | `small golden roast bird stuffed with grain on a wooden plate` |
| `icon_item_liver_fry.png` | `wooden plate of dark fried liver slices with onion rings` |
| `icon_item_goat_stew.png` | `clay bowl of pale boiled meat with root vegetables` |
| `icon_item_duck_plum.png` | `roast duck leg with dark purple plums on a wooden plate` |
| `icon_item_boar_stew.png` | `clay bowl of very dark stew with a bay leaf` |
| `icon_item_meatballs.png` | `wooden plate of round browned meatballs` |
| `icon_item_sucuk_egg.png` | `clay pan with two fried eggs and red sausage slices` |

## Doğrulama
316 ölçütleri; `python tools/art/style.py check --family arayuz <dosya> ...`. Toplu önizleme x4, parşömen ve koyu zeminde (`art/results/720-ikonlar-x4.png`).

## Notlar
`icon_item_<id>` adıyla geldikleri için kod kendiliğinden bulur. Eşya kimlikleri kodda bu adlarla açıldı (58 §8, 58a §2-3, 60a §2.2).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
