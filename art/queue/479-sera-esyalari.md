---
id: "479"
title: "Sera eşyaları: fidan rafı ve saksı tezgâhı (2)"
status: review
claimed_at: 2026-11-20T11:06Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("sera oda tipi: bunu yapalım")
priority: 40
family: esya
style_gate: false
cost_est: 2
cost_max: 6
depends_on: ["303"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_seedling_rack_se.png
  - Assets/Art/Iso/_aday/iso_item_potting_bench_se.png
created: 2026-11-20
---
# 479 Sera eşyaları: fidan rafı ve saksı tezgâhı (2)

## Amaç
Sera oda tipi (Project belgesi 45c, sera eki): bakımlı sera için bir sera iş eşyası gerekir. İkisi de seraya ait sayılır (`tags: greenhouse, greenhouse_work`).

## Tarif
**303 ile aynı yöntem** (iç mekân eşyası, `se` yönü; aynalama kodla).

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.seedling_rack` | `iso_item_seedling_rack_se.png` | 1x1 | three-tier wooden rack with small clay pots of green seedlings |
| 2 | `item.potting_bench` | `iso_item_potting_bench_se.png` | 2x1 | rustic wooden potting bench with soil heap, trowel and stacked clay pots |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok (plastik saksı, metal kürek yok; ahşap ve pişmiş toprak).

## Notlar
- Kod görseli adla bulur (`IsoArt.Get`), gelene kadar renkli şekil çizer.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
