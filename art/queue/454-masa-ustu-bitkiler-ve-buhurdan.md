---
id: "454"
title: "Masa üstü saksılar, eğrelti teknesi, buhurdan, kuş yalağı (12)"
status: accepted
claimed_at: 2026-11-20T06:49Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (ev bitkileri ve yeni dekor), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 12
cost_max: 20
depends_on: ["383"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_pot_lily_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_narcissus_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_violet_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_cyclamen_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_snowdrop_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_crocus_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_lavender_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_mint_se.png
  - Assets/Art/Iso/_aday/iso_item_pot_chamomile_se.png
  - Assets/Art/Iso/_aday/iso_item_fern_trough_se.png
  - Assets/Art/Iso/_aday/iso_item_censer_se.png
  - Assets/Art/Iso/_aday/iso_item_birdbath_se.png
created: 2026-11-19
---
# 454 Masa üstü saksılar, eğrelti teknesi, buhurdan, kuş yalağı (12)

## Amaç
Belge 45 ek: masa ve pencere üstü canlı saksılar (her mevsimde çiçek açan bir tür: nergis ve menekşe ilkbahar, zambak yaz, çiğdem sonbahar, siklamen ve kardelen kış).

## Tarif
**383 ile aynı yöntem** (masa üstü küçük eşya, pivot tabanda). Kuş yalağı 302 ölçeği.

| # | items.json id | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.pot_lily` | `iso_item_pot_lily_se.png` | yuva O | `clay pot with tall white madonna lilies` |
| 2 | `item.pot_narcissus` | `iso_item_pot_narcissus_se.png` | yuva K | `small clay pot with white and yellow narcissus` |
| 3 | `item.pot_violet` | `iso_item_pot_violet_se.png` | yuva K | `small clay pot with purple sweet violets` |
| 4 | `item.pot_cyclamen` | `iso_item_pot_cyclamen_se.png` | yuva K | `small clay pot with pink wild cyclamen and marbled leaves` |
| 5 | `item.pot_snowdrop` | `iso_item_pot_snowdrop_se.png` | yuva K | `small clay pot with white snowdrops` |
| 6 | `item.pot_crocus` | `iso_item_pot_crocus_se.png` | yuva K | `small clay pot with lilac crocus flowers` |
| 7 | `item.pot_lavender` | `iso_item_pot_lavender_se.png` | yuva O | `clay pot with flowering lavender` |
| 8 | `item.pot_mint` | `iso_item_pot_mint_se.png` | yuva K | `small clay pot with bushy mint` |
| 9 | `item.pot_chamomile` | `iso_item_pot_chamomile_se.png` | yuva K | `small clay pot with chamomile daisies` |
| 10 | `item.fern_trough` | `iso_item_fern_trough_se.png` | yuva O | `low clay trough with ferns and green moss` |
| 11 | `item.censer` | `iso_item_censer_se.png` | yuva O | `small pierced brass incense burner with a wisp of smoke` |
| 12 | `item.birdbath` | `iso_item_birdbath_se.png` | 1x1, dış | `shallow stone bird bath on a short pedestal` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Eşya → dosya eşlemesi `Assets/Scripts/Game/IsoArt.cs`'te hazır (bu adlarla); görsel gelince kendiliğinden kullanılır. Pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.
- Teslim `cp -n`, `.meta` yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
