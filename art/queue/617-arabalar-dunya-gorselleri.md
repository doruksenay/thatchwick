---
id: "617"
title: Arabaların dünya görselleri (el arabası, hafif at arabası, üstü örtülü araba, kış kızağı)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: esya
style_gate: false
cost_est: 50
cost_max: 60
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_wheelbarrow_se.png
  - Assets/Art/Iso/iso_item_horse_cart_2x1_se.png
  - Assets/Art/Iso/iso_item_covered_cart_2x1_se.png
  - Assets/Art/Iso/iso_item_sled_2x1_se.png
created: 2026-11-20
---
# 617 Arabaların dünya görselleri (el arabası, hafif at arabası, üstü örtülü araba, kış kızağı)

## Amaç
Belge 57e §2.2: arabalar avluda duran, yerleştirilen eşya olacak ve haritada gidip gelecek. Bu PR'da filo çekirdeği ve paneli var; arabalar hâlâ envanter kaynağı. Bu görseller gelince arabalar yerleştirilen eşyaya geçer.

## Tarif
Eşya ailesi stil eşlemesi; var olan `iso_item_ox_cart` referans (aynı ölçek). El arabası tek karo, tek tekerlekli ahşap. Hafif at arabası iki tekerlekli, okları yerde. Üstü örtülü araba keten tenteli. Kış kızağı ahşap kızaklı, yük sandığı.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`.

## Notlar
Koşulu hayvanla birlikte yürüyüş kareleri ayrı iş olur (araba dünyada yürüyünce).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
