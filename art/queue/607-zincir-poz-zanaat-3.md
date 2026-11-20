---
id: "607"
title: "Zincir pozları, zanaat 3/3: yaz, havanda döv, yayık döv, daldır, serp (40)"
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
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_write_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_pound_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_churn_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_dip_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_sprinkle_{se,sw,nw,ne}_<NN>.png
  - art/results/607-*.md
created: 2026-11-20
---
# 607 Zincir pozları, zanaat 3/3: yaz, havanda döv, yayık döv, daldır, serp (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgeleri 60, 63, 63a) adım pozları: yazı kürsüsü, havan, yayık, mum ve boya, tuzlama. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza düşer. Kare gelince kod değişmeden oynar.

**Onay:** ikinci dalga; kullanıcı 21.11.2026 (belge 66g) beklemeden üretilmesini onayladı.

| Anahtar | Nerede oynar |
|---|---|
| `write` | yazı kürsüsünde yazma (kronik, mektup, sözleşme, tarif defteri) |
| `pound` | havanda dövme (baharat, ot, sarımsak) |
| `churn` | yayık dövme |
| `dip` | fitil daldırma (mum), kumaş daldırma (boya kazanı) |
| `sprinkle` | tuz, un, baharat, kül ya da kum serpme |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `write`: `standing at a slanted waist-high desk, writing with a quill in small strokes, pausing to dip the quill to the side, loop`
  - `pound`: `holding a mortar steady with one hand and pounding straight down with a pestle in the other, loop`
  - `churn`: `standing over a tall narrow churn and plunging a long dasher up and down with both hands, loop`
  - `dip`: `lowering something held in both hands down into a waist-high vat, holding a moment, and lifting it out dripping, loop`
  - `sprinkle`: `taking a pinch from the cupped left hand and sprinkling it forward with the right hand in a small arc, loop`
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
