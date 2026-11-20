---
id: "888"
title: "Çocuk: kıpır kıpır durma ve koşu (24)"
status: pending
approved_by: user        # kullanıcı 21.11.2026: "şimdi yazılsın" (Project belgesi 69 §8 S7); kapı 880 açılınca çalışır
priority: 50
family: karakter
style_gate: false
cost_est: 24
cost_max: 32
depends_on: ["880", "577"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_idle_fidget_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_run_<yön>_<NN>.png
  - art/results/888-*.md
created: 2026-11-21
---
# 888 Çocuk: kıpır kıpır durma ve koşu (24)

## Amaç
Çocuk çeşitleri (69 §6, karar S4: çocukta kıpır durma ve koşu 1.0'da). Çocuk boştayken `idle_fidget` oynar (`AnimStyleRules.IdleKey`); oyunda ve kavgadan kaçarken koşar (`RunRules`).

## Tarif
- Karakterler: 015c işindeki oğlan ve kız (`boy_r64`, `girl_r64`; kimlikler 015c raporunda); 64x64, pivot x=32 y=2. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Yürüme ya da koşu anahtarı 2 karakter x 8 yön (16 çağrı, 018b2 yöntemi ve ücretsiz ayak hizalama); durma anahtarı 2 karakter x 4 çapraz yön (8 çağrı). Hepsinde `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Aynalama yok.
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Eylem metinleri (`action_description`):
  - `idle_fidget`: `a child standing restlessly, bouncing on the toes and swinging the arms, kicking a pebble, subtle loop`
  - `run`: `a child running happily, arms swinging, a full stride cycle with the left foot then the right foot`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, giysi ve saç durma karesiyle aynı. `idle_fidget` 4 çapraz yön; `run` 8 yön, 018b2 ayak hizalama yöntemiyle. Klip başına GIF `art/results/` altına.

## Notlar
- **Kapı:** 880 (ilk poz dalgası Unity'de görülünce kullanıcı `accepted` yapar). Kapı açılmadan çalışmaz (69 §8 S7).
- Kod hazır (PR: animasyon çeşitliliği kod katmanı): kare gelince kod değişmeden oynar; gelene kadar kodla çeşit (tempo, salınım, parçacık) oynar.
- Giysi katmanları (43c, 509): bu kareler gövde setidir; giysi ailelerine aktarım ayrı iştir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
