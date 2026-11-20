---
id: "749"
title: "Muhabbet pozları, çocuk: gül, kıkırda, başını salla, şaşır, alkışla, anlat (48)"
status: running
claimed_at: 2026-11-20T21:07Z
approved_by: user        # kullanıcı 20.11.2026 ("çocuk muhabbet asset'leri ... alınsın"); Project belgeleri 67a §6, 67b §5
priority: 40
family: karakter
style_gate: false
cost_est: 48
cost_max: 60
depends_on: ["015c"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_laugh_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_giggle_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_listen_nod_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_gasp_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_clap_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_tell_story_{se,sw,nw,ne}_<NN>.png
  - art/results/749-*.md
created: 2026-11-20
---
# 749 Muhabbet pozları, çocuk (48)

## Amaç
Çocuklar toplu sohbette (şenlik, sofra, düğün) dinler, güler, masal ister; kendi aralarında da halka kurarlar (Project belgesi 67a §6). Ergenler yetişkin gövdesini kullanır (740-741).

## Tarif
- Karakter kimlikleri: 015c raporu (`art/results/015c-yurume-cocuk-64.md`; boy `6940a34e-...`, girl `9d58ae77-...`, tam kimlik `list_characters` ile). 64 px tuval. `get_character` ile doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 çapraz yön = 8 çağrı; 6 anahtar x 8 = 48 gen. Aynalama yok. 588'deki gibi her çağrının metninin başına yön eklenir (örn. `facing south-east: ...`).
- Eylem metinleri (`action_description`):
  - `laugh`: `a child laughing loudly, bouncing a little on the toes, hands on the belly, loop`
  - `giggle`: `a child giggling with both hands over the mouth, shoulders shaking, loop`
  - `listen_nod`: `a child listening with wide eyes and nodding eagerly, loop`
  - `gasp`: `a child gasping in surprise, both hands on the cheeks, short`
  - `clap`: `a child clapping happily and hopping once, loop`
  - `tell_story`: `a child telling a story excitedly with big arm gestures, loop`

## Ortak işleme (512 ile aynı)
588'deki "Ortak işleme" aynen.

## Doğrulama
018a ölçütleri her klip için. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
