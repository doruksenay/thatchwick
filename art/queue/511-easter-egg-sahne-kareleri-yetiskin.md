---
id: "511"
title: "Easter egg sahne kareleri: topaç, uluma, yerden alma (erkek ve kadın 84 px)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet (easter egg sahneleri), Project belgeleri 55 ve 55d
priority: 50
family: karakter
style_gate: false
cost_est: 24
cost_max: 34
depends_on: ["018e"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_top_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_howl_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_pickup_{se,sw,nw,ne}_<NN>.png
  - art/results/511-*.md
created: 2026-11-20
---
# 511 Easter egg sahne kareleri (yetişkin)

## Amaç
Kullanıcı (20.11.2026): easter egg davranışlarının animasyonu mutlaka olmalı. Üç yeni eylem anahtarı (kod: `GameRoot.ActionKeys`):
- `top`: topaç şovu (en yüksek topaç ustalığındaki karakter çevresine gösteri yapar).
- `howl`: dolunay gecesi köpekle birlikte uluma.
- `pickup`: kedinin yere düşürdüğü süsü eğilip alma ("Yerine koy"). Kullanıcı şartı: yerden aldığı mutlaka görünsün.

Kareler gelene kadar kod en yakın eldeki kareye düşer (`fun`, `care`).

## Tarif
- erkek: `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın: `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 çapraz yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 çağrı; 3 anahtar x 8 = 24 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `top`: `crouching slightly and spinning a small wooden spinning top on the ground in front with a whip cord, wrist flicking, the top visible spinning at the feet`
  - `howl`: `tilting the head far back and howling at the sky like a wolf, hands cupped around the mouth, chest rising`
  - `pickup`: `bending down, picking up a small object from the floor with one hand, then standing up holding it`

## Ortak işleme (018e ile aynı)
- Yöntem: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Yön başına tek çağrı, tek deneme. Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Tuval 84x84 (PixelLab başka tuval verirse maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), pivot x=42, y=2 (ALTTAN).
- Ücretsiz hizalama (018b2): her kare, yere basan ayağın en alt pikseli durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırılır.
- Palet: karakterin durma karelerinin renkleri (≤32), en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı pozsa son kare atılır.
- Doğrulama: `python tools/art/validate_iso.py --kind object --max-colors 32 <dosya>`, `python tools/art/style.py check --family karakter <dosya>` (KALDI teslim edilmez), klip başına GIF ve yan yana sayfası `art/results/` altına; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
Ortak işleme doğrulaması. `pickup`: en az iki karede el yere yakın (gövde belirgin eğik), son karelerde dik. `top`: topaç ayak hizasında, karenin dışına taşmaz. `howl`: baş geriye yatık, tepe kırpılmaz.

## Notlar
- Dosya adı: `iso_char_<man|woman>_r84_<top|howl|pickup>_<yön>_<NN>.png`.
- Dönem: tahta topaç ve kamçı ipi ortaçağ oyuncağıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
