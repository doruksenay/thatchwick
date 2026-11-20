---
id: "587"
title: "Kedi balık kapma ve köpek dilenme animasyonları (16)"
status: review
claimed_at: 2026-11-20T17:15Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 50
family: hayvan
style_gate: false
cost_est: 16
cost_max: 32
depends_on: ["375", "379"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_animal_cat_{sneak,pounce,run_fish}_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_animal_dog_beg_{se,sw,nw,ne}_<NN>.png
  - art/results/587-*.md
created: 2026-11-20
---
# 587 Kedi balık kapma ve köpek dilenme animasyonları (16)

## Amaç
Project belgesi 60c §2.4-2.5: kedi açıkta kalan balığı sinsice yaklaşıp kapar ve kaçar; köpek sofranın yanında oturup dilenir.

## Tarif
- Kedi: 375'te kabul edilen kedi (tekir, beyaz ayaklı); köpek: 379'daki temel köpek. Diğer kedi ve köpek tipleri (48 genetik) bu kareler kabul edilince ayrı işle türetilir.
- 4 çapraz yön, 8 kare, `animate_character` (hayvan setlerinin yürüme işindeki yöntem). 4 klip x 4 yön = 16 gen.
- Eylem metinleri:
  - `sneak`: `creeping forward very low with the belly near the ground, tail tip twitching, slow loop`
  - `pounce`: `crouching, wiggling the hindquarters and springing forward to grab something with the mouth`
  - `run_fish`: `running fast with a small silver fish held in the mouth, loop`
  - `beg` (köpek): `sitting upright looking up with the head tilted, tail sweeping the ground, one front paw lifting now and then, loop`

## Doğrulama
Hayvan setlerinin ölçütleri (375/379): ayak satırı sabit, palet ve boy durma karesiyle aynı. `run_fish`'te balık okunur olmalı. Klip başına GIF `art/results/` altına.

## Notlar
Kod: `IsoArt.Animals` tablosuna klip anahtarları eklenir (kod oturumu, koku ve kedi partisi).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
