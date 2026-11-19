---
id: "018b3"
title: Yürüme seti, kalan 12 yön (sekmesiz)
status: review
claimed_at: 2026-11-19T07:31Z
approved_by: user        # kullanıcı 19.11.2026: "önerilerini yap"
priority: 10
family: karakter
style_gate: false
cost_est: 40
cost_max: 55
depends_on: ["018b2"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_walk2_<yön>_<NN>.png
  - art/results/018b3-yurume-kalan-yonler.md
created: 2026-11-19
---
# 018b3 Yürüme: kalan 12 yön

## Amaç
018b2'de yalnız erkek se/e/w ve kadın e sekme ölçütünü geçti, bunlar kabul edildi. Bu iş kalan yönleri tamamlar:
- erkek: n, ne, nw, s, sw
- kadın: n, ne, nw, s, se, sw, w

## Tarif
018b2'nin tarifi, işleme kuralları ve sekme ölçütü aynen (kafa tepesi farkı ≤ 3 px, kırpma yok). Var olan 4 yönün dosyalarına dokunulmaz.
- Her yön önce bir kez üretilir. Ölçütü geçmeyen yön için bütçe izin verirse bir kez daha denenir (yön başına en çok 2 deneme).
- Yine geçmeyen yön teslim edilmez; işlenmiş kareleri `art/results/018b3-red/` altına, nedeni sonuç dosyasına yazılır.

## Doğrulama
018b2 ile aynı. GIF'ler kabul edilmiş 4 yönle birlikte 8 yönlü ızgara olarak (erkek, kadın).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
