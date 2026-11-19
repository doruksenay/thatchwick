---
id: "448"
title: "Zemin varyantları (6)"
status: running
claimed_at: 2026-11-19T14:02Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: zemin
style_gate: false
cost_est: 30
cost_max: 35
depends_on: ["333", "335"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_floor_terracotta_a.png
  - Assets/Art/Iso/iso_floor_terracotta_b.png
  - Assets/Art/Iso/iso_floor_brick_herring_a.png
  - Assets/Art/Iso/iso_floor_brick_herring_b.png
  - Assets/Art/Iso/iso_floor_checker_a.png
  - Assets/Art/Iso/iso_floor_checker_b.png
  - Assets/Art/Iso/iso_floor_black_flag_a.png
  - Assets/Art/Iso/iso_floor_black_flag_b.png
  - Assets/Art/Iso/iso_floor_painted_boards_a.png
  - Assets/Art/Iso/iso_floor_painted_boards_b.png
  - Assets/Art/Iso/iso_floor_walnut_a.png
  - Assets/Art/Iso/iso_floor_walnut_b.png
created: 2026-11-19
---
# 448 Zemin varyantları (6)

## Amaç
Belge 45 §10: zemin çeşitliliği (Taş ustalığı ve Marangozluk).

## Tarif
**333 ve 335 ile aynı yöntem** (a/b döşeme çifti, döşeme testi).

- `floor_terracotta`: `square terracotta floor tiles`
- `floor_brick_herring`: `red brick floor laid in herringbone`
- `floor_checker`: `black and white stone checkerboard pavement`
- `floor_black_flag`: `dark basalt flagstones`
- `floor_painted_boards`: `wide floorboards painted red-brown`
- `floor_walnut`: `dark walnut floorboards`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family zemin <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_floor_<ad>_a/_b`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
