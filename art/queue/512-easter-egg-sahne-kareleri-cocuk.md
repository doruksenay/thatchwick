---
id: "512"
title: "Easter egg sahne kareleri: topaç, uluma, yerden alma (çocuk 64 px)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet (easter egg sahneleri), Project belgeleri 55 ve 55d
priority: 50
family: karakter
style_gate: false
cost_est: 24
cost_max: 34
depends_on: ["015c"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_top_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_howl_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_pickup_{se,sw,nw,ne}_<NN>.png
  - art/results/512-*.md
created: 2026-11-20
---
# 512 Easter egg sahne kareleri (çocuk)

## Amaç
511'in çocuk eşi. Topaç bir çocuk oyuncağı ve dolunayda ulumaya çocuklar eşlik ediyor; çocukların şu an hiç eylem karesi yok. Kod (`IsoArt.CharacterAction`) çocuk için `iso_char_<boy|girl>_r64_<anahtar>_<yön>_NN` karelerini arar; yoksa durma görseli kalır.

## Tarif
- Karakter kimlikleri: 015c raporu (`art/results/015c-yurume-cocuk-64.md`; boy `6940a34e-...`, girl `9d58ae77-...`, tam kimlik `list_characters` ile). 64 px tuval. `get_character` ile doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 çapraz yön = 8 çağrı; 3 anahtar x 8 = 24 gen. 015c'deki "aynı metinli istek mevcut klibe eşleniyor" sorunu için her çağrının metninin başına yön eklenir (örn. `facing south-east: ...`).
- Eylem metinleri (`action_description`):
  - `top`: `a child crouching and spinning a small wooden spinning top on the ground in front with a whip cord, wrist flicking, the top visible spinning at the feet`
  - `howl`: `a child tilting the head far back and howling playfully at the sky like a wolf, hands cupped around the mouth, chest rising`
  - `pickup`: `bending down, picking up a small object from the floor with one hand, then standing up holding it`

## Ortak işleme (018e ile aynı)
- Yöntem: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Yön başına tek çağrı, tek deneme. Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Tuval 64x64 (PixelLab başka tuval verirse maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), pivot x=32, y=2 (ALTTAN).
- Ücretsiz hizalama (018b2): her kare, yere basan ayağın en alt pikseli durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırılır.
- Palet: karakterin durma karelerinin renkleri (≤32), en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı pozsa son kare atılır.
- Doğrulama: `python tools/art/validate_iso.py --kind object --max-colors 32 <dosya>`, `python tools/art/style.py check --family karakter <dosya>` (KALDI teslim edilmez), klip başına GIF ve yan yana sayfası `art/results/` altına; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
511 ile aynı ölçütler; boy 015c durma kareleriyle aynı bantta.

## Notlar
- Çocuk oranı korunur (büyük baş, kısa bacak); yetişkine benzemez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
