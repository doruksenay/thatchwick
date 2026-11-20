---
id: "468c"
title: "Şifalı merhem: ikon ve dünya görseli"
status: pending
approved_by: user
family: esya
style_gate: false
cost_est: 3
cost_max: 8
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_healing_salve.png
  - Assets/Art/Iso/_aday/iso_item_healing_salve_se.png
  - art/results/468c-sifali-merhem.md
created: 2026-11-20
---
# 468c Şifalı merhem

## Amaç
Belge 14 §3F ve 17: insan yarası için şifalı merhem (mürver çiçeği + bal). İyileşmeyi yarıya, iz şansını üçte bire indirir. `InjurySystem.ApplySalve` hazır, eşya henüz yok. Eşya kimliği `healing_salve` (hayvan merhemi `salve` etkileşiminden ayrı).

## Tarif
- **İkon:** C oturumundaki ürün ikonu yöntemi, `icon_item_<id>`. `create_map_object` 32x32 + `, no face, no eyes, no creature`.
  - Açıklama: `a small round clay pot with a cloth lid tied with string, a sprig of white elderflowers beside it`.
- **Dünya:** masada duran küçük eşya, `iso_item_healing_salve_se.png`, 304'teki küçük eşya tarifiyle.

## Doğrulama
- `validate_iso.py`, `style.py check --family esya`.
- İkon 32x32 ≤16 renk.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
