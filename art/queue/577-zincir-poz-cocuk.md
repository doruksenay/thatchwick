---
id: "577"
title: "Zincir pozları, çocuklar: otur, sofrada ye, uyu, yüzünü yıka (32)"
status: review
claimed_at: 2026-11-20T16:43Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 40
family: karakter
style_gate: false
cost_est: 32
cost_max: 44
depends_on: ["015c"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_sit_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_sit_eat_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_lie_sleep_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_wash_{se,sw,nw,ne}_<NN>.png
  - art/results/577-*.md
created: 2026-11-20
---
# 577 Zincir pozları, çocuklar: otur, sofrada ye, uyu, yüzünü yıka (32)

## Amaç
İzlenen eylem zincirlerinin (Project belgesi 60) adım pozları. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`, `GameRoot.StepFrameFor`); kare gelene kadar en yakın eldeki poza ya da etkileşimin eski simgesine düşer. Kare gelince kod değişmeden oynar.

| Anahtar | Nerede oynar |
|---|---|
| `sit` | oturma |
| `sit_eat` | sofrada yeme |
| `lie_sleep` | uyuma |
| `wash` | el ve yüz yıkama |

## Tarif
- Karakterler: 015c işindeki oğlan ve kız (`boy_r64`, `girl_r64`; kimlikler 015c raporunda); 64x64, pivot x=32 y=2.
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 4 anahtar x 8 = 32 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `sit`: `sitting on a low stool swinging the legs a little, loop`
  - `sit_eat`: `sitting on a low stool and eating eagerly from a bowl with a spoon, loop`
  - `lie_sleep`: `lying curled on the side on a bed with eyes closed, breathing slowly, loop`
  - `wash`: `standing on tiptoe at a basin and splashing water on the face, loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, karakterin giysisi ve saçı durma karesiyle aynı. Elde tutulan nesne (bıçak, kepçe, kürek) okunur ama küçük olmalı: oyunda taşınan eşya ayrıca el çapasına çizilir. Klip başına GIF `art/results/` altına.

## Sorular
- 2026-11-20 (işçi): kısmi teslim. Yalnızca `wash` geçti (8 klip). `sit`, `sit_eat`, `lie_sleep` pixminimax + durma karesi bitiş yöntemiyle oturma/yatma pozuna gitmedi (karakter ayakta kaldı); tek deneme kuralı gereği tekrar denenmedi. Öneri: oturan/yatan başlangıç karesi (`custom_start_frame`, yön başına) ile yeni iş (ör. 577b); tabure ve yatak ayrı nesne. Ayrıntı: art/results/577-zincir-poz-cocuk.md. Kullanıcı kararı: 577b yazılsın mı?

## Notlar
- Giysi katmanları (43c, 509 planı): bu kareler gövde setidir; giysi ailelerine aktarım 509'daki toplu parti yöntemiyle ayrı iştir.
- El konumları: her klibin ilk karesindeki tutan elin konumu rapora yazılır (`hand_anchors.txt` için).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
