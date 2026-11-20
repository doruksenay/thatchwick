---
id: "484d"
title: "Deniz ve kumsal karoları (4)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); kullanıcı 20.11.2026 "üretilecek asset varsa kuyruğa at" (deniz kıyısı, belge 50)
priority: 10        # oyun dünyası (art/budget.md Öncelik); tester raporu 66
family: zemin
style_gate: false
cost_est: 10
cost_max: 15     # kullanıcı 21.11.2026: işçinin önerisiyle (create_tiles_pro çağrı başına 10)
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
- 2026-11-21 (tester raporu 66): iş numarası çift kullanılıyordu, `queue_ready.py` bu işi hiç görmüyordu; numara "484d" yapıldı.
Dosya adları kodda sabit; değişmesin.

## Sorular
- `create_tiles_pro` aracı açıklaması: bağımsız karolar için çağrı başına 10 generation (64px'te 10 ya da 15), 16 hücrelik sayfa tek fiyat. `cost_est: 4` ve `cost_max: 8` ile uyuşmuyor (en az 10 > cost_max 8, tahminin 2,5 katı). Maliyet kuralı gereği çağrı yapılmadı, harcama 0. Öneri: `cost_est: 10`, `cost_max: 15` yapılıp iş `pending`'e alınsın (tek çağrıyla 4 numaralı istek, 346 gibi). Bakiye 9983.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- **Cevap (kullanıcı 21.11.2026):** öneri uygulandı; `cost_est: 10`, `cost_max: 15`, iş yeniden `pending`.
