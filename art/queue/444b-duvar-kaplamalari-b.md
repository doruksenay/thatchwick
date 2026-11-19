---
id: "444b"
title: "Yeni duvar kaplamaları B (5)"
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
  - Assets/Art/Iso/iso_wall_finish_rose_wash_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_rose_wash_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_green_wash_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_green_wash_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_red_band_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_red_band_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_black_panel_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_black_panel_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_checker_paint_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_checker_paint_se_nw.png
created: 2026-11-19
---
# 444b Yeni duvar kaplamaları B (5)

## Amaç
Belge 45 §8 ve §10: duvar kaplaması artık güzellik de verir; her evde aynı duvar görünmesin.

## Tarif
**328 ile aynı yöntem** (doku, iki yön `sw_ne` ve `se_nw`, alçak ve yarım hâli 402 betiğiyle kırpılır).

- `finish_rose_wash`: `limewash tinted pale madder pink`
- `finish_green_wash`: `limewash tinted pale sage green`
- `finish_red_band`: `plaster wall with a broad red ochre band along the bottom`
- `finish_black_panel`: `dark blackened oak wall panelling`
- `finish_checker_paint`: `plaster painted with a red and cream checker pattern`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun yeni görselleri kendiliğinden kullanır: `iso_wall_finish_<ad>_<eksen>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
