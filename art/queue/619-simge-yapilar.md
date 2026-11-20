---
id: "619"
title: "Simge yapılar (bölge başına 1-2; 9 eşya)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("dediklerini uygula tamamen"; belge 64a §4 Aşama A madde 6)
family: yapi
style_gate: false
cost_est: 22
cost_max: 34
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_boundary_stone_se.png
  - Assets/Art/Iso/_aday/iso_item_lightning_oak_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_hunter_hut_ruin_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_sunken_boat_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_broken_boardwalk_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_watchtower_ruin_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_old_quarry_cleft_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_vineyard_house_ruin_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_beacon_fire_place_se.png
  - Assets/Art/Iso/_aday/iso_item_boundary_fountain_2x1_se_nw.png
created: 2026-11-20
---
# 619 Simge yapılar

## Amaç
Belge 64 §3 Katman 2, 64a madde 27: her bölgede 1-2 benzersiz yer; özgünlüğün ana kaynağı. Kod hazır: `markers.json` items,
etiket `landmark`; görseli gelmeyen simge yapı konmaz (`art.pending`). `item.boundary_stone` (köy yolu) tanımlı; diğerleri
görsel gelince eşya olarak eklenecek (64b).

## Tarif
Yöntem 357 (`create_map_object`, stil eşleme; aile referansları). Ortak ek: `, medieval European countryside, worn and old, high top-down isometric view, light from top-left, single dark outline, transparent background`. Dönem: hayali ortaçağ, yazıt yalnızca aşınmış çizgi.

| # | Dosya | Bölge | Ayak izi | Tuval | Açıklama |
|---|---|---|---|---|---|
| 1 | boundary_stone_se | Köy yolu | 1x1 | 80x112 | `tall weathered standing boundary stone with worn carved lines, lichen, small offering stones at its base` |
| 2 | lightning_oak_2x2_se | Orman | 2x2 | 160x192 | `huge old oak split by lightning, half dead blackened trunk, one side still green` |
| 3 | hunter_hut_ruin_2x2_se | Orman | 2x2 | 160x144 | `ruined small hunter's hut, collapsed roof, antlers above the door` |
| 4 | sunken_boat_2x1_se_nw | Irmak ve göl | 2x1 | 112x80 | `old wooden rowing boat half sunk at the lake shore, reeds growing through` |
| 5 | broken_boardwalk_2x1_se_nw | Sazlık | 2x1 | 112x72 | `collapsed section of an old wooden boardwalk sinking into marsh` |
| 6 | watchtower_ruin_2x2_se | Tepe | 2x2 | 160x224 | `ruined stone watchtower, only two walls standing, ivy` |
| 7 | old_quarry_cleft_2x2_se | Tepe | 2x2 | 160x128 | `old abandoned quarry cleft cut into rock, square tool marks, rubble` |
| 8 | vineyard_house_ruin_2x2_se | Güney Yamaçları | 2x2 | 160x160 | `small abandoned stone vineyard hut, roofless, wild vine climbing` |
| 9 | beacon_fire_place_se | Deniz kıyısı | 1x1 | 80x96 | `stone ring of an old beacon fire on rocks, charred wood` |
| 10 | boundary_fountain_2x1_se_nw | Köy meydanı | 2x1 | 112x112 | `old stone boundary fountain with a carved spout and trough, moss` |

## Doğrulama
`validate_iso.py`, `style.py check --family yapi`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
