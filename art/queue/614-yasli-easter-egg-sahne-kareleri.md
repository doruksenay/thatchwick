---
id: "614"
title: "Yaşlılar: easter egg sahne kareleri (topaç, uluma, yerden alma)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet ("yaşlıların eylem kareleri: evet"), Project belgesi 55d
priority: 50
family: karakter
style_gate: false
cost_est: 24
cost_max: 34
depends_on: ["415", "511"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_top_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_howl_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_pickup_{se,sw,nw,ne}_<NN>.png
  - art/results/614-*.md
created: 2026-11-20
---
# 614 Yaşlılar: topaç, uluma, yerden alma

## Amaç
511 (yetişkin) ve 512'nin (çocuk) yaşlı eşi. Yaşlılar artık yere düşen süsü kendiliğinden kaldırıyor (kod: `LifeSystems.TidyJobFor`); kullanıcı şartı "yerden aldığı mutlaka görünsün". Şu an yaşlının hiç eylem karesi yok, durma görseliyle kalıyor. Kod kareleri adla bulur (`GameRoot.ActionKeys`: `pickup`, `top`, `howl`); kare gelince kod değişmeden oynar.

## Tarif
- Karakterler (415 raporu): yaşlı erkek `8dcaede5-cc4d-4068-b56f-ab5e327fea1c` (elder_m_r84), yaşlı kadın `b7f31e8d-29f2-4f6a-9092-a02759611159` (elder_f_r84); 84x84. `get_character` ile `completed` doğrulanır (ücretsiz).
- Yöntem 015a ile aynı: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `custom_start_frame_url` ve `end_frame_url` = ilgili yönün **depodaki** durma karesi (`get_upload_url` ile yüklenir). Yaşlı erkekte küreksiz sürüm kullanılır: `iso_char_elder_m_r84_idle_<yön>_v3` varsa o, yoksa `_v2`, yoksa düz ad (423, 423b). PixelLab'deki döndürme görseli kullanılmaz (kürekli).
- Her anahtar için 2 karakter x 4 çapraz yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 çağrı. 3 anahtar x 8 = 24 gen. Aynalama yok. Bağımsız çağrılar paralel gönderilir; çağrıdan önce maliyet satırı okunur, `cost_est`'ten çok farklıysa çağrılmaz.
- Her metnin sonuna yaşlı duruşu eklenir: `, an elderly person moving slowly and carefully, slightly stooped`.
- Eylem metinleri (`action_description`; erkek ve kadın setindeki metinlerle aynı):
  - `top`: `crouching slightly and spinning a small wooden spinning top on the ground in front with a whip cord, wrist flicking, the top visible spinning at the feet`
  - `howl`: `tilting the head far back and howling at the sky like a wolf, hands cupped around the mouth, chest rising`
  - `pickup`: `bending down, picking up a small object from the floor with one hand, then standing up holding it`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (84x84 tuval, ölçekleme yok, ayak satırı durma karesiyle aynı, palet karakterin durma karelerinden, 00 ile son kare aynı pozsa son kare atılır) ve 018b2'deki ücretsiz dikey hizalama. Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
`python tools/art/validate_iso.py --kind object --max-colors 32 <dosya>`, `python tools/art/style.py check --family karakter <dosya>` (KALDI teslim edilmez). Klip başına GIF ve yan yana sayfası (durma karesi + 00/02/04/06). Kimlik: yüz, saç ve giysi durma karesiyle aynı; yaşlı erkekte kürek görünmez. Yaşlı gençleşmez: duruş hafif kambur kalır.

Ek ölçüt: `pickup` karelerinde en az iki karede el yere yakın (gövde belirgin eğik), son karelerde dik. `top`: topaç ayak hizasında, karenin dışına taşmaz. `howl`: baş geriye yatık, tepe kırpılmaz.

## Notlar
- Dönem: tahta topaç ve kamçı ipi ortaçağ oyuncağıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
