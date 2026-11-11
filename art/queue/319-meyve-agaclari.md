---
id: "319"
title: Meyve ağaçları (elma, armut, kiraz, erik; genç, olgun, meyveli)
status: pending
approved_by: pending
cost_est: 12
cost_max: 20
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_apple_{young,grown,fruit}_se.png
  - Assets/Art/Iso/_aday/iso_tree_pear_{young,grown,fruit}_se.png
  - Assets/Art/Iso/_aday/iso_tree_cherry_{young,grown,fruit}_se.png
  - Assets/Art/Iso/_aday/iso_tree_plum_{young,grown,fruit}_se.png
created: 2026-11-11
---
# 319 Meyve ağaçları

## Amaç
`item.tree_apple`, `item.tree_pear`, `item.tree_cherry`, `item.tree_plum` (`plant`, `growDays` 16-24, `fruitSeasons`, `fruitDays`). Meyve ağacı üç görsel hal gösterir: **genç** (büyümeden önce, `IsGrown` yok), **olgun** (büyüdü, meyve yok), **meyveli** (`FruitReady`; hasatla `TakesFruit` meyveyi alır, ağaç kalır). Görsel olarak bunlar bahçe ağacıdır; meşeden (`item.tree_oak`) taç şekli ve meyve renkleriyle ayrılır.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

- Ağaç ayak izi 1x1 (gövde tabanı), tuval 96x112, kılavuz `--n 1`, yükseklik 78-92. Gövde tabanı elmas merkezinde; taç 2 karoya taşabilir ama zemin çizilmez ("no ground").
- genç: boy 40-48, ince gövde, birkaç yaprak; olgun: boy 78-92, geniş yuvarlak taç; meyveli: olgunun aynısı, tacın üzerinde 8-12 görünür meyve (taç ve gövde piksel piksel aynı kalmalı, yalnızca meyve noktaları eklenir).
- Meyve renkleri: elma kırmızı-yeşil, armut sarı-yeşil, kiraz koyu kırmızı (küçük, çok), erik mor.
- Taç renkleri: elma orta yeşil, armut açık yeşil, kiraz koyu yeşil ve pembe çiçek izi, erik gri-yeşil.
- Maliyet: 12 × 1 gen = 12. İkinci deneme payı 8 → `cost_max` 20.

## Doğrulama
301 doğrulama tablosu. Ek: her ağacın 3 hali yan yana; olgun ↔ meyveli farkı yalnızca meyve noktaları mı; meşe ve 4 meyve ağacı yan yana ayırt edilebilir mi.

## Notlar
- Fidan ikonları (`item.sapling_*`) ve meyve ikonları 316 kapsamında.
- Meyveli hal yalnızca fruitSeasons'ta gösterilir; kış çıplak hali ayrı sprite değildir (olgun halin kışın göründüğü kabul edilir, ilk sürümde mevsim kaplaması yok).
