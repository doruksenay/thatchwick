---
id: "447c"
title: "Çit varyantları (3)"
status: accepted
claimed_at: 2026-11-19T21:21Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 30
cost_max: 35
depends_on: ["331"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_fence_cob_sw_ne.png
  - Assets/Art/Iso/iso_wall_fence_cob_se_nw.png
  - Assets/Art/Iso/iso_wall_fence_wattle_hurdle_sw_ne.png
  - Assets/Art/Iso/iso_wall_fence_wattle_hurdle_se_nw.png
  - Assets/Art/Iso/iso_wall_fence_iron_sw_ne.png
  - Assets/Art/Iso/iso_wall_fence_iron_se_nw.png
created: 2026-11-19
---
# 447c Çit varyantları (3)

## Amaç
Belge 45 §10.

## Tarif
**331 ile aynı yöntem.**

- `fence_cob`: `low mud-brick garden wall with a thatched coping`
- `fence_wattle_hurdle`: `dense woven wattle hurdle fence`
- `fence_iron`: `wrought iron garden grille fence`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_wall_fence_<ad>_<eksen>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
