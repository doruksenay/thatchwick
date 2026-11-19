---
id: "424k"
title: "Zeytin ağacı, pres yeri, süzgeç, küp ve çay öbekleri (iso)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 32
family: esya
style_gate: false
cost_est: 12
cost_max: 24
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_plant_olive_young_se.png
  - Assets/Art/Iso/_aday/iso_plant_olive_grown_se.png
  - Assets/Art/Iso/_aday/iso_plant_olive_fruit_se.png
  - Assets/Art/Iso/_aday/iso_item_olive_crusher_se.png
  - Assets/Art/Iso/_aday/iso_item_olive_press_se.png
  - Assets/Art/Iso/_aday/iso_item_brine_crock_se.png
  - Assets/Art/Iso/_aday/iso_item_honey_strainer_se.png
  - Assets/Art/Iso/_aday/iso_forage_chamomile_se.png
  - Assets/Art/Iso/_aday/iso_forage_linden_se.png
  - Assets/Art/Iso/_aday/iso_forage_sage_se.png
  - Assets/Art/Iso/_aday/iso_forage_mint_se.png
  - Assets/Art/Iso/_aday/icon_item_tea_rosehip.png
created: 2026-11-19
---
# 424k Zeytin ağacı, pres yeri, süzgeç, küp ve çay öbekleri (iso)

## Amaç
Belge 32-7 (zeytin), 8.1 (çay), 8.2 (bal) için dünyada görünen eşyalar.

## Tarif
- Zeytin ağacı üç evre (371-374 ağaç evreleri ölçeği, 1x1): `young olive tree, thin twisted trunk, narrow silver-green leaves`, `old gnarled olive tree with silver-green crown`, `old gnarled olive tree with clusters of black and green olives`.
- Taş ezme yalağı (1x1, dış mekân): `round stone olive crushing basin with an upright stone wheel and a wooden pole`.
- Zeytin baskısı (1x1, dış mekân): `wooden lever olive press with stacked woven mats and a stone base`.
- Salamura küpü (1x1, mutfak): `squat clay brine crock with a wooden lid`.
- Bal süzgeci (1x1, mutfak): `cloth honey straining bag hanging over a clay bowl on a small wooden stand`.
- Toplama öbekleri (371 toplama düğümü ölçeği): papatya, ıhlamur dalı, adaçayı, yabani nane öbeği.
- Ek: kuru kuşburnu ikonu (423k stili, 32x32): `small cloth bag of dried red rosehips`.
- Her açıklamanın sonuna `, no face, no eyes, no creature, no person`.

## Doğrulama
`validate_iso.py`, `style.py check --family esya` (ikon için `--family arayuz`).

## Notlar
- Kod eşlemesi görseller gelince kod oturumunun işi (`IsoArt.Plants`, `IsoArt.Items`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
