---
id: "603"
title: "Zincir pozları, sosyal 1/2: sarıl, uzat, gül, bebek tut, diz çök (40)"
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
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_hug_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_give_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_laugh_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_hold_baby_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_kneel_{se,sw,nw,ne}_<NN>.png
  - art/results/603-*.md
created: 2026-11-20
---
# 603 Zincir pozları, sosyal 1/2: sarıl, uzat, gül, bebek tut, diz çök (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgeleri 60, 63, 63a) adım pozları: 37 sosyal işin ortak hareketleri. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza düşer. Kare gelince kod değişmeden oynar.

**Bekletme:** bu iş ikinci dalgadır. Kullanıcı kararı (63a-K10): önce ilk dalga pozları (565, 566, 570-577) üretilip Unity'de görülecek. Kullanıcı onay verince `approved_by: user` yapılır.

| Anahtar | Nerede oynar |
|---|---|
| `hug` | sarılma, teselli, kavuşma |
| `give` | hediye verme, ikram, tabak uzatma |
| `laugh` | şaka, sohbet, eğlence |
| `hold_baby` | bebek besleme, ninni, kucakta sallama |
| `kneel` | evlenme teklifi, çocukla göz hizasında konuşma, yaralıya bakma |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `hug`: `stepping forward, opening both arms and closing them in a warm embrace, holding for a moment, then releasing`
  - `give`: `holding a small object in both hands and offering it forward at chest height with a slight nod, then lowering the hands`
  - `laugh`: `throwing the head back and laughing heartily with shaking shoulders, one hand on the belly, loop`
  - `hold_baby`: `cradling a swaddled baby in both arms against the chest and rocking gently side to side, loop`
  - `kneel`: `going down on one knee, raising one open hand forward, holding the pose, then rising`
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
