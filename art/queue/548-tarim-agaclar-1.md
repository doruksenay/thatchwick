---
id: "548"
title: "Yeni ağaçlar 1/4: dut, incir, nar, ceviz (12)"
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
  - Assets/Art/Iso/_aday/iso_tree_mulberry_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_mulberry_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_mulberry_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_fig_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_fig_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_fig_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_pomegranate_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_pomegranate_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_pomegranate_fruit_se.png
  - Assets/Art/Iso/_aday/iso_tree_walnut_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_walnut_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_walnut_fruit_se.png
created: 2026-11-20
---
# 548 Yeni ağaçlar 1/4: dut, incir, nar, ceviz (12)

## Amaç
Project belgesi 59 §6.1: yeni meyve ağaçları ve çalılar. Üç hâl: genç, olgun, meyveli (meyve mevsiminde çizilir).

## Tarif
Yöntem **372 ve 377 ile aynı**: `create_map_object` stil eşleme, tuval 96x112, pivot 40,24, bitki referansları, 377'deki küçük gövde kılavuzu ve `, the trunk ends directly on the transparent background, no base plate, no turf, no diamond` eki; üretimden sonra `style.py fit --family bitki`, ardından `check`.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| dut | `iso_tree_mulberry_young_se.png` | 36 | `young mulberry sapling with heart-shaped leaves` |
| dut | `iso_tree_mulberry_grown_se.png` | 72 | `broad mulberry tree with dense heart-shaped leaves` |
| dut | `iso_tree_mulberry_fruit_se.png` | 72 | `broad mulberry tree with many dark purple-black berries among the leaves` |
| incir | `iso_tree_fig_young_se.png` | 32 | `young fig sapling with large lobed leaves` |
| incir | `iso_tree_fig_grown_se.png` | 64 | `low spreading fig tree with pale grey bark and large lobed leaves` |
| incir | `iso_tree_fig_fruit_se.png` | 64 | `spreading fig tree with purple-green figs on the branches` |
| nar | `iso_tree_pomegranate_young_se.png` | 30 | `young pomegranate shrub with small glossy leaves` |
| nar | `iso_tree_pomegranate_grown_se.png` | 60 | `small bushy pomegranate tree with small glossy leaves` |
| nar | `iso_tree_pomegranate_fruit_se.png` | 60 | `pomegranate tree with round red pomegranates` |
| ceviz | `iso_tree_walnut_young_se.png` | 46 | `young walnut sapling with long compound leaves` |
| ceviz | `iso_tree_walnut_grown_se.png` | 92 | `tall broad walnut tree with a thick grey trunk` |
| ceviz | `iso_tree_walnut_fruit_se.png` | 92 | `tall walnut tree with green round walnut husks` |

- Maliyet 12 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod: `IsoArt.Trees` (`<önek>_young_se`, `_grown_se`, `_fruit_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
