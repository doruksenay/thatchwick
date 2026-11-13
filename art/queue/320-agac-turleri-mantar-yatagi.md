---
id: "320"
title: Yeni ağaç türleri ve bodrum mantar yatağı (huş, çam, söğüt; mantar yatağı)
status: pending
approved_by: pending
family: bitki
style_gate: false
cost_est: 7
cost_max: 12
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_birch_{young,grown}_se.png
  - Assets/Art/Iso/_aday/iso_tree_pine_{young,grown}_se.png
  - Assets/Art/Iso/_aday/iso_tree_willow_{young,grown}_se.png
  - Assets/Art/Iso/_aday/iso_item_mushroom_bed_se.png
created: 2026-11-11
---
# 320 Yeni ağaç türleri ve bodrum mantar yatağı

## Amaç
`item.tree_birch` (hızlı, az odun), `item.tree_pine` (yavaş, çok odun + reçine), `item.tree_willow` (söğüt filizi). Her biri meşeden görsel olarak ayrılmalı (renk ve silüet). `item.mushroom_bed` yalnızca bodrumda (`levels: [-1]`) durur; 1x1 saman ve çürük kütük karışımı yatak, üzerinde mantar topluluğu (olgunken `FruitReady`).

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

- Ağaç ayak izi 1x1 (gövde tabanı), tuval 96x112, kılavuz `--n 1`, yükseklik 78-92. Gövde tabanı elmas merkezinde; taç 2 karoya taşabilir ama zemin çizilmez ("no ground").
- Ağaç silüetleri: huş ince beyaz gövdeli ve açık yeşil seyrek tepeli (genç boy 40, olgun 76); çam koyu yeşil üçgen katlı, kahverengi gövde (genç 44, olgun 96); söğüt sarkık uzun dallı, gri-yeşil (genç 40, olgun 84).
- Mantar yatağı: tuval 64x40, yükseklik 14; açıklama `low wooden tray bed of dark soil and straw with a cluster of pale cream mushrooms, cellar style, muted colours`; olgun/olgunsuz ayrı sprite yok (ilk sürümde sabit; sprite'taki mantarlar hep görünür kabul).
- Maliyet: 7 × 1 gen = 7. İkinci deneme payı 5 → `cost_max` 12.

## Doğrulama
301 doğrulama tablosu. Ek: meşe + 3 yeni tür yan yana olgun hallerde ayırt ediliyor mu; mantar yatağı koyu bodrum zemin rengi (Assets/Art/Iso/_aday/iso_ground_cellar kabul edildiyse) üzerinde okunuyor mu.

## Notlar
- Reçine, söğüt filizi ve fidan ikonları 316 kapsamında.
