---
id: "560"
title: "Tarım ikonları 5/6: meyve ve fidan (19)"
status: pending
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
  - Assets/Art/Iso/_aday/icon_item_mulberry.png
  - Assets/Art/Iso/_aday/icon_item_fig.png
  - Assets/Art/Iso/_aday/icon_item_walnut.png
  - Assets/Art/Iso/_aday/icon_item_almond.png
  - Assets/Art/Iso/_aday/icon_item_quince.png
  - Assets/Art/Iso/_aday/icon_item_apricot.png
  - Assets/Art/Iso/_aday/icon_item_peach.png
  - Assets/Art/Iso/_aday/icon_item_sour_cherry.png
  - Assets/Art/Iso/_aday/icon_item_medlar.png
  - Assets/Art/Iso/_aday/icon_item_cornel.png
  - Assets/Art/Iso/_aday/icon_item_bitter_orange.png
  - Assets/Art/Iso/_aday/icon_item_lemon.png
  - Assets/Art/Iso/_aday/icon_item_carob.png
  - Assets/Art/Iso/_aday/icon_item_raspberry.png
  - Assets/Art/Iso/_aday/icon_item_currant.png
  - Assets/Art/Iso/_aday/icon_item_henna_leaf.png
  - Assets/Art/Iso/_aday/icon_item_sapling_fruit_tree.png
  - Assets/Art/Iso/_aday/icon_item_sapling_citrus.png
  - Assets/Art/Iso/_aday/icon_item_scion.png
created: 2026-11-20
---
# 560 Tarım ikonları 5/6: meyve ve fidan (19)

## Amaç
Project belgesi 59 §6.1: yeni ağaç ve çalıların meyvesi ve fidanı.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk.
- Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Tarif
| Dosya (`_aday/`) | Açıklama |
|---|---|
| `icon_item_mulberry.png` | `handful of dark purple mulberries` |
| `icon_item_fig.png` | `purple fig cut to show red inside` |
| `icon_item_walnut.png` | `walnut in shell with one cracked half` |
| `icon_item_almond.png` | `almonds in shell` |
| `icon_item_quince.png` | `knobbly yellow quince` |
| `icon_item_apricot.png` | `two orange apricots` |
| `icon_item_peach.png` | `blushed peach` |
| `icon_item_sour_cherry.png` | `pair of dark red sour cherries on stems` |
| `icon_item_medlar.png` | `brown russet medlar fruit` |
| `icon_item_cornel.png` | `oval bright red cornelian cherries` |
| `icon_item_bitter_orange.png` | `rough-skinned bitter orange` |
| `icon_item_lemon.png` | `yellow lemon` |
| `icon_item_carob.png` | `dark brown carob pods` |
| `icon_item_raspberry.png` | `red raspberries` |
| `icon_item_currant.png` | `string of red currants` |
| `icon_item_henna_leaf.png` | `small pile of dried green henna leaves` |
| `icon_item_sapling_fruit_tree.png` | `small bare-root fruit sapling with a soil ball, generic` |
| `icon_item_sapling_citrus.png` | `small citrus sapling with glossy leaves and a soil ball` |
| `icon_item_scion.png` | `bundle of grafting scion twigs tied with twine` |

## Doğrulama
`validate_iso.py`, `style.py check --family arayuz`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
