---
id: "342"
title: Gölet, balık sepeti, çıkrık, dokuma tezgâhı, imbik, iksir rafı
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (yeni beceriler, 2026-11-17); "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 10
cost_max: 14
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_pond_3x2_se.png
  - Assets/Art/Iso/_aday/iso_item_fish_trap_se.png
  - Assets/Art/Iso/_aday/iso_item_spinning_wheel_se.png
  - Assets/Art/Iso/_aday/iso_item_loom_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_alembic_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_potion_shelf_se.png
created: 2026-11-17
---
# 342 Yeni becerilerin eşyaları

## Amaç
Balıkçılık (gölet, balık sepeti), dokumacılık (çıkrık, dokuma tezgâhı) ve otacılık (imbik, iksir rafı) istasyonları. Gölet aynı zamanda sulama kanalına su verir. Ahşap olanlar ham, açık tonlu (`finishGroup: wood`).

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- gölet (3x2) = small natural pond with reeds and stones at the edge, clear water, flush with the ground; balık sepeti (1x1) = woven willow fish trap basket half in water with a stake; çıkrık (1x1) = wooden spinning wheel with a bit of wool; dokuma tezgâhı (2x1) = wooden floor loom with half-woven cloth; imbik (2x1) = copper alembic still on a small brick stove with glass bottles; iksir rafı (1x1) = small wooden shelf with colored potion bottles and dried herbs.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
