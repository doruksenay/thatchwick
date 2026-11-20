---
id: "885"
title: "Yaşlı: bastonlu yürüme (8 yön) ve baston durması (24)"
status: pending
approved_by: user        # kullanıcı 21.11.2026: "şimdi yazılsın" (Project belgesi 69 §8 S7); kapı 880 açılınca çalışır
priority: 50
family: karakter
style_gate: false
cost_est: 24
cost_max: 32
depends_on: ["880", "576"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_walk_cane_<yön>_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_idle_cane_{se,sw,nw,ne}_<NN>.png
  - art/results/885-*.md
created: 2026-11-21
---
# 885 Yaşlı: bastonlu yürüme (8 yön) ve baston durması (24)

## Amaç
Bastonlu yaşlı (69 §6, karar S4: 1.0'da şart; Project belgesi 51 yaşlılık sıkıntıları). Dizi tutmayan ya da bastonu olan yaşlı bu kareleri oynar (`Gait.Cane`, `idle_cane`).

## Tarif
- Karakterler: 015a işindeki yaşlı erkek ve yaşlı kadın (`elder_m_r84`, `elder_f_r84`; kimlikler 015a raporunda, yaşlı erkek küreksiz sürüm, 423); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Yürüme ya da koşu anahtarı 2 karakter x 8 yön (16 çağrı, 018b2 yöntemi ve ücretsiz ayak hizalama); durma anahtarı 2 karakter x 4 çapraz yön (8 çağrı). Hepsinde `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Aynalama yok.
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Eylem metinleri (`action_description`):
  - `walk_cane`: `an old person walking slowly with a simple wooden walking cane in the right hand, the cane touching the ground with each step, slightly stooped, a full stride cycle`
  - `idle_cane`: `an old person standing and leaning on a simple wooden walking cane with both hands, breathing slowly, subtle loop`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, giysi ve saç durma karesiyle aynı. `walk_cane` 8 yön, 018b2 yöntemi ve ayak hizalama; `idle_cane` 4 çapraz yön. Baston her karede aynı elde ve ince (1-2 px); kod ayrıca baston çizmez. Klip başına GIF `art/results/` altına.

## Notlar
- **Kapı:** 880 (ilk poz dalgası Unity'de görülünce kullanıcı `accepted` yapar). Kapı açılmadan çalışmaz (69 §8 S7).
- Kod hazır (PR: animasyon çeşitliliği kod katmanı): kare gelince kod değişmeden oynar; gelene kadar kodla çeşit (tempo, salınım, parçacık) oynar.
- Giysi katmanları (43c, 509): bu kareler gövde setidir; giysi ailelerine aktarım ayrı iştir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
