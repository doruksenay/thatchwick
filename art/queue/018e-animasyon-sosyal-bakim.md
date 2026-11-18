---
id: "018e"
title: Etkileşim animasyonları: sohbet, eğlence, bakım, sevgi, balık
status: pending
approved_by: user        # otomatik akış (art/budget.md); plan işi 018
priority: 50
family: karakter
style_gate: false
cost_est: 40
cost_max: 52
depends_on: ["018a2"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_social_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_fun_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_care_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_love_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_fish_{se,sw,nw,ne}_<NN>.png
  - art/results/018e-*.md
created: 2026-11-18
---
# 018e Etkileşim animasyonları: sohbet, eğlence, bakım, sevgi, balık

## Amaç
Etkileşim animasyonları (84 px erkek ve kadın). Anahtar eşlemesi: `art/results/018-etkilesim-animasyonlari-plani.md`.

## Tarif
- erkek: `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın: `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84, 8 yön. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `social`: `talking with expressive hand gestures, slight head nods, standing`
  - `fun`: `playing: a small joyful dance or hop, arms swinging`
  - `care`: `gently rocking a bundled baby in the arms, standing`
  - `love`: `embracing a partner: arms opening and closing in a hug, warm gesture`
  - `fish`: `casting and holding a fishing rod toward the water in front, slight tug`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme (014 kuralları 84 px'e uyarlanmış)
- Yöntem: `animate_character`, `mode=pixminimax`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Tek deneme.
- Tuval 84x84 (PixelLab başka tuval verirse 84x84'e maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), tek pivot x=42, y=2 (ALTTAN).
- Palet: 012b dosyalarının 32 rengi (karakter başına); en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı poz ise son kare atılır.
- Dosya adı: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`.
- Doğrulama: `validate_iso.py`, `style.py check --family karakter` (KALDI teslim edilmez), her anahtar için GIF/önizleme ve yan yana sayfası; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
018a ölçütleri her klip için; ek olarak elde tutulan nesnenin okunurluğu ve ayak satırı sabitliği. Klip başına GIF `art/results/` altına.


<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
