---
id: "425k"
title: "Petekli bal ikonu ve zeytinyağı kandili (iso, yanık/sönük)"
status: accepted
claimed_at: 2026-11-19T07:55Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 32 (kullanıcı 19.11.2026 "hepsini yap")
family: esya
style_gate: false
cost_est: 4
cost_max: 8
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_honeycomb.png
  - Assets/Art/Iso/_aday/icon_item_olive_lamp.png
  - Assets/Art/Iso/_aday/iso_item_olive_lamp_se.png
  - Assets/Art/Iso/_aday/iso_item_olive_lamp_lit_se.png
created: 2026-11-19
---
# 425k Petekli bal ikonu ve zeytinyağı kandili

## Amaç
- `item.honeycomb`: kovandan dumanla kesilen petek (belge 32-8.2).
- `item.olive_lamp`: zeytinyağıyla yanan küçük kil kandil; yağı bitince söner (`needsOil`). Masa ve raf üstüne konur.

## Tarif
- İkonlar 423k stili (32x32, saydam, tek renk kontur, en çok 16 renk): `slab of golden honeycomb with dripping honey`, `small clay oil lamp with a spout`.
- Kandil iso (masa üstü küçük eşya, 302 ölçeği): sönük `small clay olive oil lamp with a short spout, unlit`; yanık `... with a small calm flame at the spout` (`style.py check --light`).
- Her açıklamanın sonuna `, no face, no eyes, no creature, no person`.

## Doğrulama
`validate_iso.py`; ikonlar `style.py check --family arayuz`, kandil `--family esya` (yanık karede `--light`).

## Notlar
- Yanık/sönük seçimi `ItemInstance.GivesLight` ile; `IsoArt` eşlemesi görseller gelince kod oturumunun işi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
