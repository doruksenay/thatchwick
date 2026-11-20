---
id: "605"
title: "Zincir pozları, zanaat 1/3: testere, dikiş, eğir, doku, körük (40)"
status: pending
approved_by: user        # kullanıcı 21.11.2026 (belge 66g): ikinci dalga beklemeden üretilsin
priority: 45
family: karakter
style_gate: false
cost_est: 40
cost_max: 52
depends_on: ["018a3", "571", "572"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_saw_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_sew_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_spin_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_weave_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_bellows_{se,sw,nw,ne}_<NN>.png
  - art/results/605-*.md
created: 2026-11-20
---
# 605 Zincir pozları, zanaat 1/3: testere, dikiş, eğir, doku, körük (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgeleri 60, 63, 63a) adım pozları: marangoz, terzi, çıkrık, dokuma tezgâhı, demirci ve camcı. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza düşer. Kare gelince kod değişmeden oynar.

**Onay:** ikinci dalga; kullanıcı 21.11.2026 (belge 66g) beklemeden üretilmesini onayladı.

| Anahtar | Nerede oynar |
|---|---|
| `saw` | tahta biçme (marangoz tezgâhı, yarma kütüğü) |
| `sew` | dikiş ve nakış (dikiş sehpası, saraç tezgâhı, yama) |
| `spin` | çıkrıkta ve kirmanla iplik eğirme |
| `weave` | dokuma tezgâhı |
| `bellows` | körük çekme (demir ocağı, cam ocağı, çiçek ocağı) |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `saw`: `bracing a plank on a waist-high bench with one hand and sawing back and forth with a hand saw in the other, loop`
  - `sew`: `sitting posture with cloth on the lap, pushing a needle through and pulling the thread up and out in a long arc, loop`
  - `spin`: `sitting posture, one hand drawing out a strand of wool to the side while the other hand turns a wheel in small circles, loop`
  - `weave`: `sitting posture at a loom, throwing a shuttle from one hand to the other and then pulling a beater bar toward the body, loop`
  - `bellows`: `standing sideways and pumping a long bellows handle down and up with both hands, loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, karakterin giysisi ve saçı durma karesiyle aynı. Elde tutulan nesne okunur ama küçük olmalı: oyunda taşınan eşya ayrıca el çapasına çizilir. Klip başına GIF `art/results/` altına.

## Notlar
- Giysi katmanları (43c, 509 planı): bu kareler gövde setidir; giysi ailelerine aktarım 509'daki toplu parti yöntemiyle ayrı iştir.
- El konumları: her klibin ilk karesindeki tutan elin konumu rapora yazılır (`hand_anchors.txt` için).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
