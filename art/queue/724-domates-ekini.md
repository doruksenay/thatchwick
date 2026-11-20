---
id: "724"
title: "Domates ekini (3)"
status: running
claimed_at: 2026-11-20T14:42Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (domates salça için bilinçli dönem istisnası, Project belgesi 60a-E1)
priority: 20
family: bitki
style_gate: false
cost_est: 3
cost_max: 6
depends_on: ["374"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_tomato_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_tomato_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_tomato_3_se.png
created: 2026-11-20
---
# 724 Domates ekini (3)

## Amaç
Domates, salça için bilinçli dönem istisnası (Project belgesi 60a-E1; kahve ve siyah çaydan sonra üçüncüsü). Ekin kodda açıldı (`item.crop_tomato`). Üç evre: filiz, gelişmiş bitki, **olgun**.

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_tomato` (domates) | `iso_crop_tomato_1_se.png` | `tomato seedlings with small serrated leaves` |
| `item.crop_tomato` (domates) | `iso_crop_tomato_2_se.png` | `bushy tomato plant tied to a short stake with small yellow flowers` |
| `item.crop_tomato` (domates) | `iso_crop_tomato_3_se.png` | `staked tomato plant heavy with round red fruits` |

- Maliyet 3 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): domates bu işte bilinçli istisnadır; başka Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
