---
id: "387"
title: Doldurulan dekor: sepetler, örgüler, sofra (12)
status: review
claimed_at: 2026-11-18T13:22Z
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
  - Assets/Art/Iso/_aday/iso_item_fruit_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_bread_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_cheese_board_se.png
  - Assets/Art/Iso/_aday/iso_item_honey_pot_se.png
  - Assets/Art/Iso/_aday/iso_item_salt_cellar_se.png
  - Assets/Art/Iso/_aday/iso_item_onion_braid_se.png
  - Assets/Art/Iso/_aday/iso_item_garlic_braid_se.png
  - Assets/Art/Iso/_aday/iso_item_cured_meat_hanger_se.png
  - Assets/Art/Iso/_aday/iso_item_dried_fruit_string_se.png
  - Assets/Art/Iso/_aday/iso_item_elderflower_jug_se.png
  - Assets/Art/Iso/_aday/iso_item_nut_bowl_se.png
  - Assets/Art/Iso/_aday/iso_item_vegetable_crate_se.png
created: 2026-11-18
---
# 387 Doldurulan dekor: sepetler, örgüler, sofra (12)

## Amaç
Ev dekorasyonu D2 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026): tarz ekseni (Çatkı Evi, Yörük Odası), doldurulan dekor, masa ve sedir örtüsü, pencere içi ve perde. Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- **Dolu hâl çizilir** (oyunda boşken soluk ve küçük görünür, bayat ve çürük renk tonuyla gösterilir). Boş hâl görseli ileride ayrı iş olabilir.
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Tarz ipuçları: **Çatkı Evi** meşe, demir kulp, saz, badana (Kuzey Avrupa köyü); **Yörük Odası** sedir, minder, kilim, boyalı ahşap, bakır (Anadolu). Kök boyalar: kökboya kırmızısı, çivit mavisi, cehri sarısı, doğal yün.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.fruit_basket` (Köylü) | `iso_item_fruit_basket_se.png` | yuva O | 32x28 | `shallow wicker basket heaped with red apples and pears` |
| 2 | `item.bread_basket` (Köylü) | `iso_item_bread_basket_se.png` | yuva O | 32x24 | `oval wicker basket with three round loaves of brown bread` |
| 3 | `item.cheese_board` (Zanaatkâr) | `iso_item_cheese_board_se.png` | yuva O | 32x20 | `wooden cheese board with a wheel of pale cheese and a small knife` |
| 4 | `item.honey_pot` (Köylü) | `iso_item_honey_pot_se.png` | yuva K | 20x24 | `small glazed earthenware honey pot with a wooden dipper` |
| 5 | `item.salt_cellar` (Köylü) | `iso_item_salt_cellar_se.png` | yuva K | 16x16 | `small turned wooden salt cellar filled with white salt` |
| 6 | `item.onion_braid` (Köylü) | `iso_item_onion_braid_se.png` | kanca O | 20x40 | `braided string of golden onions hanging from a loop` |
| 7 | `item.garlic_braid` (Köylü) | `iso_item_garlic_braid_se.png` | kanca O | 20x40 | `braided string of white garlic bulbs hanging from a loop` |
| 8 | `item.cured_meat_hanger` (Köylü) | `iso_item_cured_meat_hanger_se.png` | kanca O | 20x40 | `two pieces of dark red air-dried cured beef hanging on a cord` |
| 9 | `item.dried_fruit_string` (Köylü) | `iso_item_dried_fruit_string_se.png` | kanca K | 16x40 | `string of dried apple rings and dried plums hanging on a cord` |
| 10 | `item.elderflower_jug` (Köylü) | `iso_item_elderflower_jug_se.png` | yuva K | 20x32 | `small earthenware jug holding sprigs of creamy white elderflowers` |
| 11 | `item.nut_bowl` (Köylü) | `iso_item_nut_bowl_se.png` | yuva K | 24x16 | `small wooden bowl of hazelnuts and chestnuts` |
| 12 | `item.vegetable_crate` (Köylü) | `iso_item_vegetable_crate_se.png` | yuva B | 32x28 | `low wooden crate full of turnips, carrots, leeks and a cabbage` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
