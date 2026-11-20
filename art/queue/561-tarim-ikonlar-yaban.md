---
id: "561"
title: "Tarım ikonları 6/6: yaban ürünleri (19)"
status: running
claimed_at: 2026-11-20T15:35Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 35
family: arayuz
style_gate: false
cost_est: 19
cost_max: 38
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_bilberry.png
  - Assets/Art/Iso/_aday/icon_item_morel.png
  - Assets/Art/Iso/_aday/icon_item_chanterelle.png
  - Assets/Art/Iso/_aday/icon_item_porcini.png
  - Assets/Art/Iso/_aday/icon_item_hawthorn.png
  - Assets/Art/Iso/_aday/icon_item_sloe.png
  - Assets/Art/Iso/_aday/icon_item_acorn.png
  - Assets/Art/Iso/_aday/icon_item_wild_pear.png
  - Assets/Art/Iso/_aday/icon_item_sorrel.png
  - Assets/Art/Iso/_aday/icon_item_mallow.png
  - Assets/Art/Iso/_aday/icon_item_dandelion.png
  - Assets/Art/Iso/_aday/icon_item_sumac.png
  - Assets/Art/Iso/_aday/icon_item_juniper.png
  - Assets/Art/Iso/_aday/icon_item_salep.png
  - Assets/Art/Iso/_aday/icon_item_st_johns_wort.png
  - Assets/Art/Iso/_aday/icon_item_licorice.png
  - Assets/Art/Iso/_aday/icon_item_marsh_mallow_root.png
  - Assets/Art/Iso/_aday/icon_item_samphire.png
  - Assets/Art/Iso/_aday/icon_item_caper.png
created: 2026-11-20
---
# 561 Tarım ikonları 6/6: yaban ürünleri (19)

## Amaç
Project belgesi 59 §6.2: yalnız toplanan bitkilerin ürün ikonları.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_bilberry.png` | `handful of dark blue bilberries` |
| `icon_item_morel.png` | `morel mushroom with honeycomb cap` |
| `icon_item_chanterelle.png` | `golden chanterelle mushroom` |
| `icon_item_porcini.png` | `stout brown-capped porcini mushroom` |
| `icon_item_hawthorn.png` | `cluster of small red haws` |
| `icon_item_sloe.png` | `dusty blue-black sloes on a thorny twig` |
| `icon_item_acorn.png` | `two acorns with caps` |
| `icon_item_wild_pear.png` | `small hard green wild pear` |
| `icon_item_sorrel.png` | `bunch of arrow-shaped sorrel leaves` |
| `icon_item_mallow.png` | `bunch of round mallow leaves` |
| `icon_item_dandelion.png` | `dandelion leaves with a root` |
| `icon_item_sumac.png` | `dark red sumac fruit cone` |
| `icon_item_juniper.png` | `blue-black juniper berries on a spiky twig` |
| `icon_item_salep.png` | `pair of pale orchid tubers` |
| `icon_item_st_johns_wort.png` | `sprig of yellow St John's wort flowers` |
| `icon_item_licorice.png` | `bundle of woody licorice root sticks` |
| `icon_item_marsh_mallow_root.png` | `pale marsh mallow root` |
| `icon_item_samphire.png` | `bunch of fleshy green samphire` |
| `icon_item_caper.png` | `green caper buds` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
