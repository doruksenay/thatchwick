---
id: "446c"
title: "Pencere varyantları (4)"
status: review
claimed_at: 2026-11-19T13:49Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 40
cost_max: 45
depends_on: ["330"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_window_shutter_red_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_shutter_red_se_nw.png
  - Assets/Art/Iso/iso_wall_window_shutter_blue_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_shutter_blue_se_nw.png
  - Assets/Art/Iso/iso_wall_window_shutter_green_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_shutter_green_se_nw.png
  - Assets/Art/Iso/iso_wall_window_grille_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_grille_se_nw.png
created: 2026-11-19
---
# 446c Pencere varyantları (4)

## Amaç
Belge 45 §10: renkli kepenkler, demir parmaklık.

## Tarif
**330 ile aynı yöntem.**

- `window_shutter_red`: `shuttered window, shutters painted red`
- `window_shutter_blue`: `shuttered window, shutters painted blue`
- `window_shutter_green`: `shuttered window, shutters painted green`
- `window_grille`: `small window with a wrought iron grille`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_wall_window_<ad>_<eksen>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
