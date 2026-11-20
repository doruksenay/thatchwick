---
id: "552"
title: "Yeni çalılar: ahududu, frenk üzümü, kına (9)"
status: running
claimed_at: 2026-11-20T14:43Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 25
family: bitki
style_gate: false
cost_est: 9
cost_max: 18
depends_on: ["372"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_bush_raspberry_young_se.png
  - Assets/Art/Iso/_aday/iso_bush_raspberry_grown_se.png
  - Assets/Art/Iso/_aday/iso_bush_raspberry_fruit_se.png
  - Assets/Art/Iso/_aday/iso_bush_currant_young_se.png
  - Assets/Art/Iso/_aday/iso_bush_currant_grown_se.png
  - Assets/Art/Iso/_aday/iso_bush_currant_fruit_se.png
  - Assets/Art/Iso/_aday/iso_bush_henna_young_se.png
  - Assets/Art/Iso/_aday/iso_bush_henna_grown_se.png
  - Assets/Art/Iso/_aday/iso_bush_henna_fruit_se.png
created: 2026-11-20
---
# 552 Yeni çalılar: ahududu, frenk üzümü, kına (9)

## Amaç
Project belgesi 59 §6.1: yeni meyve ağaçları ve çalılar. Üç hâl: genç, olgun, meyveli (meyve mevsiminde çizilir).

## Tarif
422k'daki çalı yöntemi (1x1 karo, 371-374 ölçeği), aile `bitki`.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| ahududu | `iso_bush_raspberry_young_se.png` | 15 | `a few young raspberry canes` |
| ahududu | `iso_bush_raspberry_grown_se.png` | 30 | `raspberry canes with green leaves` |
| ahududu | `iso_bush_raspberry_fruit_se.png` | 30 | `raspberry canes with red raspberries` |
| frenk üzümü | `iso_bush_currant_young_se.png` | 14 | `small currant shrub` |
| frenk üzümü | `iso_bush_currant_grown_se.png` | 28 | `currant shrub with lobed leaves` |
| frenk üzümü | `iso_bush_currant_fruit_se.png` | 28 | `currant shrub with hanging strings of red currants` |
| kına | `iso_bush_henna_young_se.png` | 17 | `small henna shrub with tiny leaves` |
| kına | `iso_bush_henna_grown_se.png` | 34 | `dense henna shrub with small oval leaves` |
| kına | `iso_bush_henna_fruit_se.png` | 34 | `henna shrub with clusters of tiny white flowers` |

- Maliyet 9 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod: `IsoArt.Trees` (`<önek>_young_se`, `_grown_se`, `_fruit_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
