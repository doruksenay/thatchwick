---
id: "606"
title: "Zincir pozları, zanaat 2/3: çark, cam üfle, rendele, yont, kazı (40)"
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
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_wheel_throw_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_blow_glass_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_plane_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_chisel_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_scrape_{se,sw,nw,ne}_<NN>.png
  - art/results/606-*.md
created: 2026-11-20
---
# 606 Zincir pozları, zanaat 2/3: çark, cam üfle, rendele, yont, kazı (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgeleri 60, 63, 63a) adım pozları: çömlekçi, camcı, marangoz, taşçı, tabak. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza düşer. Kare gelince kod değişmeden oynar.

**Onay:** ikinci dalga; kullanıcı 21.11.2026 (belge 66g) beklemeden üretilmesini onayladı.

| Anahtar | Nerede oynar |
|---|---|
| `wheel_throw` | çömlekçi çarkında şekil verme |
| `blow_glass` | cam üfleme |
| `plane` | tahta rendeleme |
| `chisel` | taş ve ahşap yontma |
| `scrape` | deri kazıma, tahta zımparalama, post temizleme |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `wheel_throw`: `sitting posture leaning forward over a low wheel, both wet hands cupped around a rising lump of clay, one foot kicking the wheel, loop`
  - `blow_glass`: `holding a long blowpipe up to the mouth with both hands and blowing while slowly rotating it, then lowering it`
  - `plane`: `leaning over a waist-high bench and pushing a wooden hand plane forward with both hands in long strokes, then drawing it back, loop`
  - `chisel`: `holding a chisel against a block at waist height with one hand and tapping it with a mallet in the other, short rhythmic strokes, loop`
  - `scrape`: `leaning forward and scraping a stretched hide downward with a two-handled blade in long pulls, loop`
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
