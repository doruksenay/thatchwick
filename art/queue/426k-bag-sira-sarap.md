---
id: "426k"
title: "Üzüm asması evreleri, üzüm teknesi ve bağ ürünleri ikonları"
status: review
claimed_at: 2026-11-19T12:23Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 32-8.3 (kullanıcı 18.11.2026)
family: esya
style_gate: false
cost_est: 11
cost_max: 22
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_plant_grape_young_se.png
  - Assets/Art/Iso/_aday/iso_plant_grape_grown_se.png
  - Assets/Art/Iso/_aday/iso_plant_grape_fruit_se.png
  - Assets/Art/Iso/_aday/iso_item_grape_tub_se.png
  - Assets/Art/Iso/_aday/icon_item_sapling_grape.png
  - Assets/Art/Iso/_aday/icon_item_grape.png
  - Assets/Art/Iso/_aday/icon_item_must.png
  - Assets/Art/Iso/_aday/icon_item_wine_new.png
  - Assets/Art/Iso/_aday/icon_item_wine_aged.png
created: 2026-11-19
---
# 426k Üzüm asması, üzüm teknesi ve bağ ürünleri

## Amaç
Bağ zinciri (belge 32-8.3): asma → üzüm → teknede ayakla ezme → şıra → pekmez, sirke, şarap (+18 paketi). Bağbozumu şenliği.

## Tarif
- Asma üç evre (371-374 ağaç evreleri ölçeği, 1x1, ahşap çatal destekli): `young grapevine on a short wooden stake`, `grapevine trained on a wooden stake with broad green leaves`, `grapevine on a wooden stake heavy with dark purple grape bunches`.
- Üzüm teknesi (1x1, dış mekân): `wide shallow wooden treading tub with a little grape must inside`.
- İkonlar 423k stili (32x32, saydam, tek renk kontur, en çok 16 renk):
  - asma çeliği: `bundle of grapevine cuttings tied with twine`;
  - üzüm: `bunch of dark purple grapes`;
  - şıra: `clay bowl of cloudy purple grape must`;
  - yeni şarap: `small clay jug of red wine`;
  - eski şarap: `small dusty clay amphora of red wine with a wax seal`.
- Her açıklamanın sonuna `, no face, no eyes, no creature, no person`. Cam yok.

## Doğrulama
`validate_iso.py`; iso `style.py check --family esya`, ikonlar `--family arayuz`.

## Notlar
- Şarap yalnızca +18 paketi açıkken görünür; ikonları yine üretilir.
- Kod eşlemesi görseller gelince kod oturumunun işi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
