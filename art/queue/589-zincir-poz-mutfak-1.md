---
id: "589"
title: "Zincir pozları, mutfak 1/2: karıştır, doğra, tat, kepçe, yoğur (40)"
status: running
claimed_at: 2026-11-20T16:43Z
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
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_stir_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_chop_knife_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_taste_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ladle_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_knead_{se,sw,nw,ne}_<NN>.png
  - art/results/570-*.md
created: 2026-11-20
---
# 589 Zincir pozları, mutfak 1/2: karıştır, doğra, tat, kepçe, yoğur (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgesi 60) adım pozları. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza ya da etkileşimin eski simgesine düşer. Kare gelince kod değişmeden oynar.

| Anahtar | Nerede oynar |
|---|---|
| `stir` | kazanda karıştırma (yemek, boya, mum yağı, mayalama) |
| `chop_knife` | tezgâhta bıçakla doğrama, dilimleme |
| `taste` | yemeğin tadına bakma |
| `ladle` | yemeği kepçeyle servis kabına alma |
| `knead` | hamur yoğurma, kil yoğurma, peynir bastırma |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `stir`: `standing at a waist-high pot and stirring it in slow circles with a long wooden spoon held in one hand, loop`
  - `chop_knife`: `standing at a waist-high board and chopping vegetables with a small knife in quick short strokes, the other hand steadying the food, loop`
  - `taste`: `lifting a wooden spoon to the lips, tasting, pausing and giving a small satisfied nod`
  - `ladle`: `scooping from a waist-high pot with a ladle and pouring it into a bowl held in the other hand, loop`
  - `knead`: `leaning over a waist-high trough and kneading dough with both hands, pushing forward and folding back, rhythmic loop`
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
