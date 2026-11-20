---
id: "357t"
title: "Eşya düzeltmeleri (7 parça: kompost, 2 kaya, hela, adaçayı, boyalı kiriş, köy güvercinliği)"
status: running
claimed_at: 2026-11-20T11:20Z
approved_by: user        # kullanıcı kararı 2026-11-20 (sohbette eşya incelemesi: "kabul et, yalnız 7 parçayı düzelt")
priority: 18
family: esya
style_gate: true
cost_est: 10
cost_max: 20
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_compost_heap_se_v2.png
  - Assets/Art/Iso/_aday/iso_terrain_boulder_se_v2.png
  - Assets/Art/Iso/_aday/iso_terrain_boulder_large_2x1_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_flush_privy_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_sage_se_v2.png
  - Assets/Art/Iso/_aday/iso_ceiling_item_painted_beam_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_dovecote_village_se_v2.png
created: 2026-11-20
---
# 357t Eşya düzeltmeleri

## Neden
Eşya incelemesinde (2026-11-20) bütün işler kabul edildi, ama bu 7 parça okunmuyor ya da yanlış görünüyor.

## Tarif
Her parça kendi kaynak işinin tarifini (ayak izi, tuval, kamera, palet) aynen kullanır.

| Parça | Kaynak | Ne olmalı |
|---|---|---|
| `compost_heap` | 321 | Ahşap çıtalı, üstü açık bir bölme içinde kahve-yeşil yığın: yarı çürümüş yaprak, saman ve sebze artıkları; üstünde bir dirgen. Mor ya da dikenli bir yığın olmamalı. |
| `terrain_boulder` | 357 | Doğal, yuvarlatılmış, düzensiz gri kaya; bir iki çatlak, dibinde yosun ve ot. Küp ya da kutu biçimi olmamalı. |
| `terrain_boulder_large_2x1` | 357 | İki-üç iri yuvarlak kayadan oluşan öbek, düzensiz siluet, likenli. Küp olmamalı. |
| `flush_privy` | 353 | Akarsulu hela: dere ya da ark üstüne kurulmuş küçük tahta kulübe. Kapısında ay ya da yıldız oyuğu, altından su akan taş oluk görünür. Kapalı bir sandık gibi olmamalı. |
| `forage_sage` | 424k | Adaçayı öbeği: gri-yeşil, kadifemsi, uzun oval yapraklar; birkaç mor-lila çiçek başağı. Mavi kristal ya da mücevher gibi olmamalı. |
| `ceiling_item_painted_beam` | 440 | Tavana yatay uzanan ahşap kiriş; yan yüzünde kırmızı-mavi-sarı küçük tekrarlı bir boya bezeme (zikzak, rozet). Gri bir leke olmamalı; kiriş biçimi net olmalı. |
| `dovecote_village` | 442 | Köy güvercinliği: yuvarlak ya da sekizgen alçak taş kule, konik saz ya da kiremit çatı, üst kısımda sıra sıra güvercin delikleri, çatıda bir iki güvercin. Baca gibi olmamalı. |

## Doğrulama
Kaynak işlerin doğrulaması. Yan yana sayfası: `art/results/357t-yanyana.png`. Eski hâli ile v2 yan yana.

## Notlar
