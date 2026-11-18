---
id: "336"
title: Çatı dokuları: saz, ahşap şindıl, kiremit, arduvaz (eğim + sırt)
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: yapi
style_gate: false
cost_est: 20
cost_max: 25
depends_on: ["325"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_roof_thatch_a.png
  - Assets/Art/Iso/iso_roof_thatch_ridge.png
  - Assets/Art/Iso/iso_roof_shingle_a.png
  - Assets/Art/Iso/iso_roof_shingle_ridge.png
  - Assets/Art/Iso/iso_roof_tile_a.png
  - Assets/Art/Iso/iso_roof_tile_ridge.png
  - Assets/Art/Iso/iso_roof_slate_a.png
  - Assets/Art/Iso/iso_roof_slate_ridge.png
created: 2026-11-17
---
# 336 Çatı dokuları

## Amaç
Otomatik çatı (`RoofBuilder`): her iç odanın üstü açık karoları çatı karosu olur, uzun eksende sırtlı beşik çatı. Motor karoları sırta doğru yükseltir ve eğim yönüne göre gölgeler; bu iş yalnızca stil başına düz doku ister: `iso_roof_<ad>_a` (eğim) ve `_ridge` (sırt). Saz varsayılan ve ücretsizdir.

## Tarif
- Yöntem **002 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları; 64x32 elmas, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.
- Stil referansı ayrıca 325'te kabul edilen duvar.
- Açıklamalar: saz = thick golden thatch; şindıl = overlapping dark wooden shingles; kiremit = curved terracotta roof tiles; arduvaz = blue grey slate. Sırt karosunda tepe boyunca bir sırt örtüsü (saz demeti, ahşap kapak, mahya kiremidi, taş).

## Doğrulama
`validate_iso.py`, `style.py check --family yapi`, 6x4 çatı döşeme testi (sırt hattı kesintisiz).

## Notlar
Elle çatı çizimi ve kalkan duvarı (gable) sonraki aşama; bu dokular onlarda da kullanılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
