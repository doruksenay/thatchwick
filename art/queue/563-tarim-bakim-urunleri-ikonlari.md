---
id: "563"
title: "Bakım ürünleri ve avokado ikonları (17)"
status: review
claimed_at: 2026-11-20T15:35Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 35
family: arayuz
style_gate: false
cost_est: 17
cost_max: 34
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_rose_water.png
  - Assets/Art/Iso/_aday/icon_item_cold_cream.png
  - Assets/Art/Iso/_aday/icon_item_hand_salve.png
  - Assets/Art/Iso/_aday/icon_item_almond_oil.png
  - Assets/Art/Iso/_aday/icon_item_calendula_salve.png
  - Assets/Art/Iso/_aday/icon_item_hypericum_oil.png
  - Assets/Art/Iso/_aday/icon_item_joint_salve.png
  - Assets/Art/Iso/_aday/icon_item_lip_balm.png
  - Assets/Art/Iso/_aday/icon_item_lavender_sachet.png
  - Assets/Art/Iso/_aday/icon_item_henna_paste.png
  - Assets/Art/Iso/_aday/icon_item_chamomile_rinse.png
  - Assets/Art/Iso/_aday/icon_item_walnut_rinse.png
  - Assets/Art/Iso/_aday/icon_item_bath_set.png
  - Assets/Art/Iso/_aday/icon_item_avocado.png
  - Assets/Art/Iso/_aday/icon_item_avocado_cream.png
  - Assets/Art/Iso/_aday/icon_item_avocado_mash.png
  - Assets/Art/Iso/_aday/icon_item_avocado_pit.png
created: 2026-11-20
---
# 563 Bakım ürünleri ve avokado ikonları (17)

## Amaç
Project belgeleri 59 §7 ve 59a §2: Aktarlık altındaki bakım hattı ve tüccardan gelen avokado.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_rose_water.png` | `small stoppered clay flask with a pink rose mark` |
| `icon_item_cold_cream.png` | `small lidded ceramic pot of white cream` |
| `icon_item_hand_salve.png` | `small wooden tub of yellow salve` |
| `icon_item_almond_oil.png` | `small clay bottle with an almond mark` |
| `icon_item_calendula_salve.png` | `small ceramic pot of orange salve` |
| `icon_item_hypericum_oil.png` | `small glass bottle of deep red oil` |
| `icon_item_joint_salve.png` | `small dark clay pot of green salve with a juniper sprig` |
| `icon_item_lip_balm.png` | `tiny round wooden box of honey-coloured balm` |
| `icon_item_lavender_sachet.png` | `small linen sachet tied with purple thread with lavender tips showing` |
| `icon_item_henna_paste.png` | `small bowl of dark green-brown henna paste` |
| `icon_item_chamomile_rinse.png` | `clay jug with a chamomile flower mark` |
| `icon_item_walnut_rinse.png` | `clay jug with a walnut mark` |
| `icon_item_bath_set.png` | `folded cloth with a bar of soap and a small flask` |
| `icon_item_avocado.png` | `dark green pear-shaped avocado cut to show the pale flesh and round pit` |
| `icon_item_avocado_cream.png` | `small lidded ceramic pot of pale green cream` |
| `icon_item_avocado_mash.png` | `slice of bread spread with pale green mash` |
| `icon_item_avocado_pit.png` | `round brown avocado pit sprouting a thin shoot in a small cup of water` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
