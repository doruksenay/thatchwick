---
id: "018a"
title: Yürüme pilotu: erkek 84 px, SE
status: pending
approved_by: user        # otomatik akış (art/budget.md); plan işi 018
priority: 50
family: karakter
style_gate: true
cost_est: 2
cost_max: 6
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_man_r84_walk_se_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_man_r84_idle_se.png
  - art/results/018a-yurume-pilot-84.md
created: 2026-11-18
---
# 018a Yürüme pilotu: erkek 84 px, SE

## Amaç
012b'de kabul edilen 84 px gerçekçi erkek karakterin yürüme döngüsünü pixminimax ile doğrulamak. Bu pilot, 018b ve bütün etkileşim işlerinin (018c-e, 368) yöntem kapısıdır; sonucu kullanıcı onaylar.

## Tarif
- erkek: `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın: `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84, 8 yön. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Yalnızca erkek, yalnızca `south-east`. Tek `animate_character` çağrısı: `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = SE durma karesi (`Assets/Art/Iso/_aday/iso_char_man_r84_cand_se.png`), `action_description="walking forward at a calm, steady pace, one complete step cycle"`.
- Maliyet satırı `cost_est`'ten çok farklıysa çağırma, `## Sorular` ekle ve `blocked` yap.

## Ortak işleme (014 kuralları 84 px'e uyarlanmış)
- Yöntem: `animate_character`, `mode=pixminimax`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Tek deneme.
- Tuval 84x84 (PixelLab başka tuval verirse 84x84'e maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), tek pivot x=42, y=2 (ALTTAN).
- Palet: 012b dosyalarının 32 rengi (karakter başına); en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı poz ise son kare atılır.
- Dosya adı: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`.
- Doğrulama: `validate_iso.py`, `style.py check --family karakter` (KALDI teslim edilmez), her anahtar için GIF/önizleme ve yan yana sayfası; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
- 013 ölçüm tablosunu (boy aralığı, ardışık IoU, döngü kapanışı, palet dışı oran, tepe kırpılması) 84 px için yeniden yaz. Kapanış IoU 1,00 ise son kare atılır.
- Kafa/gövde oranı korunur (012b'de boy/kafa ~3,5; bu pilot oranı düzeltmez).
- Önizleme GIF'i `art/results/018a-yurume.gif`.


<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
