---
id: "572"
title: "Zincir pozları, sofra ve ihtiyaç: otur, sofrada ye, iç, ellerini ısıt, yüzünü yıka (40)"
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
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_sit_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_sit_eat_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_drink_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_warm_hands_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_wash_{se,sw,nw,ne}_<NN>.png
  - art/results/572-*.md
created: 2026-11-20
---
# 572 Zincir pozları, sofra ve ihtiyaç: otur, sofrada ye, iç, ellerini ısıt, yüzünü yıka (40)

## Amaç
İzlenen eylem zincirlerinin (Project belgesi 60) adım pozları. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza ya da etkileşimin eski simgesine düşer. Kare gelince kod değişmeden oynar.

| Anahtar | Nerede oynar |
|---|---|
| `sit` | sandalyede, bankta, sedirde oturma |
| `sit_eat` | sofrada oturarak yeme |
| `drink` | bardaktan ya da fincandan içme |
| `warm_hands` | ocak başında ısınma |
| `wash` | leğende ya da çeşmede el ve yüz yıkama |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `sit`: `sitting upright on a low stool with hands resting on the knees, breathing calmly, small idle movement, loop`
  - `sit_eat`: `sitting on a low stool and eating from a bowl on a table in front with a spoon, lifting the spoon to the mouth, loop`
  - `drink`: `raising a small cup to the lips with one hand, sipping and lowering it, loop`
  - `warm_hands`: `standing with both hands held out forward at waist height toward a fire, rubbing them together now and then, loop`
  - `wash`: `bending over a basin at waist height, scooping water with both hands and splashing the face, loop`
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
