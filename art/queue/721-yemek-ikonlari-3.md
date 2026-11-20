---
id: "721"
title: "Yemek ikonları 3: balık, sebze ve tahıl (14)"
status: review
claimed_at: 2026-11-20T15:49Z
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
  - Assets/Art/Iso/_aday/icon_item_paste_stew.png
  - Assets/Art/Iso/_aday/icon_item_fish_steam.png
  - Assets/Art/Iso/_aday/icon_item_eel_pie.png
  - Assets/Art/Iso/_aday/icon_item_mussel_pilaf.png
  - Assets/Art/Iso/_aday/icon_item_salt_fish_boil.png
  - Assets/Art/Iso/_aday/icon_item_broad_bean_oil.png
  - Assets/Art/Iso/_aday/icon_item_pumpkin_stuffed.png
  - Assets/Art/Iso/_aday/icon_item_radish_salad.png
  - Assets/Art/Iso/_aday/icon_item_cress_salad.png
  - Assets/Art/Iso/_aday/icon_item_cabbage_paste.png
  - Assets/Art/Iso/_aday/icon_item_turnip_mash.png
  - Assets/Art/Iso/_aday/icon_item_porridge_barley.png
  - Assets/Art/Iso/_aday/icon_item_pea_mash.png
  - Assets/Art/Iso/_aday/icon_item_rye_gruel.png
created: 2026-11-20
---
# 721 Yemek ikonları 3: balık, sebze ve tahıl (14)

## Amaç
Tam tarif listesinin balık, sebze ve tahıl yemekleri.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Çiğ et ikonlarında kan ve hayvan parçası (baş, ayak) çizilmez; temiz kesilmiş parça.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): bilinçli istisnalar kahve, siyah çay, domates (yalnızca salça için; karar 60a-E1), avokado.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_paste_stew.png` | `clay bowl of red-brown meat stew` |
| `icon_item_fish_steam.png` | `clay dish with a pale steamed fish and onion rings` |
| `icon_item_eel_pie.png` | `round golden pie with a slice cut out` |
| `icon_item_mussel_pilaf.png` | `bowl of grain pilaf topped with dark mussel shells` |
| `icon_item_salt_fish_boil.png` | `clay bowl of pale fish pieces with root vegetables` |
| `icon_item_broad_bean_oil.png` | `clay dish of green broad beans glistening with oil` |
| `icon_item_pumpkin_stuffed.png` | `hollowed pale green bottle gourd filled with grain` |
| `icon_item_radish_salad.png` | `small wooden bowl of sliced red and white radish` |
| `icon_item_cress_salad.png` | `small wooden bowl of dark green watercress leaves` |
| `icon_item_cabbage_paste.png` | `clay dish of red-tinted stewed cabbage` |
| `icon_item_turnip_mash.png` | `clay bowl of pale mashed turnip with a pat of butter` |
| `icon_item_porridge_barley.png` | `wooden bowl of pale grey barley porridge` |
| `icon_item_pea_mash.png` | `clay bowl of thick yellow-green pease pudding` |
| `icon_item_rye_gruel.png` | `wooden bowl of thin brown gruel` |

## Doğrulama
316 ölçütleri; `python tools/art/style.py check --family arayuz <dosya> ...`. Toplu önizleme x4, parşömen ve koyu zeminde (`art/results/721-ikonlar-x4.png`).

## Notlar
`icon_item_<id>` adıyla geldikleri için kod kendiliğinden bulur. Eşya kimlikleri kodda bu adlarla açıldı (58 §8, 58a §2-3, 60a §2.2).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
