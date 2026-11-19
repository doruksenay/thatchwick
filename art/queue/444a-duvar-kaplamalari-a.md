---
id: "444a"
title: "Yeni duvar kaplamaları A (5)"
status: running
claimed_at: 2026-11-19T13:38Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 25
cost_max: 30
depends_on: ["328"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_finish_masonry_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_masonry_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_stencil_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_stencil_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_mural_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_mural_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_star_tile_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_star_tile_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_blue_wash_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_blue_wash_se_nw.png
created: 2026-11-19
---
# 444a Yeni duvar kaplamaları A (5)

## Amaç
Belge 45 §8 ve §10: duvar kaplaması artık güzellik de verir; her evde aynı duvar görünmesin.

## Tarif
**328 ile aynı yöntem** (doku, iki yön `sw_ne` ve `se_nw`, alçak ve yarım hâli 402 betiğiyle kırpılır).

- `finish_masonry`: `lime plaster painted with thin red lines imitating stone blocks (masonry pattern)`
- `finish_stencil`: `lime plaster with repeated ochre and red stencilled flower motifs`
- `finish_mural`: `lime plaster wall painting of a hunting scene in red, ochre and black, 14th century domestic style`
- `finish_star_tile`: `glazed turquoise and white star-and-cross wall tiles, 13th century Anatolian Seljuk style`
- `finish_blue_wash`: `limewash tinted pale indigo blue`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun yeni görselleri kendiliğinden kullanır: `iso_wall_finish_<ad>_<eksen>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
