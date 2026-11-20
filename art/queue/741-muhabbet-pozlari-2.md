---
id: "741"
title: "Muhabbet pozları 2: anlat, dedikodu fısılda, şaşır, alkışla, laf sok, düşün (48)"
status: pending
approved_by: user        # kullanıcı 20.11.2026; Project belgeleri 67a §6, 67b
priority: 40
family: karakter
style_gate: false
cost_est: 48
cost_max: 60
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_tell_story_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_gossip_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_gasp_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_clap_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_scoff_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_think_{se,sw,nw,ne}_<NN>.png
  - art/results/741-*.md
created: 2026-11-20
---
# 741 Muhabbet pozları 2 (48)

## Amaç
Toplu sohbette konuşanın ve dinleyenin ikinci poz seti (Project belgesi 67a §6). 018e'deki `social` (el kol hareketiyle konuşma) bunlara ek olarak kalır.

| Anahtar | Ne |
|---|---|
| `tell_story` | fıkra ya da hikâye anlatma: geniş el hareketleri |
| `gossip` | öne eğilip elini ağzının yanına koyarak fısıldama |
| `gasp` | dedikoduya şaşırma: eli göğsünde, ağzı açık |
| `clap` | türkü ya da fıkra sonunda alkış |
| `scoff` | laf sokma: göz devirip elini savurma |
| `think` | eli çenesinde düşünme |

## Tarif
- 572 ile aynı karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8`, kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d`; 84x84.
- Her anahtar için 2 karakter x 4 yön = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 6 x 8 = 48 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `tell_story`: `telling a funny story with big animated hand gestures, standing, loop`
  - `gossip`: `leaning forward and whispering with one hand raised beside the mouth, glancing sideways, loop`
  - `gasp`: `gasping in surprise with one hand on the chest and mouth open, leaning back slightly, short`
  - `clap`: `clapping hands cheerfully at chest height, standing, loop`
  - `scoff`: `rolling the eyes and flicking one hand dismissively, smirking, standing, short`
  - `think`: `thinking with one hand on the chin, head tilted, standing, loop`
- Bağımsız çağrılar paralel gönderilir.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen.

## Doğrulama
018a ölçütleri her klip için. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
