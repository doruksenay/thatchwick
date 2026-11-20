---
id: "748"
title: "Gönül yolu pozları 2: yüzük tak, kurdele kes, kına yak, halay, alay yürüyüşü, testi kır (48)"
status: pending
approved_by: user        # kullanıcı 20.11.2026; Project belgeleri 67b §3.5-3.7, 67c
priority: 40
family: karakter
style_gate: false
cost_est: 48
cost_max: 60
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_put_ring_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_cut_ribbon_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_henna_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_halay_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_procession_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_throw_jar_{se,sw,nw,ne}_<NN>.png
  - art/results/748-*.md
created: 2026-11-20
---
# 748 Gönül yolu pozları 2 (48)

## Amaç
Nişan, kına gecesi ve düğün sahneleri (Project belgesi 67b §3.5-3.7).

| Anahtar | Nerede |
|---|---|
| `put_ring` | nişan: karşısındakinin parmağına yüzük takma |
| `cut_ribbon` | nişan: aile büyüğü makasla kurdeleyi keser |
| `henna` | kına gecesi: avuca kına sürme (sürende; gelin `sit` pozunda avucunu açar) |
| `halay` | düğün: el ele omuz omuza halay adımı (yan yana dizilen karakterler aynı klibi oynar) |
| `procession` | gelin alma alayında neşeli yürüyüş, el sallama |
| `throw_jar` | gelin eşikte testiyi yere atıp kırar |

## Tarif
- 572 ile aynı karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8`, kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d`; 84x84.
- Her anahtar için 2 karakter x 4 yön = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 6 x 8 = 48 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `put_ring`: `gently holding someone's hand forward with one hand and sliding a small ring onto a finger with the other, smiling`
  - `cut_ribbon`: `holding small scissors and snipping a red ribbon held at chest height, then smiling proudly`
  - `henna`: `crouching slightly and dabbing a paste from a small bowl onto an open palm in front, careful, loop`
  - `halay`: `folk line dance step: arms linked at shoulder height, stepping side to side and kicking lightly in rhythm, loop`
  - `procession`: `walking cheerfully in place while waving one hand above the head, festive, loop`
  - `throw_jar`: `lifting a small clay jug overhead with both hands and throwing it down to the ground in front, joyful`
- Bağımsız çağrılar paralel gönderilir.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen.

## Doğrulama
018a ölçütleri her klip için; `halay`'da kollar omuz hizasında ve yan komşuya uzanır görünmeli. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
