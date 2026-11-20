---
id: "831"
title: "Hâl durmaları 1/2: huzurlu, neşeli, şevkli, gururlu (32)"
status: pending
approved_by: user        # kullanıcı 21.11.2026: "şimdi yazılsın" (Project belgesi 69 §8 S7); kapı 830 açılınca çalışır
priority: 50
family: karakter
style_gate: false
cost_est: 32
cost_max: 42
depends_on: ["830"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_idle_peaceful_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_idle_cheerful_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_idle_eager_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_idle_proud_{se,sw,nw,ne}_<NN>.png
  - art/results/831-*.md
created: 2026-11-21
---
# 831 Hâl durmaları 1/2: huzurlu, neşeli, şevkli, gururlu (32)

## Amaç
Hâl durmaları (Project belgesi 69 §3.1, karar S1: 7 hâlin her birine ayrı durma). Karakter boştayken baskın hâline göre oynar (`AnimStyleRules.IdleKey`, `GameRoot.HalIdleFrame`). Sakin hâl bugünkü durmayı kullanır.

## Tarif
- Karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Aynalama yok.
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Eylem metinleri (`action_description`):
  - `idle_peaceful`: `standing relaxed with hands clasped behind the back, slowly shifting weight from one foot to the other, calm content face, subtle loop`
  - `idle_cheerful`: `standing and swaying gently side to side, nodding the head to a tune, small smile, subtle loop`
  - `idle_eager`: `standing and rubbing the hands together eagerly, then rolling up one sleeve, ready to work, subtle loop`
  - `idle_proud`: `standing tall with chest out and hands on the hips, chin slightly raised, subtle loop`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, giysi ve saç durma karesiyle aynı. Durma hareketi küçük kalmalı (gövde 2 px'ten fazla kaymaz); ilk ve son kare durma karesine oturur, döngü dikişsiz. Klip başına GIF `art/results/` altına.

## Notlar
- **Kapı:** 830 (ilk poz dalgası Unity'de görülünce kullanıcı `accepted` yapar). Kapı açılmadan çalışmaz (69 §8 S7).
- Kod hazır (PR: animasyon çeşitliliği kod katmanı): kare gelince kod değişmeden oynar; gelene kadar kodla çeşit (tempo, salınım, parçacık) oynar.
- Giysi katmanları (43c, 509): bu kareler gövde setidir; giysi ailelerine aktarım ayrı iştir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
