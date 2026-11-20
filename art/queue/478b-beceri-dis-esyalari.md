---
id: "478b"
title: "Beceri tamamlama: dış mekân eşyaları (5)"
status: running
claimed_at: 2026-11-20T10:57Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("geliştirmeyi tamamla"), Project belgesi 45 bölüm 9
priority: 40
family: esya
style_gate: false
cost_est: 5
cost_max: 12
depends_on: ["302"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_flower_bed_se.png
  - Assets/Art/Iso/_aday/iso_item_storm_bell_se.png
  - Assets/Art/Iso/_aday/iso_item_brood_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_crayfish_trap_se.png
  - Assets/Art/Iso/_aday/iso_item_gravestone_se.png
created: 2026-11-20
---
# 478b Beceri tamamlama: dış mekân eşyaları (5)

## Amaç
Belge 45: Bahçıvanlık 6 çiçek tarhı, Hava bilgisi 8 fırtına çanı, Hayvancılık 6 kuluçka sepeti, Balıkçılık 5 kerevit tuzağı, Taş ustalığı 8 mezar taşı.

## Tarif
**302 ölçeği**, `se` yönü; aynalama kodla.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.flower_bed` | `iso_item_flower_bed_se.png` | 2x1 | `low stone-edged flower bed with red poppies, blue cornflowers, white daisies and lavender` |
| 2 | `item.storm_bell` | `iso_item_storm_bell_se.png` | 1x1 | `bronze bell hanging in a small wooden frame on a post` |
| 3 | `item.brood_basket` | `iso_item_brood_basket_se.png` | 1x1 | `round woven straw brooding basket with hay and eggs` |
| 4 | `item.crayfish_trap` | `iso_item_crayfish_trap_se.png` | 1x1 | `small cylindrical woven willow crayfish trap with funnel opening` |
| 5 | `item.gravestone` | `iso_item_gravestone_se.png` | 1x1 | `white marble headstone with carved inscription lines, rounded top` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- Kod görseli adla bulur (`IsoArt.Get`), gelene kadar renkli şekil çizer; dosya adı değişmesin.
- Çiçek tarhında kadife çiçeği (Yeni Dünya) olmasın.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
