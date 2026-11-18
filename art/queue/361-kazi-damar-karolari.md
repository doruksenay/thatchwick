---
id: "361"
title: Kazı damar karoları (8 hammadde) ve damar izi
status: accepted   # kullanıcı 18.11.2026: "hepsi iyi duruyor"
claimed_at: 2026-11-18T13:45Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 28 (18.11.2026, "tek seferde yapalım")
priority: 10
family: zemin
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["360"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_rock_vein_limestone_a.png
  - Assets/Art/Iso/iso_rock_vein_flint_a.png
  - Assets/Art/Iso/iso_rock_vein_clay_a.png
  - Assets/Art/Iso/iso_rock_vein_ochre_a.png
  - Assets/Art/Iso/iso_rock_vein_coal_a.png
  - Assets/Art/Iso/iso_rock_vein_iron_ore_a.png
  - Assets/Art/Iso/iso_rock_vein_gypsum_a.png
  - Assets/Art/Iso/iso_rock_vein_slate_stone_a.png
  - Assets/Art/Iso/iso_rock_vein_hint.png
created: 2026-11-18
---
# 361 Kazı damar karoları (8 hammadde) ve damar izi

## Amaç
Damar karosu (MineTile.Ore) ve açık karonun yakınında görünen damar izi (23. belge 6.2). 1.0'ın ilk dilimindeki hammaddeler: arsa ve ocak galeri katı.

## Tarif
360'taki taş bloğunun (`iso_rock_stone_a.png`) üst yüzüne ve ön yüzüne damar şeridi; yöntem 360 ile aynı, 360 çıktısı referans görsel.
 - `iso_rock_vein_limestone_a.png`: `low cut grey stone block crossed by pale cream limestone layers`
- `iso_rock_vein_flint_a.png`: `low cut grey stone block crossed by dark glossy flint nodules`
- `iso_rock_vein_clay_a.png`: `low cut grey stone block crossed by wet orange-grey clay pocket`
- `iso_rock_vein_ochre_a.png`: `low cut grey stone block crossed by red and yellow ochre earth streaks`
- `iso_rock_vein_coal_a.png`: `low cut grey stone block crossed by black coal seam with dull shine`
- `iso_rock_vein_iron_ore_a.png`: `low cut grey stone block crossed by rusty red-brown iron ore streaks`
- `iso_rock_vein_gypsum_a.png`: `low cut grey stone block crossed by white chalky gypsum crystals`
- `iso_rock_vein_slate_stone_a.png`: `low cut grey stone block crossed by dark blue-grey slate layers`
- `iso_rock_vein_hint.png`: 64x32 saydam üst katman, kenarlarda soluk renkli lekeler (`faint colored mineral speckles at the tile edge, transparent background`); kod rengini damara göre boyar.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
