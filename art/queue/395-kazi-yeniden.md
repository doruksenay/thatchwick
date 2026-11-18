---
id: "395"
title: Kazı yeniden: korsuz çatlak taş, ahşap çatkı, dren çukuru, kandil, kristal
status: pending
approved_by: user        # otomatik akış (art/budget.md); kullanıcı 18.11.2026: "önerilerini uygula" (360-367 yeniden üretim)
priority: 10
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["360", "362", "363", "366"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_rock_cracked_b.png
  - Assets/Art/Iso/_aday/iso_item_mine_support_wood_b_se.png
  - Assets/Art/Iso/_aday/iso_item_drain_pit_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_lamp_se.png
  - Assets/Art/Iso/_aday/iso_decor_relic_crystal.png
created: 2026-11-18
---
# 395 Kazı yeniden: korsuz çatlak taş, ahşap çatkı, dren çukuru, kandil, kristal

## Amaç
360-366'da teslim edilmeyen ya da sorunlu çıkan parçalar (`art/results/36x-*.md`, `36x-red/`). Var olan PNG'lerin üzerine yazılmaz; düzeltmeler `_b` adıyla gelir.

## Tarif
Her parçanın yöntemi, tuvali ve doğrulaması kendi ana işiyle aynı. Farklar:
- **Çatlak taş** (360 yöntemi): 360'ta turuncu kor çıktı. Metin: `low cut block of dark rock with black soot stains and many fine cracks, cold, no fire, no glow, no orange`. Kor ya da turuncu piksel varsa KALDI.
- **Ahşap çatkı** (362): 362'de A biçimli tezgâh ve mor ton çıktı. Metin: `mine timber support: exactly two vertical pine posts and one horizontal beam across the top, square frame, plain brown wood, viewed isometric`. Referans: 362'deki çürüyen çatkının biçimi (kiriş doğru okunuyordu), rengi `fit --family esya` ile.
- **Dren çukuru** (362-red'deki aday KALDI): metin aynı; tuval 80x48, çukur ayak izinin ortasında, çakıl gri.
- **Kandil** (363-red: renk 55 > 48, ışık %41): `small plain clay oil lamp with a tiny flame`, `reduce_colors` 40, alev küçük tutulur; `--light`.
- **Kristal** (366-red): `small cluster of three clear quartz crystals on a grey stone base, muted`, doygunluk düşük; `fit --family esya`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
