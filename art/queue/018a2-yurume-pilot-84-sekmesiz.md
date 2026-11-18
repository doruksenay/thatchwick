---
id: "018a2"
title: Yürüme pilotu yeniden, erkek 84 px SE (dikey sekme yok)
status: failed
claimed_at: 2026-11-18T19:12Z
approved_by: user        # kullanıcı 18.11.2026: 018a reddedildi ("yürürken zıplıyor")
priority: 5
family: karakter
style_gate: true
cost_est: 8
cost_max: 20
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_man_r84_walk2_se_<NN>.png
  - art/results/018a2-yurume-pilot-84-sekmesiz.md
created: 2026-11-18
---
# 018a2 Yürüme pilotu yeniden: sekmesiz

## Amaç
018a reddedildi: karakter yürürken zıplıyor. Ölçüm (018a kareleri): ayak satırı sabit (y=82), tepe 2-10 px arasında oynuyor; 84 px boyda 8 px dikey sekme. Gerçek yürüyüşte baş en çok 2-3 px oynar. Bu pilot yöntemi düzeltir; 018b-e ve 368 bunun sonucunu kullanır.

## Tarif
018a'nın "Ortak işleme" kuralları aynen (karakter kimlikleri, tuval 84x84, pivot x=42 y=2, palet, dosya adı; ad `walk2`). Yalnızca erkek, yalnızca SE. İki yol denenir, ucuz olan önce:
1. **Şablon yürüyüş:** `animate_character` iskelet şablonu (`walk`/`walking`), aynı karakter ve yön. Şablon yürüyüşler genelde baş hizasını korur.
2. Şablon yoksa ya da tepe ölçütünü geçmezse **pixminimax yeniden:** `action_description="walking at a calm steady pace, smooth gliding stride, head and shoulders stay level, no bouncing, no hopping, feet stay on the ground"`, `frame_count=8`, `keep_first_frame=true`, `end_frame` SE durma karesi.

Her yolda üretimden sonra **ücretsiz düzeltme** denenebilir: tepe oynaması 4-5 px ise üst gövdeyi kaydırmak yasak (kimlik bozulur); yalnızca bütün kareyi kaydırmak da yasak (ayak kayar). Düzeltilemiyorsa teslim edilmez.

## Doğrulama
- **Sekme ölçütü (yeni, bağlayıcı):** her karenin opak bbox tepesi ile 00'ın tepesi arasındaki fark en çok **3 px**; ayak satırı (bbox altı) bütün karelerde aynı. Raporda kare kare tablo.
- 018a doğrulamaları (validate_iso, style check, GIF, yan yana, kimlik kayması).
- Yan yana sayfasında 018a GIF'i ile yeni GIF yan yana.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
