---
id: "362"
title: Kazı destekleri, kuyu inişi, dren çukuru
status: review
claimed_at: 2026-11-18T13:45Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 28 (18.11.2026, "tek seferde yapalım")
priority: 10
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301", "360"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_mine_support_wood_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_support_stone_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_support_rotten_se.png
  - Assets/Art/Iso/_aday/iso_item_drain_pit_se.png
  - Assets/Art/Iso/_aday/iso_item_shaft_ladder_se.png
created: 2026-11-18
---
# 362 Kazı destekleri, kuyu inişi, dren çukuru

## Amaç
Core.Mining.SupportKind (ahşap çatkı, taş kemer, çürüyen çatkı), `item.drain_pit` ve kat -1'den -2'ye inen kuyu (merdiven) görünümü.

## Tarif
Yöntem **301 ile aynı** (`art/queue/301-esya-grup2.md` > Tarif), 1x1, yalnızca SE.
| Dosya | Tuval | Açıklama metni |
|---|---|---|
| `iso_item_mine_support_wood_se.png` | 80x96 | `medieval mine timber support frame: two upright posts and a cross beam, rough pine` |
| `iso_item_mine_support_stone_se.png` | 80x96 | `small dressed stone arch support in a mine gallery` |
| `iso_item_mine_support_rotten_se.png` | 80x96 | `old rotting timber mine support, sagging beam, dark damp wood` |
| `iso_item_drain_pit_se.png` | 80x56 | `small square drainage pit in a dirt floor filled with gravel` |
| `iso_item_shaft_ladder_se.png` | 80x96 | `short wooden ladder going down a square shaft hole in a dirt floor` |

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
