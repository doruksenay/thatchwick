---
id: "551"
title: "Yeni ağaçlar 4/4: turunç, limon, keçiboynuzu (9)"
status: running
claimed_at: 2026-11-20T14:42Z
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
  - Assets/Art/Iso/_aday/iso_tree_bitter_orange_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_bitter_orange_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_bitter_orange_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_lemon_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_lemon_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_lemon_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_carob_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_carob_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_carob_fruit_se.png
created: 2026-11-20
---
# 551 Yeni ağaçlar 4/4: turunç, limon, keçiboynuzu (9)

## Amaç
Project belgesi 59 §6.1: yeni meyve ağaçları ve çalılar. Üç hâl: genç, olgun, meyveli (meyve mevsiminde çizilir).

## Tarif
Yöntem **372 ve 377 ile aynı**: `create_map_object` stil eşleme, tuval 96x112, pivot 40,24, bitki referansları, 377'deki küçük gövde kılavuzu ve `, the trunk ends directly on the transparent background, no base plate, no turf, no diamond` eki; üretimden sonra `style.py fit --family bitki`, ardından `check`.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| turunç | `iso_tree_bitter_orange_young_se.png` | 30 | `young citrus sapling with glossy dark leaves` |
| turunç | `iso_tree_bitter_orange_grown_se.png` | 60 | `small rounded bitter orange tree with glossy dark leaves` |
| turunç | `iso_tree_bitter_orange_fruit_se.png` | 60 | `bitter orange tree with rough-skinned orange fruits` |
| limon | `iso_tree_lemon_young_se.png` | 28 | `young lemon sapling with glossy leaves` |
| limon | `iso_tree_lemon_grown_se.png` | 56 | `small lemon tree with glossy leaves` |
| limon | `iso_tree_lemon_fruit_se.png` | 56 | `lemon tree with pale yellow lemons` |
| keçiboynuzu | `iso_tree_carob_young_se.png` | 36 | `young carob sapling with leathery round leaflets` |
| keçiboynuzu | `iso_tree_carob_grown_se.png` | 72 | `broad dark carob tree with leathery leaves` |
| keçiboynuzu | `iso_tree_carob_fruit_se.png` | 72 | `carob tree with long dark brown hanging pods` |

- Maliyet 9 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod: `IsoArt.Trees` (`<önek>_young_se`, `_grown_se`, `_fruit_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
