---
id: "446a"
title: "Kapı varyantları A (3)"
status: running
claimed_at: 2026-11-19T13:43Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 30
cost_max: 35
depends_on: ["329"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_door_red_sw_ne.png
  - Assets/Art/Iso/iso_wall_door_red_se_nw.png
  - Assets/Art/Iso/iso_wall_door_blue_sw_ne.png
  - Assets/Art/Iso/iso_wall_door_blue_se_nw.png
  - Assets/Art/Iso/iso_wall_door_green_sw_ne.png
  - Assets/Art/Iso/iso_wall_door_green_se_nw.png
created: 2026-11-19
---
# 446a Kapı varyantları A (3)

## Amaç
Belge 45 §10: boyalı ve oymalı kapılar (Marangozluk ve Demircilikle açılır).

## Tarif
**329 ile aynı yöntem.**

- `door_red`: `plank door painted madder red, iron strap hinges`
- `door_blue`: `plank door painted indigo blue`
- `door_green`: `plank door painted green`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_wall_door_<ad>_<eksen>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
