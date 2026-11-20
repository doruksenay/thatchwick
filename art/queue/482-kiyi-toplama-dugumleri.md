---
id: "482"
title: "Deniz kıyısı toplama düğümleri (3)"
status: review
claimed_at: 2026-11-20T09:48Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe her şey doğadan, deniz kıyısı, balık tutma oyunu), Project belgeleri 47 ve 50
priority: 20
family: zemin
style_gate: false
cost_est: 3
cost_max: 6
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_tidepool_se.png
  - Assets/Art/Iso/_aday/iso_forage_driftline_se.png
  - Assets/Art/Iso/_aday/iso_forage_sea_sand_se.png
created: 2026-11-20
---
# 482 Deniz kıyısı toplama düğümleri (3)

## Amaç
Belge 47 ek: gölün güneyine eklenen deniz kıyısı bölgesinde gelgit çukuru, kıyı çizgisi ve deniz kumsalı. Cam bahçe için deniz yosunu, deniz suyu, deniz kumu ve kabuklar buradan gelir.

## Tarif
322 ile aynı yöntem: `create_map_object` ya da pixflux, izometrik 1x1, `se` yönü, taban ortası pivot. Kum ve kaya tonları `zemin` ailesinden.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.forage_tidepool` | `iso_forage_tidepool_se.png` | 1x1 | `small rock pool in grey shore rocks, clear sea water, green seaweed, a periwinkle shell` |
| 2 | `item.forage_driftline` | `iso_forage_driftline_se.png` | 1x1 | `strandline on sand: line of dried brown seaweed, a piece of driftwood, small shells` |
| 3 | `item.forage_sea_sand` | `iso_forage_sea_sand_se.png` | 1x1 | `small heap of pale sea sand with a few shells on a beach` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family zemin <dosya> ...`, yan yana `art/results/482-yanyana.png`. Dönem kuralları (CLAUDE.md, Project belgesi 20).

## Notlar
Kod görseli adla bulur; dosya adı değişmesin. Gelene kadar yer tutucu çizilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
