---
id: "478d"
title: "Beceri tamamlama: Çatkı ve Yörük takımları (6)"
status: review
claimed_at: 2026-11-20T11:05Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("geliştirmeyi tamamla"), Project belgesi 45 bölüm 9
priority: 40
family: esya
style_gate: false
cost_est: 6
cost_max: 14
depends_on: ["302"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_catki_table_se.png
  - Assets/Art/Iso/_aday/iso_item_catki_chair_se.png
  - Assets/Art/Iso/_aday/iso_item_catki_chest_se.png
  - Assets/Art/Iso/_aday/iso_item_yoruk_table_se.png
  - Assets/Art/Iso/_aday/iso_item_yoruk_chair_se.png
  - Assets/Art/Iso/_aday/iso_item_yoruk_chest_se.png
created: 2026-11-20
---
# 478d Beceri tamamlama: Çatkı ve Yörük takımları (6)

## Amaç
Belge 45 tablo: Marangozluk 5 Çatkı takımı, 6 Yörük takımı (öncekiler: Köylü, Zanaatkâr, Tüccar, Asil).

## Tarif
**302 ölçeği**, `se`. Çatkı: geçmeli, çivisiz meşe. Yörük: keçe ve kilim, kırmızı-lacivert göçebe deseni.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.catki_table` | `iso_item_catki_table_se.png` | 2x1 | `sturdy pegged oak trestle table with crossed legs, no nails visible` |
| 2 | `item.catki_chair` | `iso_item_catki_chair_se.png` | 1x1 | `pegged oak chair with crossed back slats` |
| 3 | `item.catki_chest` | `iso_item_catki_chest_se.png` | 1x1 | `pegged oak chest with dovetail joints` |
| 4 | `item.yoruk_table` | `iso_item_yoruk_table_se.png` | 2x1 | `low round wooden nomad dining board on a woven kilim` |
| 5 | `item.yoruk_chair` | `iso_item_yoruk_chair_se.png` | 1x1 | `thick felt floor cushion with red and blue nomad pattern` |
| 6 | `item.yoruk_chest` | `iso_item_yoruk_chest_se.png` | 1x1 | `stack of folded kilims and felt bedding on a low wooden chest (yüklük)` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- styleSet "Çatkı" ve "Yörük". Takım içi tutarlılık için önce masa, sonra ötekiler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
