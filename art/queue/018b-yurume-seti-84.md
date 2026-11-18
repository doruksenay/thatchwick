---
id: "018b"
title: Yürüme seti: erkek ve kadın 84 px, 8 yön
status: running
claimed_at: 2026-11-18T18:54Z
approved_by: user        # otomatik akış (art/budget.md); plan işi 018
priority: 50
family: karakter
style_gate: false
cost_est: 16
cost_max: 24
depends_on: ["018a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_walk_<yön>_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_idle_<yön>.png
  - art/results/018b-yurume-seti-84.md
created: 2026-11-18
---
# 018b Yürüme seti: erkek ve kadın 84 px, 8 yön

## Amaç
Oyunun yürüme animasyonu: erkek ve kadın 84 px karakter, 8 yön, 8 kare döngü. Eski 56 px köylü için yazılmış 013/014/015 işlerinin yerini alır.

## Tarif
- erkek: `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın: `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84, 8 yön. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- 2 karakter x 8 yön = 16 `animate_character` çağrısı (south, south-east, east, north-east, north, north-west, west, south-west), her biri `pixminimax`, `frame_count=8`, `action_description="walking forward at a calm, steady pace, one complete step cycle"` (018a farklı metin seçtiyse onun raporu geçerli). PixelLab aynı karakter ve aynı metinle yeni iş açmıyorsa metne küçük bir ek yap ve raporla (014 notu). Aynalama yok.
- Çağrılar birbirinden bağımsız: paralel gönder.

## Ortak işleme (014 kuralları 84 px'e uyarlanmış)
- Yöntem: `animate_character`, `mode=pixminimax`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Tek deneme.
- Tuval 84x84 (PixelLab başka tuval verirse 84x84'e maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), tek pivot x=42, y=2 (ALTTAN).
- Palet: 012b dosyalarının 32 rengi (karakter başına); en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı poz ise son kare atılır.
- Dosya adı: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`.
- Doğrulama: `validate_iso.py`, `style.py check --family karakter` (KALDI teslim edilmez), her anahtar için GIF/önizleme ve yan yana sayfası; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
Her yönde validate_iso + style check; 018a ölçüm tablosunun yön başına özeti. İki karakter arasında yön kimliği tutarlı olmalı.


<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
