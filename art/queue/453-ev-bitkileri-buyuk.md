---
id: "453"
title: "Ev bitkileri: büyük saksılar, su teknesi, sarmaşık, pencere çiçekliği (6)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (ev bitkileri ve yeni dekor), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["302", "306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_pot_bay_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_myrtle_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_rose_se.png
  - Assets/Art/Iso/_aday/iso_item_water_garden_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_ivy_wall_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_window_flower_box_sw_ne.png
created: 2026-11-19
---
# 453 Ev bitkileri: büyük saksılar, su teknesi, sarmaşık, pencere çiçekliği (6)

## Amaç
Belge 45 ek: canlı süs bitkileri (sulanır, büyür, mevsiminde çiçek açar). Teraryum dönem dışı (Ward kutusu 1829); yerine eğrelti ve yosun teknesi ile su teknesi bahçesi.

## Tarif
Yer bitkileri 302 ölçeği, duvar bitkileri 306 ile aynı (`sw_ne`). Bitkiler **olgun ve çiçekte** çizilir; solmuş ve çiçeksiz hâli oyun renk tonuyla gösterir.

| # | items.json id | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.pot_bay` | `iso_item_pot_bay_se.png` | 1x1 | `small bay laurel tree in a large terracotta pot, glossy dark leaves` |
| 2 | `item.pot_myrtle` | `iso_item_pot_myrtle_se.png` | 1x1 | `myrtle shrub in a terracotta pot with small white flowers` |
| 3 | `item.pot_rose` | `iso_item_pot_rose_se.png` | 1x1 | `apothecary rose bush in a wooden half-barrel planter, deep pink flowers` |
| 4 | `item.water_garden` | `iso_item_water_garden_se.png` | 1x1 | `round stone basin with water and white water lilies` |
| 5 | `item.ivy_wall` | `iso_wall_item_ivy_wall_sw_ne.png` | duvar | `trailing ivy in a small hanging clay pot on the wall` |
| 6 | `item.window_flower_box` | `iso_wall_item_window_flower_box_sw_ne.png` | pencere, alt bant | `wooden window box planter with small spring flowers` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Eşya → dosya eşlemesi `Assets/Scripts/Game/IsoArt.cs`'te hazır (bu adlarla); görsel gelince kendiliğinden kullanılır. Pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.
- Teslim `cp -n`, `.meta` yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
