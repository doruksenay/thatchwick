---
id: "445b"
title: "Yeni duvar malzemeleri B (3)"
status: review
claimed_at: 2026-11-19T13:42Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 30
cost_max: 35
depends_on: ["326"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_wall_black_stone_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_black_stone_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_red_ashlar_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_red_ashlar_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_marble_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_marble_se_nw.png
created: 2026-11-19
---
# 445b Yeni duvar malzemeleri B (3)

## Amaç
Belge 45 §10: duvar malzemesi çeşitliliği (Taş ustalığı 6-10).

## Tarif
**326 ile aynı yöntem.**

- `wall_black_stone`: `dark basalt block wall`
- `wall_red_ashlar`: `fine red sandstone ashlar wall`
- `wall_marble`: `white marble ashlar wall, thin grey veins`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_wall_<parça>_<eksen>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
