---
id: "015c"
title: Yürüme seti, çocuk erkek ve kız (64 px, walk2, 8 yön)
status: review
claimed_at: 2026-11-19T12:54Z
approved_by: user        # kullanıcı 19.11.2026: "Kodu yaz" (görsel parti 3; 014/015 yeni karaktere göre yeniden yazıldı)
priority: 20
family: karakter
style_gate: false
cost_est: 24
cost_max: 40
depends_on: ["018b2", "416"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_walk2_<yön>_<NN>.png
  - art/results/015c-yurume-cocuk-64.md
created: 2026-11-19
---
# 015c Yürüme seti: çocuk erkek ve kız (64 px)

## Amaç
Eski 015c (K2 56 px boy_a, girl_a) 012'nin reddiyle geçersiz kaldı. Çocuklar 416'da gerçekçi oranlı 64 px tuvale geçti (kabul). Oyunda çocuklar şu an yürürken durma karesi + kodla sallantı gösteriyor.

## Tarif
- **Yöntem ve metin 018b2 ile aynı:**
  - `animate_character`, `pixminimax`, `frame_count=8`, `keep_first_frame=true`;
  - `end_frame` = ilgili yönün durma karesi (`iso_char_{boy,girl}_r64_idle_<yön>`);
  - metin aynen: `walking at a calm steady pace, a full stride cycle with the left foot then the right foot stepping equally, head and shoulders level, no bouncing, no limping`.
- **Karakterler (416):**
  - boy `6940a34e-75c4-4cb6-b88d-8ef7e0e7719e`
  - girl `9d58ae77-0243-403e-84d3-33799a2f7e70`
  - Her biri 8 yön; çağrılar bağımsız, paralel.
- **Ücretsiz düzeltme (zorunlu, 018b2 ile aynı):** ayak satırı hizalama ve döngüyü bozan son karenin atılması.
- Tuval 64x64, pivot (32, 2 alttan); ad `walk2`.
- Bir yön ölçütü geçmezse bütçe izin verirse bir kez daha denenir (yön başına en çok 2 deneme).
- Yine geçmeyen yön teslim edilmez; kareleri `art/results/015c-red/` altına, nedeni sonuç dosyasına yazılır.

## Doğrulama
- **Sekme ölçütü (bağlayıcı):** kareler arası kafa farkı ≤ 2 px (64 tuvalde), ayak satırı sabit; yön × karakter tablosu.
- `validate_iso.py --kind object --max-colors 32`, pivot (32, 2).
- İki GIF (boy, girl; 8 yön ızgara) ve yetişkin erkekle yan yana bir GIF.

## Notlar
- Kod hazır: `IsoArt.CharacterWalk` çocukta `iso_char_{boy,girl}_r64_walk2_<yön>_NN` karelerini arar. Kare yoksa durma karesi + kod sallantısı sürer.
- Bebek bu setin 0,6 ölçeği (416 notu); bebek yürümez, ayrı iş gerekmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
