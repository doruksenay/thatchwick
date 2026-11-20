---
id: "562"
title: "Ata tohumu rozetleri, görünür ata çeşitleri ve hasat aletleri (12)"
status: running
claimed_at: 2026-11-20T14:54Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 30
family: arayuz
style_gate: false
cost_est: 12
cost_max: 24
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_heirloom_seed.png
  - Assets/Art/Iso/_aday/ui_icon_heirloom_seed_gold.png
  - Assets/Art/Iso/_aday/icon_item_carrot_purple.png
  - Assets/Art/Iso/_aday/icon_item_carrot_yellow.png
  - Assets/Art/Iso/_aday/icon_item_radish_black.png
  - Assets/Art/Iso/_aday/icon_item_cabbage_red.png
  - Assets/Art/Iso/_aday/ui_icon_tool_sickle.png
  - Assets/Art/Iso/_aday/ui_icon_tool_scythe.png
  - Assets/Art/Iso/_aday/icon_item_harvest_doll.png
  - Assets/Art/Iso/_aday/ui_icon_field_ripe.png
  - Assets/Art/Iso/_aday/ui_icon_field_mix_risk.png
  - Assets/Art/Iso/_aday/ui_icon_field_thirsty.png
created: 2026-11-20
---
# 562 Ata tohumu rozetleri, görünür ata çeşitleri ve hasat aletleri (12)

## Amaç
Project belgeleri 59 §4 ve 59a §4: ata tohumu **her yerde aynı rozetle** görünür; dört görünür çeşidin ürün ikonu; orak ve tırpan.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `ui_icon_heirloom_seed.png` | `a tiny wheat ear embroidered in burgundy thread on a cream linen patch, badge` |
| `ui_icon_heirloom_seed_gold.png` | `a tiny wheat ear embroidered in gold thread on a cream linen patch with a small ribbon, badge` |
| `icon_item_carrot_purple.png` | `deep purple medieval carrot with green top` |
| `icon_item_carrot_yellow.png` | `pale yellow medieval carrot with green top` |
| `icon_item_radish_black.png` | `round black radish with white flesh showing at a cut` |
| `icon_item_cabbage_red.png` | `purple-red cabbage head` |
| `ui_icon_tool_sickle.png` | `curved iron sickle with a short wooden handle` |
| `ui_icon_tool_scythe.png` | `long scythe with a curved blade and two-handled wooden snath` |
| `icon_item_harvest_doll.png` | `small woven straw harvest doll tied with red thread` |
| `ui_icon_field_ripe.png` | `small golden wheat ear with a tiny sparkle, status marker` |
| `ui_icon_field_mix_risk.png` | `two different seed heads crossing with a small warning dot, status marker` |
| `ui_icon_field_thirsty.png` | `drooping green seedling with a single water drop, status marker` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
