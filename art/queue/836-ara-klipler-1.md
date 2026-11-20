---
id: "836"
title: "Ara klipler 1/2: alın teri, bel, esneme, baş kaşıma, silkelenme, karın (48)"
status: pending
approved_by: user        # kullanıcı 21.11.2026: "şimdi yazılsın" (Project belgesi 69 §8 S7); kapı 830 açılınca çalışır
priority: 50
family: karakter
style_gate: false
cost_est: 48
cost_max: 58
depends_on: ["830"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ix_wipe_brow_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ix_back_stretch_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ix_yawn_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ix_scratch_head_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ix_shake_off_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ix_hold_belly_{se,sw,nw,ne}_<NN>.png
  - art/results/836-*.md
created: 2026-11-21
---
# 836 Ara klipler 1/2: alın teri, bel, esneme, baş kaşıma, silkelenme, karın (48)

## Amaç
Ara klipler (69 §4, kod: `InterjectionPicker`, `GameRoot.UpdateInterjection`): uzun işin, durmanın ve yürüyüş duraklarının arasına nedenine göre giren tek seferlik hareket (sıcakta alın teri, açken karın, iş bitince küçük sevinç...). Klip yaklaşık 1,2 sn oynar; kare yokken kod küçük sıçrama ve parçacık gösterir.

## Tarif
- Karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Aynalama yok.
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Eylem metinleri (`action_description`):
  - `ix_wipe_brow`: `wiping sweat from the forehead with the back of the hand, once, then back to standing`
  - `ix_back_stretch`: `straightening up and pressing both hands into the lower back, arching a little, once, then back to standing`
  - `ix_yawn`: `a big yawn with one hand over the mouth and a small stretch, once, then back to standing`
  - `ix_scratch_head`: `scratching the back of the head in puzzlement, once, then back to standing`
  - `ix_shake_off`: `shaking the body and arms quickly to shake off water or dust, once, then back to standing`
  - `ix_hold_belly`: `pressing one hand to the belly as if hungry, shoulders dropping a little, once, then back to standing`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, giysi ve saç durma karesiyle aynı. Tek seferlik: ilk ve son kare durma karesine oturur (döngü değil). Klip başına GIF `art/results/` altına.

## Notlar
- **Kapı:** 830 (ilk poz dalgası Unity'de görülünce kullanıcı `accepted` yapar). Kapı açılmadan çalışmaz (69 §8 S7).
- Kod hazır (PR: animasyon çeşitliliği kod katmanı): kare gelince kod değişmeden oynar; gelene kadar kodla çeşit (tempo, salınım, parçacık) oynar.
- Giysi katmanları (43c, 509): bu kareler gövde setidir; giysi ailelerine aktarım ayrı iştir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
