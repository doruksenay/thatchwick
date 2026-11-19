---
id: "440"
title: "Gömme ve ev eşyaları (5)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301", "306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_wall_niche_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_wall_niche_lamp_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_window_seat_sw_ne.png
  - Assets/Art/Iso/_aday/iso_ceiling_item_painted_beam_se.png
  - Assets/Art/Iso/_aday/iso_item_embroidery_frame_se.png
created: 2026-11-19
---
# 440 Gömme ve ev eşyaları (5)

## Amaç
Belge 45: duvar nişi (yalnızca kalın duvara oyulur), lamba nişi, pencere oturağı, kalem işi kiriş (tavan katmanı), nakış kasnağı (desen çizmek için istasyon).

## Tarif
Duvar eşyaları **306 ile aynı** (`sw_ne`), tavan eşyası 385 ile aynı, yer eşyası 302 ölçeği. Pencere oturağı pencerenin alt bandına oturur.

| # | items.json id | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.wall_niche` | `iso_wall_item_wall_niche_sw_ne.png` | duvar, orta bant | `small arched recess cut into a thick stone wall, plastered inside, two little ledges` |
| 2 | `item.wall_niche_lamp` | `iso_wall_item_wall_niche_lamp_sw_ne.png` | duvar | `tall pointed-arch lamp niche in a stone wall, soot-darkened top` |
| 3 | `item.window_seat` | `iso_wall_item_window_seat_sw_ne.png` | pencere, alt bant | `low wooden window seat built into a deep window recess, plain plank top` |
| 4 | `item.ceiling_painted_beam` | `iso_ceiling_item_painted_beam_se.png` | tavan | `short wooden ceiling beam painted with red, blue and ochre geometric bands, seen from below at an angle` |
| 5 | `item.embroidery_frame` | `iso_item_embroidery_frame_se.png` | 1x1 | `wooden embroidery hoop frame on a small stand with a half-stitched cloth` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Eşya → dosya eşlemesi `Assets/Scripts/Game/IsoArt.cs`'te hazır (bu adlarla); görsel gelince kendiliğinden kullanılır. Pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.
- Teslim `cp -n`, `.meta` yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
