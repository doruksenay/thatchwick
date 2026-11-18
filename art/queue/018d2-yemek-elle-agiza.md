---
id: "018d2"
title: Yemek animasyonu yeniden: eliyle ağzına götürür (erkek ve kadın, 4 yön)
status: review
claimed_at: 2026-11-18T19:22Z
approved_by: user        # kullanıcı 18.11.2026 (sohbet)
priority: 10
family: karakter
style_gate: false
cost_est: 10
cost_max: 20
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_eat2_{se,sw,nw,ne}_<NN>.png
  - art/results/018d2-yemek-elle-agiza.md
created: 2026-11-18
---
# 018d2 Yemek: eliyle ağzına götürür

## Amaç
018d'deki `eat` animasyonunda karakter başını yemeğe doğru indiriyor (erkek SE'de kafa 5 px iniyor). Kullanıcı: "yemek yerken direkt ağzına gitmesin, eliyle ağzına götürsün". Baş dik ve sabit kalır, yemeği el kaldırır.

## Tarif
018d'nin karakter kimlikleri, yönleri (se, sw, nw, ne) ve ortak işleme kuralları aynen; dosya adı `eat2`.
- `animate_character`, `pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` ilgili yönün durma karesi.
- `action_description="standing upright, head stays still and level, raises a piece of bread with one hand up to the mouth, takes a bite, lowers the hand back to waist, the head does not lean forward"`.

## Doğrulama
- **Baş ölçütü (bağlayıcı):** kareler arası kafa (opak bbox tepesi) farkı ≤ 2 px; elin ağız hizasına çıktığı en az bir kare olmalı (yan yana sayfasında işaretle).
- 018d doğrulamaları; GIF (her yön) ve 018d'deki `eat` GIF'iyle yan yana.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
