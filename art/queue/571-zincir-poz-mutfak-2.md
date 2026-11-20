---
id: "571"
title: "Zincir pozları, mutfak 2/2: dök, uzan, taşı, tutuştur, fırın küreği (40)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 40
family: karakter
style_gate: false
cost_est: 40
cost_max: 52
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_pour_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_reach_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_carry_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_kindle_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_peel_{se,sw,nw,ne}_<NN>.png
  - art/results/571-*.md
created: 2026-11-20
---
# 571 Zincir pozları, mutfak 2/2: dök, uzan, taşı, tutuştur, fırın küreği (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgesi 60) adım pozları. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza ya da etkileşimin eski simgesine düşer. Kare gelince kod değişmeden oynar.

| Anahtar | Nerede oynar |
|---|---|
| `pour` | testiden, kovadan ya da çuvaldan dökme; sulama; fincana doldurma |
| `reach` | raftan, kilerden, askıdan alma ve koyma |
| `carry` | elde eşyayla durma ve taşıma (servis kabı, sepet, çuval, tahta) |
| `kindle` | ateş tutuşturma ve üfleme; ocağa odun atma |
| `peel` | fırına kürekle sürme ve çıkarma |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `pour`: `tilting a clay jug held in both hands and pouring into a vessel at waist height, then righting it, loop`
  - `reach`: `reaching forward and up with one arm to take something from a shelf and bringing it down to the chest, loop`
  - `carry`: `standing while holding a large object against the chest with both arms, shifting weight slightly, loop`
  - `kindle`: `crouching in front of a low hearth, striking a spark and blowing gently on the embers, loop`
  - `peel`: `sliding a long-handled wooden baker peel forward at waist height and drawing it back, loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, karakterin giysisi ve saçı durma karesiyle aynı. Elde tutulan nesne (bıçak, kepçe, kürek) okunur ama küçük olmalı: oyunda taşınan eşya ayrıca el çapasına çizilir. Klip başına GIF `art/results/` altına.

## Notlar
- Giysi katmanları (43c, 509 planı): bu kareler gövde setidir; giysi ailelerine aktarım 509'daki toplu parti yöntemiyle ayrı iştir.
- El konumları: her klibin ilk karesindeki tutan elin konumu rapora yazılır (`hand_anchors.txt` için).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
