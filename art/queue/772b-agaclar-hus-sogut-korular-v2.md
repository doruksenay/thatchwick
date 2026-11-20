---
id: "772b"
title: "Ağaç stil birliği: huş, söğüt (genç, olgun) ve üç koru dekoru (_v2)"
status: pending
approved_by: user        # kullanıcı 21.11.2026: ağaç stil birliği (kabarık kümeli taç), hepsi tek seferde
priority: 12
family: bitki
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["772"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_birch_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_willow_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_edge_trees_{a,b,d}_v2.png
  - art/results/772b-*.md
created: 2026-11-21
---
# 772b Huş, söğüt ve korular

## Amaç
772 pilotunun stil ve ölçeğiyle kalan yaban ağaçları ve harita kenarı/köy korularının üç dekoru. "Genç koru" (`iso_edge_trees_d`, parlak yeşil çizgi film konileri) kullanıcının işaretlediği en belirgin stil kırığıydı.

## Tarif
**772 tarifi** (stil çapası, yasaklar, yöntem, ortak ek, `style.py fit`). Stil referansı olarak 772'nin kabul edilen (ya da review) yeni olgun meşesi ve olgun çamı da eklenir. Farklar:
- huş olgun: `mature birch tree, white bark with dark marks, tall slim trunk, light airy crown of small rounded leaf clusters` · huş genç: `young birch, thin white trunk, few small leaf clusters`
- söğüt olgun: `mature willow tree, drooping long thin branches made of small leaf clusters, muted grey green, thick short trunk` · söğüt genç: `young willow, thin trunk, few drooping branches`. Genç söğüt eski görseldeki mavi-camgöbeği tona kaymaz.
- Tuval 112x160, pivot (56, 24); boy 772 gibi (olgun 120-140, genç 60-85).
- **Korular** (eşya `art` görseli; tuval 112x160, pivot (56, 24)):
  - `a` (tek iri yaprak döken ağaç öbeği): `a cluster of two broadleaf trees, one large one small, crowns of many small rounded leaf clusters`; olgun yaban boyunda (120-140).
  - `b` (çam öbeği): `a group of three pine trees of different heights, layered textured needle branches`; en uzunu 120-140.
  - `d` ("Genç koru"): `a small grove of four young trees, two young pines and two young broadleaf saplings, layered textured needles and small leaf clusters`; genç boyunda (60-85). Parlak limon yeşili ve düzgün koniler yasak.
- 7 görsel ≈ 7-8 gen.

## Doğrulama
772 doğrulaması (stil sayfası `772b-stil.png`: yeni görseller + 772'nin meşe ve çamı + eski `edge_trees_d`).

## Notlar
Kod: korular (`iso_edge_trees_*`) için `_v2` varsa o çizilir, pivot (w/2, 24) (`IsoArt.Item`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
