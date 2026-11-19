---
id: "018b4"
title: Yürüme seti, son 5 yön (sekmesiz)
status: running
claimed_at: 2026-11-19T12:09Z
approved_by: user        # kullanıcı 19.11.2026: "uygula dediklerini"
priority: 10
family: karakter
style_gate: false
cost_est: 20
cost_max: 35
depends_on: ["018b3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_walk2_<yön>_<NN>.png
  - art/results/018b4-yurume-son-5-yon.md
created: 2026-11-19
---
# 018b4 Yürüme: son 5 yön

## Amaç
018b2 ve 018b3'te kabul edilmeyen yönler: erkek sw; kadın n, ne, se, sw. 018b3'te bu yönler iki denemede de sekme ölçütünü geçmedi, yöntem değişir.

## Tarif
018b3'ün kimlikleri, işleme kuralları ve ölçütü aynen (kafa tepesi farkı ≤ 3 px, kırpma yok). Var olan yönlere dokunulmaz.
- `action_description` sekmeyi bastıran biçimde: `walking slowly with small steps, upper body and head stay perfectly level, no bouncing, feet stay close to the ground`.
- Kadın n'de tepe kırpılması oldu: kare ayak hizasına çekildikten sonra kafa tuvalden taşıyorsa üretim yeniden yapılır, tuval büyütülmez.
- Yön başına en çok 2 deneme. Hâlâ geçmeyen yönde, iki denemeden kafa farkı en küçük olanın kareleri `art/results/018b4-red/` altına konur ve sonuç dosyasına yazılır.

## Doğrulama
018b3 ile aynı. GIF: 8 yönlü ızgara (kabul edilmiş yönlerle birlikte), erkek ve kadın.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
