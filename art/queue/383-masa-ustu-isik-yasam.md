---
id: "383"
title: Masa ve raf üstü: ışık, saksı otları, iplik ve sepetler (12)
status: pending
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_tallow_candle_se.png
  - Assets/Art/Iso/_aday/iso_item_beeswax_candle_se.png
  - Assets/Art/Iso/_aday/iso_item_pricket_candlestick_se.png
  - Assets/Art/Iso/_aday/iso_item_folding_candlestick_se.png
  - Assets/Art/Iso/_aday/iso_item_clay_lamp_se.png
  - Assets/Art/Iso/_aday/iso_item_horn_lantern_se.png
  - Assets/Art/Iso/_aday/iso_item_herb_pot_basil_se.png
  - Assets/Art/Iso/_aday/iso_item_herb_pot_rosemary_se.png
  - Assets/Art/Iso/_aday/iso_item_herb_pot_thyme_se.png
  - Assets/Art/Iso/_aday/iso_item_yarn_spindle_se.png
  - Assets/Art/Iso/_aday/iso_item_sewing_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_hanging_basket_se.png
created: 2026-11-18
---
# 383 Masa ve raf üstü: ışık, saksı otları, iplik ve sepetler (12)

## Amaç
Ev dekorasyonu D1 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Yuva türü ve boyutu, duvar bandı ve tavan katmanı kodda; görsel yalnızca eşyanın kendisi.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Dönem notu: cam baca yok; fener boynuz camlı. İç yağı mumu soluk sarı, balmumu altın sarısı.
- **Işık kaynağı kuralı:** alevli ya da yanan ışık içeren görsellerde `style.py check --light` (TEMPLATE).

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.tallow_candle` (Köylü) | `iso_item_tallow_candle_se.png` | yuva K | 16x28 | `short yellowish tallow candle on a clay saucer, small flame` |
| 2 | `item.beeswax_candle` (Tüccar) | `iso_item_beeswax_candle_se.png` | yuva K | 16x32 | `tall golden beeswax candle in a brass holder, clean flame` |
| 3 | `item.pricket_candlestick` (Köylü) | `iso_item_pricket_candlestick_se.png` | yuva K | 16x32 | `iron pricket candlestick (candle on a spike), lit` |
| 4 | `item.folding_candlestick` (Zanaatkâr) | `iso_item_folding_candlestick_se.png` | yuva K | 20x32 | `small three-legged folding candlestick, lit candle` |
| 5 | `item.clay_lamp` (Köylü) | `iso_item_clay_lamp_se.png` | yuva K | 20x16 | `small clay oil lamp with a lit wick` |
| 6 | `item.horn_lantern` (Zanaatkâr) | `iso_item_horn_lantern_se.png` | yuva O / kanca | 20x32 | `iron lantern with translucent horn panes, glowing` |
| 7 | `item.herb_pot_basil` (Köylü) | `iso_item_herb_pot_basil_se.png` | yuva K | 20x24 | `small clay pot with basil` |
| 8 | `item.herb_pot_rosemary` (Köylü) | `iso_item_herb_pot_rosemary_se.png` | yuva K | 20x24 | `small clay pot with rosemary` |
| 9 | `item.herb_pot_thyme` (Köylü) | `iso_item_herb_pot_thyme_se.png` | yuva K | 20x24 | `small clay pot with thyme` |
| 10 | `item.yarn_spindle` (Köylü) | `iso_item_yarn_spindle_se.png` | yuva K | 20x20 | `ball of wool yarn with a wooden drop spindle` |
| 11 | `item.sewing_basket` (Köylü) | `iso_item_sewing_basket_se.png` | yuva O | 28x24 | `small wicker sewing basket with cloth and thread` |
| 12 | `item.hanging_basket` (Köylü) | `iso_item_hanging_basket_se.png` | kanca O | 24x32 | `wicker basket hanging from a rope loop` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...` (ışıklılarda `--light`). Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
