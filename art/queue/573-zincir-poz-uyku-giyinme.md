---
id: "573"
title: "Zincir pozları, uyku ve giyinme: yat, uyan, giyin, işine bak (32)"
status: running
claimed_at: 2026-11-20T16:28Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 40
family: karakter
style_gate: false
cost_est: 32
cost_max: 44
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_lie_sleep_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_wake_stretch_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_dress_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_admire_{se,sw,nw,ne}_<NN>.png
  - art/results/573-*.md
created: 2026-11-20
---
# 573 Zincir pozları, uyku ve giyinme: yat, uyan, giyin, işine bak (32)

## Amaç
İzlenen eylem zincirlerinin (Project belgesi 60) adım pozları. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza ya da etkileşimin eski simgesine düşer. Kare gelince kod değişmeden oynar.

| Anahtar | Nerede oynar |
|---|---|
| `lie_sleep` | yatakta uyuma |
| `wake_stretch` | uyanıp gerinme |
| `dress` | giysi giyme ve çıkarma (uyku takımı, günlük giysi) |
| `admire` | biten işe bakma, eşyayı kaldırıp inceleme |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 4 anahtar x 8 = 32 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `lie_sleep`: `lying on the back on a bed with eyes closed, chest rising and falling slowly, loop`
  - `wake_stretch`: `sitting up on the edge of a bed, stretching both arms overhead and yawning`
  - `dress`: `pulling a loose tunic over the head and straightening it with both hands`
  - `admire`: `holding a finished object up in both hands at chest height, turning it slightly and nodding`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- `lie_sleep` yatay figürdür: 84x84 tuvale sığmazsa 96x84 denenir ve rapora yazılır; pivot yatağın ayak izinin merkezine göre kod oturumunda ayarlanır.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, karakterin giysisi ve saçı durma karesiyle aynı. Elde tutulan nesne (bıçak, kepçe, kürek) okunur ama küçük olmalı: oyunda taşınan eşya ayrıca el çapasına çizilir. Klip başına GIF `art/results/` altına.

## Notlar
- Giysi katmanları (43c, 509 planı): bu kareler gövde setidir; giysi ailelerine aktarım 509'daki toplu parti yöntemiyle ayrı iştir.
- El konumları: her klibin ilk karesindeki tutan elin konumu rapora yazılır (`hand_anchors.txt` için).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
