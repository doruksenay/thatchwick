---
id: "478c"
title: "Beceri tamamlama: iç mekân eşyaları ve bayram süslemesi (4)"
status: running
claimed_at: 2026-11-20T10:57Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("geliştirmeyi tamamla"), Project belgesi 45 bölüm 9
priority: 40
family: esya
style_gate: false
cost_est: 4
cost_max: 10
depends_on: ["303"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_cheese_rack_se.png
  - Assets/Art/Iso/_aday/iso_item_silkworm_rack_se.png
  - Assets/Art/Iso/_aday/iso_item_drum_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_festival_bunting_sw_ne.png
created: 2026-11-20
---
# 478c Beceri tamamlama: iç mekân eşyaları ve bayram süslemesi (4)

## Amaç
Belge 45: Hayvancılık 6 peynir rafı ve 8 ipek böceği rafı, Müzik 6 davul, Ev düzenleme 9 bayram süslemesi.

## Tarif
Eşyalar: **303 ile aynı yöntem** (`se`). Bayram süslemesi: duvar eşyası, bahar çelengi (`iso_wall_item_spring_garland_sw_ne`) yöntemi.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.cheese_rack` | `iso_item_cheese_rack_se.png` | 1x1 | `wooden shelving rack with round cheese wheels aging` |
| 2 | `item.silkworm_rack` | `iso_item_silkworm_rack_se.png` | 1x1 | `tiered wooden trays with mulberry leaves and white cocoons` |
| 3 | `item.drum` | `iso_item_drum_se.png` | 1x1 | `large wooden double-headed davul drum with rope tension and a stick` |
| 4 | `item.festival_bunting` | `iso_wall_item_festival_bunting_sw_ne.png` | duvar, üst bant | `red and yellow cloth bunting garland with small triangular flags` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- Kod görseli adla bulur, gelene kadar renkli şekil çizer.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
