---
id: "470"
title: "Cam bahçe kapları: küçük ve büyük (2)"
status: running
claimed_at: 2026-11-20T07:19Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe), Project belgesi 47
priority: 20
family: esya
style_gate: false
cost_est: 4
cost_max: 8
depends_on: ["302", "383"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_glass_garden_se.png
  - Assets/Art/Iso/_aday/iso_item_glass_garden_large_se.png
created: 2026-11-20
---
# 470 Cam bahçe kapları: küçük ve büyük (2)

## Amaç
Belge 47: teraryumun dönem karşılığı. Küçük cam bahçe masa üstüne (383 yöntemi), büyük olan yere (302 ölçeği).

## Tarif
Küçük: **383 ile aynı yöntem** (masa üstü, pivot tabanda). Büyük: 302 ölçeği, `se` yönü; aynalama kodla.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.glass_garden` | `iso_item_glass_garden_se.png` | yuva M | `small greenish forest-glass bell jar on a wooden base, moss and a fern inside` |
| 2 | `item.glass_garden_large` | `iso_item_glass_garden_large_se.png` | 1x1 | `large rectangular greenish forest-glass case with lead-strip frame on a carved oak stand, ferns and moss inside` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): orman camı (yeşilimsi, kabarcıklı), Yeni Dünya türü yok.

## Notlar
Kod görseli adla bulur (`IsoArt.Get`), gelene kadar renkli şekil çizer; dosya adı değişmesin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
