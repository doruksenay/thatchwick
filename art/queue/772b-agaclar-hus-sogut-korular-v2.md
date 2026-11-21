---
id: "772b"
title: "Ağaç stil birliği: huş, söğüt (genç, olgun) ve yaprak döken koru (a) (_v2)"
status: review   # kısmi: huş olgun + genç söğüt teslim; genç huş (boy), olgun söğüt ve koru a (zemin karosu) teslim edilmedi, bkz. rapor
claimed_at: 2026-11-21T00:15Z
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
  - Assets/Art/Iso/_aday/iso_edge_trees_a_v2.png
  - art/results/772b-*.md
created: 2026-11-21
---
# 772b Huş, söğüt ve korular

## Amaç
772 pilotunun stil ve ölçeğiyle kalan yaban ağaçları ve harita kenarı/köy korularının üç dekoru. "Genç koru" (`iso_edge_trees_d`, parlak yeşil çizgi film konileri) kullanıcının işaretlediği en belirgin stil kırığıydı.

## Tarif
**772 tarifi** (stil çapası, yasaklar, yöntem, ortak ek, `style.py fit`). **Stil çapası artık 772'nin kabul edilen `iso_tree_oak_grown_se_v2` ve `iso_tree_apple_grown_se_v2` görselleridir.** 772'de `create_map_object` stil görseli almadı; araç referans görseli (ör. stil görseli ya da `color_image` paleti) kabul ediyorsa bu ikisi verilir, etmiyorsa en azından `color_image` = `iso_tree_oak_grown_se_v2`. İğne yapraklı korular (b, d) çamla birlikte 772c'de. Farklar:
- huş olgun: `mature birch tree, white bark with dark marks, tall slim trunk, light airy crown of small rounded leaf clusters` · huş genç: `young birch, thin white trunk, few small leaf clusters`
- söğüt olgun: `mature willow tree, drooping long thin branches made of small leaf clusters, muted grey green, thick short trunk` · söğüt genç: `young willow, thin trunk, few drooping branches`. Genç söğüt eski görseldeki mavi-camgöbeği tona kaymaz.
- Tuval 112x160, pivot (56, 24); boy 772 gibi (olgun 120-140, genç 60-85).
- **Koru a** (eşya `art` görseli; tuval 112x160, pivot (56, 24)):
  - `a` (tek iri yaprak döken ağaç öbeği): `a cluster of two broadleaf trees, one large one small, crowns of many small rounded leaf clusters`; olgun yaban boyunda (120-140).
- 5 görsel ≈ 5-7 gen.

## Doğrulama
772 doğrulaması (stil sayfası `772b-stil.png`: yeni görseller + 772'nin meşe ve çamı + eski `edge_trees_d`).

## Notlar
Kod: korular (`iso_edge_trees_*`) için `_v2` varsa o çizilir, pivot (w/2, 24) (`IsoArt.Item`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
