---
id: "449"
title: "Çatı varyantları (3)"
status: review
claimed_at: 2026-11-19T21:26Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 12
cost_max: 20
depends_on: ["336b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_roof_stone_slab_a.png
  - Assets/Art/Iso/iso_roof_stone_slab_ridge.png
  - Assets/Art/Iso/iso_roof_tile_glazed_a.png
  - Assets/Art/Iso/iso_roof_tile_glazed_ridge.png
  - Assets/Art/Iso/iso_roof_tile_black_a.png
  - Assets/Art/Iso/iso_roof_tile_black_ridge.png
created: 2026-11-19
---
# 449 Çatı varyantları (3)

## Amaç
Belge 45 §10.

## Tarif
**336b ile aynı yöntem.**

- `roof_stone_slab`: `stone slab roof`
- `roof_tile_glazed`: `glazed roof tiles in green, ochre and black diamond pattern (Burgundian style)`
- `roof_tile_black`: `dark fired clay roof tiles`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_roof_<ad>_a/_ridge`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
