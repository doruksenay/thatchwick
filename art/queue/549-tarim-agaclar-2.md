---
id: "549"
title: "Yeni ağaçlar 2/4: badem, ayva, fındık bahçesi, kayısı (12)"
status: review
claimed_at: 2026-11-20T14:34Z
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
  - Assets/Art/Iso/_aday/iso_tree_almond_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_almond_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_almond_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_quince_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_quince_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_quince_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_hazel_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_hazel_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_hazel_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_apricot_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_apricot_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_apricot_fruit_se.png
created: 2026-11-20
---
# 549 Yeni ağaçlar 2/4: badem, ayva, fındık bahçesi, kayısı (12)

## Amaç
Project belgesi 59 §6.1: yeni meyve ağaçları ve çalılar. Üç hâl: genç, olgun, meyveli (meyve mevsiminde çizilir).

## Tarif
Yöntem **372 ve 377 ile aynı**: `create_map_object` stil eşleme, tuval 96x112, pivot 40,24, bitki referansları, 377'deki küçük gövde kılavuzu ve `, the trunk ends directly on the transparent background, no base plate, no turf, no diamond` eki; üretimden sonra `style.py fit --family bitki`, ardından `check`.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| badem | `iso_tree_almond_young_se.png` | 32 | `young almond sapling with narrow leaves` |
| badem | `iso_tree_almond_grown_se.png` | 64 | `slender almond tree with narrow leaves` |
| badem | `iso_tree_almond_fruit_se.png` | 64 | `almond tree with fuzzy grey-green almond husks` |
| ayva | `iso_tree_quince_young_se.png` | 30 | `young quince sapling with rounded leaves` |
| ayva | `iso_tree_quince_grown_se.png` | 60 | `small rounded quince tree with grey-green leaves` |
| ayva | `iso_tree_quince_fruit_se.png` | 60 | `quince tree with large knobbly golden-yellow quinces` |
| fındık | `iso_tree_hazel_young_se.png` | 26 | `young hazel shrub with a few stems` |
| fındık | `iso_tree_hazel_grown_se.png` | 52 | `multi-stemmed hazel bush with round leaves` |
| fındık | `iso_tree_hazel_fruit_se.png` | 52 | `hazel bush with clusters of hazelnuts in frilly green husks` |
| kayısı | `iso_tree_apricot_young_se.png` | 32 | `young apricot sapling` |
| kayısı | `iso_tree_apricot_grown_se.png` | 64 | `rounded apricot tree with heart-shaped leaves` |
| kayısı | `iso_tree_apricot_fruit_se.png` | 64 | `apricot tree with small orange apricots` |

- Maliyet 12 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod: `IsoArt.Trees` (`<önek>_young_se`, `_grown_se`, `_fruit_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
