---
id: "832"
title: "Zincir pozu tamamlama 2: erkek güneybatı yoğurma, yön metniyle (1 klip)"
status: pending
approved_by: user        # kullanıcı 21.11.2026: 796 "başka yöntemle bir deneme daha" (seçmeli liste)
priority: 5
family: karakter
style_gate: false
cost_est: 3
cost_max: 6
depends_on: ["589"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_man_r84_knead_sw_<NN>.png
  - art/results/832-*.md
created: 2026-11-21
---
# 832 Erkek güneybatı yoğurma, ikinci deneme (1 klip)

## Amaç
589'da tuvale sığmadı, 796'da sığdı ama hareketsiz çıktı (kare farkı 3-16 px; `art/results/796-*.md`). Bu iş 796 raporunun önerdiği iki yolu sırayla dener.

## Tarif
589/796 ile aynı karakter (`c63fe39d-b168-4732-bf11-b476172c98e8`, man_r84), yön `south-west`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi.

1. **Önce** `mode=pixminimax` + `direction_prompts` (güneybatı için): `both forearms push forward over the trough and pull back repeatedly, shoulders rocking, hands pressing and folding dough, elbows stay close to the body, rhythmic loop`. 1 gen.
2. Kare farkı yine yetersizse (ölçüt aşağıda) **bir kez** `mode=v3` ile aynı metin (~3 gen; çağrı öncesi maliyet satırı okunur, 4'ü aşarsa çağrılmaz).

İkincisi de tutmazsa teslim yok, `failed` ve rapor.

## Ortak işleme
589 ile aynı (84x84, ayak satırı y=81, pivot, 012b paleti); içerik kırpılmaz.

## Doğrulama
Hareket: kare 1'den en az bir karede ≥ 60 px fark ve en az 3 karede ≥ 30 px. 018a ölçütleri; diğer üç yön yoğurma klibiyle yan yana GIF (`art/results/832-man-knead.gif`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
