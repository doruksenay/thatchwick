---
id: "363"
title: Kazı ışıkları (mum, kandil, meşale) ve ışık halkası
status: review
claimed_at: 2026-11-18T13:39Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 28 (18.11.2026, "tek seferde yapalım")
priority: 10
family: esya
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["301", "306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_mine_candle_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_lamp_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_torch_se.png
  - Assets/Art/Iso/iso_fx_light_ring.png
created: 2026-11-18
---
# 363 Kazı ışıkları (mum, kandil, meşale) ve ışık halkası

## Amaç
Kazıya konan ışıklar (MineSystem.PlaceLight: iç yağı mumu, kandil, meşale) ve ışık yarıçapı halkası.

## Tarif
Yöntem 301 ile aynı; **ışık kaynağı kuralı**: `check`'e `--light` eklenir.
- `iso_item_mine_candle_se.png` 48x56: `tallow candle stuck on a flat stone on the ground, small flame`
- `iso_item_mine_lamp_se.png` 48x48: `small clay oil lamp on the ground with a flame`
- `iso_item_mine_torch_se.png` 48x80: `wooden torch wedged upright between stones, burning`
- `iso_fx_light_ring.png` 192x96: saydam, ortası açık sarı yarı saydam elips, kenara doğru sönen (pixellab yerine `tools/art` betiğiyle de üretilebilir; üretilirse 0 gen).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
