---
id: "582"
title: "Mezbaha eşyaları (5)"
status: review
claimed_at: 2026-11-20T15:15Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 30
family: esya
style_gate: false
cost_est: 5
cost_max: 10
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_slaughter_hook_se.png
  - Assets/Art/Iso/_aday/iso_item_meat_locker_se.png
  - Assets/Art/Iso/_aday/iso_item_ice_trough_se.png
  - Assets/Art/Iso/_aday/iso_item_drain_channel_se.png
  - Assets/Art/Iso/_aday/iso_item_hide_rack_se.png
created: 2026-11-20
---
# 582 Mezbaha eşyaları (5)

## Amaç
Mezbaha oda türü (Project belgeleri 60a §2, 60b §2): kesim kancası odayı mezbaha yapar; ürünler mezbaha deposuna düşer. Kesim görünmez; eşyalarda et, kan ya da hayvan çizilmez.

## Tarif
Eşya dalgasının yöntemi (301/302): `create_map_object` stil eşleme, `tools/art/footprint_guide.py` kılavuzu ve maskesi, aile `esya`. Masa üstü eşyalar masa üstü dekor ölçüsünde (meyve sepeti `iso_item_fruit_basket`), duvar eşyası duvar eşyası yönteminde.

| Dosya | Ayak izi | Açıklama |
|---|---|---|
| `iso_item_slaughter_hook_se.png` | 1x1 | `sturdy wooden post frame with iron meat hooks and a heavy chopping block beside it, clean, no meat, no blood` |
| `iso_item_meat_locker_se.png` | 2x1 | `wide closed wooden meat cupboard with slatted doors and an iron latch, a salt trough on top` |
| `iso_item_ice_trough_se.png` | 1x1 | `low stone trough packed with ice blocks and straw` |
| `iso_item_drain_channel_se.png` | 1x1 | `shallow stone floor gutter with a wooden bucket of water beside it` |
| `iso_item_hide_rack_se.png` | 1x1 | `simple wooden rack with a folded pale hide and a coil of rope` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Işık kaynağı kuralı geçerli.

## Notlar
Eşya kimlikleri kodda bu adlarla açıldı (item.slaughter_hook, item.meat_locker, item.ice_trough, item.drain_channel; item.hide_rack zaten vardı, görseli yoktu); pivotlar `IsoPivots.txt`'e kod oturumunda yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
