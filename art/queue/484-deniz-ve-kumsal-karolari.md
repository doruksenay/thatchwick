---
id: "484"
title: "Deniz ve kumsal karoları (4)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); kullanıcı 20.11.2026 "üretilecek asset varsa kuyruğa at" (deniz kıyısı, belge 50)
priority: 20
family: zemin
style_gate: false
cost_est: 4
cost_max: 8
depends_on: ["346"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_ground_sea_a.png
  - Assets/Art/Iso/_aday/iso_ground_sea_b.png
  - Assets/Art/Iso/_aday/iso_ground_beach_a.png
  - Assets/Art/Iso/_aday/iso_ground_beach_b.png
created: 2026-11-20
---
# 484 Deniz ve kumsal karoları

## Amaç
Belge 50: gölün güneyine eklenen deniz kıyısı bölgesi. Kod (`IsoArt.Water`, `IsoArt.Beach`, `WorldView`) kıyıdaki su karolarında önce `iso_ground_sea_a/b`, kumsalda `iso_ground_beach_a/b` arar. Gelene kadar göl suyu mavi tonla, kumsal toprak karosu kum tonuyla çizilir.

## Tarif
**346 ile aynı yöntem:** `create_tiles_pro`, stil modu, `style_images` = 346 su karoları ve kabul edilmiş zemin karoları; 64x32 elmas, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.

| # | Dosya | Açıklama metni |
|---|---|---|
| 1 | `iso_ground_sea_a.png` | calm sea water, deeper blue-green than the pond, soft small waves |
| 2 | `iso_ground_sea_b.png` | same sea water with a few white foam flecks and sparkles |
| 3 | `iso_ground_beach_a.png` | pale yellow-grey sea sand, fine grain, a few tiny shells |
| 4 | `iso_ground_beach_b.png` | same sand with faint wave ripple marks and a small pebble |

Deniz karoları birbirine ve 346 su karolarına, kumsal karoları toprak ve çim karolarına dikişsiz geçmeli.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`, 3x3 döşeme testi (deniz a/b karışık, bir kenarda kumsal a/b).

## Notlar
Dosya adları kodda sabit; değişmesin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
