---
id: "747"
title: "Gönül yolu pozları 1: öp, yanaktan öp, el tut, mendil uzat, kahve ikram et, tuzlu kahveyi iç (48)"
status: running
claimed_at: 2026-11-21T03:23Z
approved_by: user        # kullanıcı 20.11.2026 ("gönül yolu ... asset'leri şimdi kuyruğa alınsın: alınsın"); Project belgeleri 67b §3, 67c
priority: 40
family: karakter
style_gate: false
cost_est: 48
cost_max: 60
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_kiss_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_kiss_cheek_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_hold_hands_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_offer_handkerchief_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_serve_coffee_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_drink_grimace_{se,sw,nw,ne}_<NN>.png
  - art/results/747-*.md
created: 2026-11-20
---
# 747 Gönül yolu pozları 1 (48)

## Amaç
Gönül yolu (Flört → Sevgili → İsteme → Nişan → Düğün; Project belgesi 67b §3). Kod pozu adla arar; iki kişilik pozlarda (`kiss`, `hold_hands`) iki karakter yan yana karolarda yüz yüze döner ve aynı anda oynar.

| Anahtar | Nerede |
|---|---|
| `kiss` | öpüşme (romantik İlgili ve üstü); sakallıysa "Oops" iç sesi |
| `kiss_cheek` | yanaktan öpme |
| `hold_hands` | el ele durma/yürüme öncesi; uzanan el |
| `offer_handkerchief` | sevgili olma sahnesi: işlemeli mendili iki eliyle uzatma |
| `serve_coffee` | istemede tepsiyle kahve ikram etme |
| `drink_grimace` | tuzlu kahveyi içerken yüzünü buruşturmamaya çalışma |

## Tarif
- 572 ile aynı karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84.
- Her anahtar için 2 karakter x 4 yön = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 6 x 8 = 48 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `kiss`: `leaning forward and slightly tilting the head for a gentle kiss, eyes closed, then leaning back, standing`
  - `kiss_cheek`: `leaning in and giving a quick peck on the cheek, then stepping back shyly, standing`
  - `hold_hands`: `reaching one hand forward at waist height as if holding someone's hand, swaying gently, loop`
  - `offer_handkerchief`: `holding out a small folded embroidered handkerchief with both hands, shy, slightly bowing the head`
  - `serve_coffee`: `holding a small round tray with a tiny coffee cup, bending slightly forward to offer it`
  - `drink_grimace`: `sipping from a tiny coffee cup, then forcing a polite smile while the face twitches, loop`
- Ton: cozy, sıcak; PEGI 12 (öpücük kısa ve masum).
- Bağımsız çağrılar paralel gönderilir.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen.

## Doğrulama
018a ölçütleri her klip için. Elde tutulan mendil, tepsi ve fincan küçük ama okunur. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
