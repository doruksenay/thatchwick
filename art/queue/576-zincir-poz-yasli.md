---
id: "576"
title: "Zincir pozları, yaşlılar: otur, sofrada ye, uyu, yüzünü yıka (32)"
status: running
claimed_at: 2026-11-20T16:28Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 40
family: karakter
style_gate: false
cost_est: 32
cost_max: 44
depends_on: ["015a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_sit_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_sit_eat_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_lie_sleep_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_wash_{se,sw,nw,ne}_<NN>.png
  - art/results/576-*.md
created: 2026-11-20
---
# 576 Zincir pozları, yaşlılar: otur, sofrada ye, uyu, yüzünü yıka (32)

## Amaç
İzlenen eylem zincirlerinin (Project belgesi 60) adım pozları. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza ya da etkileşimin eski simgesine düşer. Kare gelince kod değişmeden oynar.

| Anahtar | Nerede oynar |
|---|---|
| `sit` | oturma |
| `sit_eat` | sofrada yeme |
| `lie_sleep` | uyuma |
| `wash` | el ve yüz yıkama |

## Tarif
- Karakterler: 015a işindeki yaşlı erkek ve yaşlı kadın (`elder_m_r84`, `elder_f_r84`; kimlikler 015a raporunda); 84x84. Yaşlı erkek küreksiz sürümdür (423).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 4 anahtar x 8 = 32 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `sit`: `sitting on a low stool, slightly hunched, hands resting on the knees, loop`
  - `sit_eat`: `sitting on a low stool and eating slowly from a bowl with a spoon, loop`
  - `lie_sleep`: `lying on the back on a bed with eyes closed, chest rising and falling slowly, loop`
  - `wash`: `bending carefully over a basin and splashing water on the face with both hands, loop`
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
