---
id: "550"
title: "Yeni ağaçlar 3/4: şeftali, vişne, muşmula, kızılcık (12)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 25
family: bitki
style_gate: false
cost_est: 12
cost_max: 24
depends_on: ["372"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_peach_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_peach_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_peach_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_sour_cherry_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_sour_cherry_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_sour_cherry_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_medlar_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_medlar_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_medlar_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_cornel_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_cornel_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_cornel_fruit_se.png
created: 2026-11-20
---
# 550 Yeni ağaçlar 3/4: şeftali, vişne, muşmula, kızılcık (12)

## Amaç
Project belgesi 59 §6.1: yeni meyve ağaçları ve çalılar. Üç hâl: genç, olgun, meyveli (meyve mevsiminde çizilir).

## Tarif
Yöntem **372 ve 377 ile aynı**: `create_map_object` stil eşleme, tuval 96x112, pivot 40,24, bitki referansları, 377'deki küçük gövde kılavuzu ve `, the trunk ends directly on the transparent background, no base plate, no turf, no diamond` eki; üretimden sonra `style.py fit --family bitki`, ardından `check`.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| şeftali | `iso_tree_peach_young_se.png` | 30 | `young peach sapling with long narrow leaves` |
| şeftali | `iso_tree_peach_grown_se.png` | 60 | `small peach tree with long drooping leaves` |
| şeftali | `iso_tree_peach_fruit_se.png` | 60 | `peach tree with blushed yellow-pink peaches` |
| vişne | `iso_tree_sour_cherry_young_se.png` | 30 | `young sour cherry sapling` |
| vişne | `iso_tree_sour_cherry_grown_se.png` | 60 | `small sour cherry tree` |
| vişne | `iso_tree_sour_cherry_fruit_se.png` | 60 | `sour cherry tree with many small dark red cherries` |
| muşmula | `iso_tree_medlar_young_se.png` | 28 | `young medlar sapling with long dull leaves` |
| muşmula | `iso_tree_medlar_grown_se.png` | 56 | `small crooked medlar tree with long leaves` |
| muşmula | `iso_tree_medlar_fruit_se.png` | 56 | `leafless late-autumn medlar tree with brown russet medlar fruits` |
| kızılcık | `iso_tree_cornel_young_se.png` | 28 | `young cornelian cherry shrub` |
| kızılcık | `iso_tree_cornel_grown_se.png` | 56 | `small dense cornelian cherry tree` |
| kızılcık | `iso_tree_cornel_fruit_se.png` | 56 | `cornelian cherry tree with small oval bright red fruits` |

- Maliyet 12 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod: `IsoArt.Trees` (`<önek>_young_se`, `_grown_se`, `_fruit_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
