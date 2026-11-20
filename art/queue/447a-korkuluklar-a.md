---
id: "447a"
title: "Korkuluk varyantları A (3)"
status: review
claimed_at: 2026-11-19T13:49Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: yapi
style_gate: false
cost_est: 30
cost_max: 35
depends_on: ["332"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_railing_red_sw_ne.png
  - Assets/Art/Iso/iso_wall_railing_red_se_nw.png
  - Assets/Art/Iso/iso_wall_railing_green_sw_ne.png
  - Assets/Art/Iso/iso_wall_railing_green_se_nw.png
  - Assets/Art/Iso/iso_wall_railing_black_sw_ne.png
  - Assets/Art/Iso/iso_wall_railing_black_se_nw.png
created: 2026-11-19
---
# 447a Korkuluk varyantları A (3)

## Amaç
Belge 45 §10 (kullanıcı örneği: kara merdiven, kırmızı korkuluk). Balkon ve galeri korkulukları.

## Tarif
**332 ile aynı yöntem.** Balüster (torna taş direk) yok: Rönesans sonrası.

- `railing_red`: `wooden railing painted red`
- `railing_green`: `wooden railing painted green`
- `railing_black`: `blackened wooden railing`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Oyun kendiliğinden kullanır: `iso_wall_railing_<ad>_<eksen>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
