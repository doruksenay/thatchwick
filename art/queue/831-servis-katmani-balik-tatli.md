---
id: "831"
title: "Servis kapları: balık ve tatlı dolu katmanı yeniden (_v2) (2)"
status: running
claimed_at: 2026-11-20T23:30Z
approved_by: user        # kullanıcı 21.11.2026: 797 "kabul, ara dolulukları kod üretsin" (balık ve tatlı katmanı neredeyse görünmüyordu)
priority: 5
family: esya
style_gate: false
cost_est: 2
cost_max: 6
depends_on: ["797"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_serve_food_fish_4_v2.png
  - Assets/Art/Iso/_aday/iso_serve_food_sweet_4_v2.png
  - art/results/831-*.md
created: 2026-11-21
---
# 831 Balık ve tatlı katmanı yeniden (2)

## Amaç
797'de bu iki dolu katman kabın üstünde neredeyse görünmüyor (balık oval tabakla aynı tonda, tatlı tahtayla aynı tonda). Ara doluluklar (`_3…_1`) üretilmez: kod dolu katmandan porsiyon azaldıkça piksel siler.

## Tarif
797 ile aynı yöntem (arka plan = kap, maske = kabın üst yüzeyi, yalnızca yemek, 32x32, (0,0)). Açıklamalar kontrastı açıkça ister:

| Dosya | Kap | Açıklama |
|---|---|---|
| `iso_serve_food_fish_4_v2.png` | `iso_serve_vessel_platter.png` | `only three whole grilled fish with silver-grey skin, dark grill stripes and a lemon-yellow herb garnish, clearly lighter than the dark wooden platter` |
| `iso_serve_food_sweet_4_v2.png` | `iso_serve_vessel_board.png` | `only rows of small golden honey-glazed pastry pieces with pale cream tops and chopped green pistachio, clearly brighter than the brown board` |

## Doğrulama
797 ile aynı; ek ölçüt: kap+katman birleşiminde yemek 48 px pencerede kaptan ayırt edilmeli. Yan yana: 797 hâli ve `_v2` (`art/results/831-yanyana.png`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
