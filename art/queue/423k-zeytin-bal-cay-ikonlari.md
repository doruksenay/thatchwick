---
id: "423k"
title: "Zeytin, bal, çay ürün ikonları (19)"
status: running
claimed_at: 2026-11-19T07:50Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 32
family: arayuz
style_gate: false
cost_est: 19
cost_max: 38
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_sapling_olive.png
  - Assets/Art/Iso/_aday/icon_item_olive.png
  - Assets/Art/Iso/_aday/icon_item_olive_fine.png
  - Assets/Art/Iso/_aday/icon_item_olives_brined.png
  - Assets/Art/Iso/_aday/icon_item_olive_paste.png
  - Assets/Art/Iso/_aday/icon_item_olive_oil.png
  - Assets/Art/Iso/_aday/icon_item_olive_oil_virgin.png
  - Assets/Art/Iso/_aday/icon_item_pomace.png
  - Assets/Art/Iso/_aday/icon_item_leek_olive_oil.png
  - Assets/Art/Iso/_aday/icon_item_soap_olive.png
  - Assets/Art/Iso/_aday/icon_item_honey_strained.png
  - Assets/Art/Iso/_aday/icon_item_chamomile.png
  - Assets/Art/Iso/_aday/icon_item_linden.png
  - Assets/Art/Iso/_aday/icon_item_sage.png
  - Assets/Art/Iso/_aday/icon_item_mint.png
  - Assets/Art/Iso/_aday/icon_item_tea_chamomile.png
  - Assets/Art/Iso/_aday/icon_item_tea_linden.png
  - Assets/Art/Iso/_aday/icon_item_tea_sage.png
  - Assets/Art/Iso/_aday/icon_item_tea_mint.png
created: 2026-11-19
---
# 423k Zeytin, bal, çay ürün ikonları (19)

## Amaç
Zeytin, bal ve bitki çayı zinciri `items.json`'a eklendi (belge 32-7, 8.1, 8.2). `IsoArt.ItemIcon` `icon_item_<id>` adını kendiliğinden bulur.

## Yöntem
419a ile aynı (316 stili): 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk. Pixen yüz üretirse `create_map_object` 32x32. Her açıklamanın sonuna `, simple still-life object, no face, no eyes, no creature, no person`. Cam yok (kap kacak pişmiş toprak ya da bez).

## Tarif
| items.json id | Dosya (`_aday/`) | Açıklama |
|---|---|---|
| `item.sapling_olive` | `icon_item_sapling_olive.png` | `small olive sapling with narrow silver-green leaves in a clod of earth` |
| `item.olive` | `icon_item_olive.png` | `handful of green and black olives` |
| `item.olive_fine` | `icon_item_olive_fine.png` | `small woven basket of hand-picked glossy olives` |
| `item.olives_brined` | `icon_item_olives_brined.png` | `small clay crock of brined green olives` |
| `item.olive_paste` | `icon_item_olive_paste.png` | `clay bowl of dark mashed olive paste` |
| `item.olive_oil` | `icon_item_olive_oil.png` | `small clay jug of golden olive oil` |
| `item.olive_oil_virgin` | `icon_item_olive_oil_virgin.png` | `small stoppered clay flask of green-gold olive oil` |
| `item.pomace` | `icon_item_pomace.png` | `small heap of dark dried olive pomace cakes` |
| `item.leek_olive_oil` | `icon_item_leek_olive_oil.png` | `clay plate of braised leeks in olive oil` |
| `item.soap_olive` | `icon_item_soap_olive.png` | `pale green bar of olive oil soap` |
| `item.honey_strained` | `icon_item_honey_strained.png` | `small clay pot of clear golden strained honey` |
| `item.chamomile` | `icon_item_chamomile.png` | `small bunch of fresh chamomile flowers` |
| `item.linden` | `icon_item_linden.png` | `sprig of pale yellow linden blossoms` |
| `item.sage` | `icon_item_sage.png` | `sprig of grey-green sage leaves` |
| `item.mint` | `icon_item_mint.png` | `sprig of fresh green mint` |
| `item.tea_chamomile` | `icon_item_tea_chamomile.png` | `small cloth bag of dried chamomile` |
| `item.tea_linden` | `icon_item_tea_linden.png` | `small cloth bag of dried linden blossoms` |
| `item.tea_sage` | `icon_item_tea_sage.png` | `small bundle of dried sage tied with string` |
| `item.tea_mint` | `icon_item_tea_mint.png` | `small bundle of dried mint tied with string` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
