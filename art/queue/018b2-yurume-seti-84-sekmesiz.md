---
id: "018b2"
title: Yürüme seti yeniden: erkek ve kadın 84 px, 8 yön, sekmesiz
status: running
claimed_at: 2026-11-18T19:40Z
approved_by: user        # kullanıcı 18.11.2026 (sohbet)
priority: 10
family: karakter
style_gate: false
cost_est: 40
cost_max: 60
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_walk2_<yön>_<NN>.png
  - art/results/018b2-yurume-seti-84-sekmesiz.md
created: 2026-11-18
---
# 018b2 Yürüme seti yeniden (sekmesiz)

## Amaç
018b reddedildi: yürürken zıplıyor (kafa 4-8 px oynuyor). Kullanıcı yalnızca bozuk yönleri değil, yürümeyi bütünüyle zıplamalı buldu; set baştan yapılır.

## Tarif
- **Yöntem 018a3'ün raporundaki kazanan yöntemdir** (şablon yürüyüş ya da sekmesiz pixminimax metni). 018a3 `rejected` ise bu iş başlamaz.
- 018b'nin karakter kimlikleri, 8 yön, ortak işleme kuralları aynen; dosya adı `walk2`.
- Çağrılar bağımsız: paralel.

## Doğrulama
- **Sekme ölçütü (bağlayıcı):** her yönde kareler arası kafa (opak bbox tepesi) farkı ≤ 3 px, ayak satırı sabit. Geçmeyen yön teslim edilmez; raporda yön × karakter tablosu.
- 018b doğrulamaları, iki GIF (erkek, kadın; 8 yön ızgara) ve yan yana.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
