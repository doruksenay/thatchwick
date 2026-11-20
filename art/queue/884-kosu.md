---
id: "884"
title: "Koşu, yetişkin, 8 yön (16)"
status: pending
approved_by: user        # kullanıcı 21.11.2026: "şimdi yazılsın" (Project belgesi 69 §8 S7); kapı 880 açılınca çalışır
priority: 50
family: karakter
style_gate: false
cost_est: 16
cost_max: 24
depends_on: ["880"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_run_<yön>_<NN>.png
  - art/results/884-*.md
created: 2026-11-21
---
# 884 Koşu, yetişkin, 8 yön (16)

## Amaç
Koşu (69, karar S3): acil durum (kavgadan kaçış, ebe çağrısı), oyuncu emriyle uzun yol (14+ karo), çocuk oyunu. Hız `RunRules` (yürümenin 1,6 katı, Atletizm ile çarpılır); kare `IsoArt.CharacterGait(..., "run")`.

## Tarif
- Karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 8 yön = 16 `animate_character` çağrısı; 018b2 yöntemi (aday d): `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` ilgili yönün durma karesi. 018b2'deki ücretsiz ayak hizalama düzeltmesi zorunlu (son kare gömülürse ya da döngüyü bozarsa atılır).
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Eylem metinleri (`action_description`):
  - `run`: `running at an easy jog, arms bent and pumping, a full stride cycle with the left foot then the right foot, body leaning slightly forward`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, giysi ve saç durma karesiyle aynı. Koşuda kafa farkı ≤ 5 px; ayak satırı sabit, iki ayak aynı anda yerden kesilen kare olabilir. Klip başına GIF `art/results/` altına.

## Notlar
- **Kapı:** 880 (ilk poz dalgası Unity'de görülünce kullanıcı `accepted` yapar). Kapı açılmadan çalışmaz (69 §8 S7).
- Kod hazır (PR: animasyon çeşitliliği kod katmanı): kare gelince kod değişmeden oynar; gelene kadar kodla çeşit (tempo, salınım, parçacık) oynar.
- Giysi katmanları (43c, 509): bu kareler gövde setidir; giysi ailelerine aktarım ayrı iştir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
