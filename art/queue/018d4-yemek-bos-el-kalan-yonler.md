---
id: "018d4"
title: Boş elle yeme, kalan 4 yön + el konumu
status: running
claimed_at: 2026-11-19T12:09Z
approved_by: user        # kullanıcı 19.11.2026: "uygula dediklerini"
priority: 10
family: karakter
style_gate: false
cost_est: 10
cost_max: 20
depends_on: ["018d3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_eat3_<yön>_<NN>.png
  - art/results/018d4-hand-anchors.txt
  - art/results/018d4-yemek-bos-el-kalan.md
created: 2026-11-19
---
# 018d4 Boş elle yeme: kalan 4 yön

## Amaç
018d3'te kabul edilmeyen yönler: erkek sw, kadın se, ne, sw. Kadın se ve ne'de elde ekmek çizildi; iki sw'de baş 3 px oynadı.

## Tarif
018d3'ün tarifi, el konumu dosyası biçimi ve doğrulaması aynen. El konumları yalnız bu yönler için `art/results/018d4-hand-anchors.txt` dosyasına yazılır.
- Kadında elde nesne çıkmasın diye: `... raises one open empty hand, fingers slightly curled, the hand is completely empty, no bread, no food, no object ...` ve negatif tarif destekleniyorsa `bread, food, object in hand`.
- sw için baş ölçütü: `head and shoulders stay perfectly still`.
- Yön başına en çok 2 deneme.

## Doğrulama
018d3 ile aynı: baş farkı ≤ 2 px, elde nesne yok, el konumu sayfası (elma %35 bindirilmiş), GIF.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
