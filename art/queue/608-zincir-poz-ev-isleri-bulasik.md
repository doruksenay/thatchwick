---
id: "608"
title: "Zincir pozları, ev işleri ve bulaşık: süpür, ovala, bulaşık yıka, badana, çiğne (40)"
status: pending
approved_by: pending     # BEKLETİLİYOR (63a-K9, K10): kullanıcı ilk dalga pozları (565, 566, 570-577) Unity'de görünce "user" yapılır; o zamana dek çalışmaz
priority: 45
family: karakter
style_gate: false
cost_est: 40
cost_max: 52
depends_on: ["018a3", "571", "572"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_sweep_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_scrub_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_wash_dishes_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_whitewash_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_tread_{se,sw,nw,ne}_<NN>.png
  - art/results/608-*.md
created: 2026-11-20
---
# 608 Zincir pozları, ev işleri ve bulaşık: süpür, ovala, bulaşık yıka, badana, çiğne (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgeleri 60, 63, 63a) adım pozları: bulaşık (63a §4), çamaşır, süpürme, badana, üzüm teknesi. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza düşer. Kare gelince kod değişmeden oynar.

**Bekletme:** bu iş ikinci dalgadır. Kullanıcı kararı (63a-K10): önce ilk dalga pozları (565, 566, 570-577) üretilip Unity'de görülecek. Kullanıcı onay verince `approved_by: user` yapılır.

| Anahtar | Nerede oynar |
|---|---|
| `sweep` | yer süpürme (Ev görevi, hizmetçi, ahır) |
| `scrub` | dere kıyısında ya da yerdeki leğende bulaşık ve çamaşır ovalama; yer silme |
| `wash_dishes` | taş yalakta ya da tezgâhtaki leğende ayakta bulaşık yıkama |
| `whitewash` | badana, boyama, cila sürme |
| `tread` | üzüm teknesinde çiğneme, kil ve kerpiç çamuru çiğneme |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `sweep`: `sweeping the floor with a long twig broom in short diagonal strokes, stepping slowly, loop`
  - `scrub`: `crouching low and scrubbing an object held on the ground with a circular motion of one hand, then dunking it forward, loop`
  - `wash_dishes`: `standing at a waist-high basin, scrubbing a bowl held in one hand with the other, dipping it and setting it aside, loop`
  - `whitewash`: `brushing a wall with a wide brush in long up-and-down strokes at head height, dipping the brush low to the side, loop`
  - `tread`: `treading in place with knees lifting alternately, hands holding up the hem of the clothing, loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, karakterin giysisi ve saçı durma karesiyle aynı. Elde tutulan nesne okunur ama küçük olmalı: oyunda taşınan eşya ayrıca el çapasına çizilir. Klip başına GIF `art/results/` altına.

## Notlar
- Giysi katmanları (43c, 509 planı): bu kareler gövde setidir; giysi ailelerine aktarım 509'daki toplu parti yöntemiyle ayrı iştir.
- El konumları: her klibin ilk karesindeki tutan elin konumu rapora yazılır (`hand_anchors.txt` için).
- `scrub` ve `wash_dishes` bulaşık zincirinin iki yolu içindir: derede çömelerek, evde ayakta.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
