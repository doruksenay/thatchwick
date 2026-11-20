---
id: "703"
title: Kompost yığını evreleri (boş, dolu, sıcak, hazır)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: esya
style_gate: false
cost_est: 25
cost_max: 40
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_compost_heap_empty_se.png
  - Assets/Art/Iso/iso_item_compost_heap_full_se.png
  - Assets/Art/Iso/iso_item_compost_heap_ready_se.png
created: 2026-11-20
---
# 703 Kompost yığını evreleri (boş, dolu, sıcak, hazır)

## Amaç
Belge 57b §3: kompost artık süreç. Yığının doluluğu ve hazır olması bakınca anlaşılmalı; sıcak evrede kod buhar parçacığı çıkarır.

## Tarif
Var olan `iso_item_compost_heap_se.png` referans alınır, aynı pivot ve boyut. Boş: çerçeve ve dipte az kalıntı. Dolu: yeşil ve kahve katmanlı taze yığın. Hazır: koyu, ufalanmış kara toprak.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`. Ek kontrol: üç kare aynı tuval ve pivotta.

## Notlar
Kod bağlantısı görsel gelince: `CompostHeap.IsEmpty`, `Hot`, `ReadyTotal`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
