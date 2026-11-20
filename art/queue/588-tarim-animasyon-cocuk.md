---
id: "588"
title: "Tarım animasyonları, çocuklar: eğilip toplama, sulama, ekim (24)"
status: review
claimed_at: 2026-11-20T17:01Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet ("Çocuk gövdesi için eğilme, sulama ve ekim kareleri üretilsin mi?" → "evet üretsin"), Project belgesi 59b
priority: 45
family: karakter
style_gate: false
cost_est: 24
cost_max: 34
depends_on: ["015c"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_bend_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_water_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_sow_{se,sw,nw,ne}_<NN>.png
  - art/results/588-*.md
created: 2026-11-20
---
# 588 Tarım animasyonları, çocuklar: eğilip toplama, sulama, ekim (24)

## Amaç
Ot yolma çocuk işi olacak ve çocuğun bostanı geliyor (Project belgeleri 59 ve 59a, N5). Bugün çocuk iş yaparken durma görselinde kalıyor. Kod (`IsoArt.CharacterAction`) çocuk için `iso_char_<boy|girl>_r64_<anahtar>_<yön>_NN` karelerini arar; kare gelince kod değişmeden oynar. Yalnız bu üç anahtar üretilir; orak, tırpan, saban ve harman çocuk işi değildir.

| Anahtar | Nerede oynar |
|---|---|
| `bend` | ot yolma, toplama düğümleri, sararmış ekini sökme (yetişkin eşi: 565) |
| `water` | çocuğun bostanını sulama (yetişkin eşi: 018c) |
| `sow` | çocuğun bostanına tohum ekme (yetişkin eşi: 018c) |

## Tarif
- Karakter kimlikleri: 015c raporu (`art/results/015c-yurume-cocuk-64.md`; boy `6940a34e-...`, girl `9d58ae77-...`, tam kimlik `list_characters` ile). 64 px tuval. `get_character` ile doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 çapraz yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 çağrı; 3 anahtar x 8 = 24 gen. Aynalama yok. 015c'deki "aynı metinli istek mevcut klibe eşleniyor" sorunu için her çağrının metninin başına yön eklenir (örn. `facing south-east: ...`).
- Eylem metinleri (`action_description`):
  - `bend`: `a child bending down at the waist, pulling a small weed from the ground with both hands and straightening up, loop`
  - `water`: `a child holding a small wooden watering pail with both hands and tipping it forward to pour water on the ground in front, loop`
  - `sow`: `a child crouching a little and dropping seeds one by one from the hand into the soil in front, loop`

## Ortak işleme (512 ile aynı)
- Yöntem: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Yön başına tek çağrı, tek deneme. Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Tuval 64x64 (PixelLab başka tuval verirse maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), pivot x=32, y=2 (ALTTAN).
- Ücretsiz hizalama (018b2): her kare, yere basan ayağın en alt pikseli durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırılır.
- Palet: karakterin durma karelerinin renkleri (≤32), en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı pozsa son kare atılır.
- Doğrulama: `python tools/art/validate_iso.py --kind object --max-colors 32 <dosya>`, `python tools/art/style.py check --family karakter <dosya>` (KALDI teslim edilmez), klip başına GIF ve yan yana sayfası `art/results/` altına; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
512 ile aynı ölçütler; boy 015c durma kareleriyle aynı bantta. Kova küçük ve çocuğun boyuna uygun okunmalı (yetişkin kovası değil).

## Notlar
- Çocuk oranı korunur (büyük baş, kısa bacak); yetişkine benzemez.
- Giysi katmanları (43c, 509 planı): bu kareler gövde setidir; çocuk giysi ailelerine aktarım 509'daki toplu parti yöntemiyle ayrı iştir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
