---
id: "442"
title: "Bahçe: çimen sıra, güneş saati, güvercinlikler (4)"
status: review
claimed_at: 2026-11-19T13:34Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["302"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_turf_seat_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_sundial_se.png
  - Assets/Art/Iso/_aday/iso_item_dovecote_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_dovecote_village_se.png
created: 2026-11-19
---
# 442 Bahçe: çimen sıra, güneş saati, güvercinlikler (4)

## Amaç
Belge 45: bahçe güzelliği (ev yarışması puanı), güvercinlik her sabah gübre verir. Çatkı tarafında kule güvercinliği (beylere özgü), Yörük tarafında kerpiç güvercinlik.

## Tarif
302 ölçeği; 2x1 tek yön `_se` (uzun eksen "\"), 2x2 cicim (390) gibi 144 px.

| # | items.json id | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.turf_seat` | `iso_item_turf_seat_2x1_se.png` | 2x1, dış | `medieval turf bench: low stone-edged box filled with grass and tiny flowers` |
| 2 | `item.sundial` | `iso_item_sundial_se.png` | 1x1, dış | `stone pedestal sundial with an iron gnomon` |
| 3 | `item.dovecote` | `iso_item_dovecote_2x2_se.png` | 2x2, dış | `small round stone dovecote tower with conical roof and rows of nesting holes` |
| 4 | `item.dovecote_village` | `iso_item_dovecote_village_se.png` | 1x1, dış | `small mud-brick pigeon house with clay pipe nesting holes, Anatolian village style` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Eşya → dosya eşlemesi `Assets/Scripts/Game/IsoArt.cs`'te hazır (bu adlarla); görsel gelince kendiliğinden kullanılır. Pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.
- Teslim `cp -n`, `.meta` yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
