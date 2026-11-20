---
id: "488"
title: "Tavşan yastığı (yapay yuva tümseği)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: esya
style_gate: false
cost_est: 3
cost_max: 6
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_warren_se.png
created: 2026-11-20
---
# 488 Tavşan yastığı

## Amaç
`item.warren` (belge 48 §8.3): ortaçağda tavşanlar için kurulan uzun, alçak toprak tümseği (pillow mound). Dış barınak, 6 tavşan.

## Tarif
Eşya ailesinin izometrik tarifi (`create_isometric_tile` yerine `create_image_pixflux` + izometrik görünüm, mevcut eşya işleri gibi), 112x80, `no_background=true`.
Açıklama metni: `long low grassy earth mound with several small burrow holes on its side, medieval rabbit warren (pillow mound), isometric view`.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Pivot items.json'da (56, 32).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
