---
id: "445a"
title: "Yeni duvar malzemeleri A (4)"
status: running
claimed_at: 2026-11-19T13:42Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 40
cost_max: 45
depends_on: ["326"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_wall_rubble_lime_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_rubble_lime_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_brick_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_brick_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_timber_red_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_timber_red_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_timber_black_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_timber_black_se_nw.png
created: 2026-11-19
---
# 445a Yeni duvar malzemeleri A (4)

## Amaç
Belge 45 §10: duvar malzemesi çeşitliliği (Taş ustalığıyla açılır).

## Tarif
**326 ile aynı yöntem.**

- `wall_rubble_lime`: `rough rubble stone wall bedded in white lime mortar`
- `wall_brick`: `medieval red brick wall, irregular hand-made bricks`
- `wall_timber_red`: `half-timbered wall with red-painted oak frame and cream infill`
- `wall_timber_black`: `half-timbered wall with blackened oak frame and white infill`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_wall_<parça>_<eksen>`; alçak ve yarım hâl 327 betiğiyle.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
