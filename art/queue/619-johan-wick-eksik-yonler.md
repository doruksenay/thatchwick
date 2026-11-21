---
id: "619j"
title: "Johan Wick: eksik yönler (yürüme güneybatı, sevme üç yön)"
status: running
claimed_at: 2026-11-21T01:22Z
approved_by: user        # otomatik akış (art/budget.md); 510 raporundaki eksikler, Project belgesi 55d
priority: 50
family: karakter
style_gate: false
cost_est: 4
cost_max: 8
depends_on: ["510"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_johan_wick_r84_walk2_sw_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_johan_wick_r84_pet_{se,sw,nw}_<NN>.png
  - art/results/619-*.md
created: 2026-11-20
---
# 619 Johan Wick: eksik yönler

## Amaç
510'da dört klip teslim edilemedi: yürüme güneybatı (kafa farkı 4 px), sevme güneydoğu (kareye köpek çizildi), güneybatı (hareketsiz), kuzeybatı (gövde eğilmiyor). Kod eksik yönde eldeki yöne düşüyor (sevme yalnızca kuzeydoğu); Johan köpeği güneyden severken sırtı dönük görünüyor.

## Tarif
- Karakter: johan_wick `8d5fc676-e8ba-4988-ba86-d18df6ab56c8`. Yöntem ve işleme 510 ile aynı (`animate_character`, `pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = yönün durma karesi). 4 çağrı, 4 gen.
- PixelLab aynı metinli isteği eski klibe eşlediği için (015c notu) metinler 510'dan farklıdır:
  - `walk2` (`south-west`): `facing south-west: walking at a calm steady pace, a full stride cycle with the left foot then the right foot stepping equally, head and shoulders perfectly level, no bouncing, no limping`
  - `pet` (`south-east`, `south-west`, `north-west`; başına yön eklenir, örn. `facing south-east: ...`): `bending the upper body far forward and down, one arm reaching low toward the ground, the hand making slow stroking motions in the air near knee height, then straightening a little and bending again, no animal drawn, no dog, nothing on the ground, empty space in front`

## Doğrulama
510 ile aynı. Yürümede kafa farkı ≤ 3 px. Sevmede: karede hayvan yok, gövde belirgin eğik (kafa en az 8 px alçalır), ardışık kareler aynı değil.

## Notlar
- 2026-11-21 (tester raporu 66): iş numarası çift kullanılıyordu, `queue_ready.py` bu işi hiç görmüyordu; numara "619j" yapıldı.
- 510'un `art/results/510-red/` altındaki kareleri teslim edilmez; bu iş onların yerine geçer.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
