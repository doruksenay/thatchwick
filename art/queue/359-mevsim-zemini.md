---
id: "359"
title: Mevsim zemini (sonbahar çimeni, karlı çimen, karlı toprak)
status: running
claimed_at: 2026-11-18T12:02Z
approved_by: user        # otomatik akış; kullanıcı onayı sohbette 2026-11-18 ("yap bunu", görsel analiz partisi)
family: zemin
style_gate: false
cost_est: 15
cost_max: 20
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_grass_autumn_a.png
  - Assets/Art/Iso/iso_ground_grass_autumn_b.png
  - Assets/Art/Iso/iso_ground_grass_snow_a.png
  - Assets/Art/Iso/iso_ground_grass_snow_b.png
  - Assets/Art/Iso/iso_ground_dirt_snow_a.png
  - Assets/Art/Iso/iso_ground_dirt_snow_b.png
created: 2026-11-18
---
# 359 Mevsim zemini

## Amaç
Görsel analiz (Project `26-gorsel-analiz.md`, P1 madde 8): zemin bütün yıl yaz çimeni. Oyun mevsimleri (sonbahar, kış) ve kar havası zeminde görünmüyor. Sonbaharda sararmış çimen, kışta ya da kar yağınca ince kar örtüsü. Kod, mevsime ve havaya göre bu karolara geçer (kar birkaç gün kalır, yavaş erir). Gün ışığı tonu ayrıca kodda (`DayLight`).

## Tarif
- Yöntem **002 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları (`iso_ground_grass_a/b`, `iso_ground_dirt_a`); 64x32 elmas, açı 30, derinlik 0, `tile_flat_top_px=2`, az tonlu, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.
- Numaralı istek:
  1. sonbahar çimeni a: `late autumn meadow grass, yellowed and olive, a few small fallen brown leaves`
  2. sonbahar çimeni b: a'nın varyasyonu (aynı ton, farklı yaprak deseni)
  3. karlı çimen a: `thin fresh snow cover over grass, a few grass blades poking through, cool white and pale blue-grey`
  4. karlı çimen b: a'nın varyasyonu
  5. karlı toprak a: `thin snow over dark ploughed earth, brown furrows showing through`
  6. karlı toprak b: a'nın varyasyonu
- Kar saf beyaz olmasın (gece ve kış tonu çarpımıyla patlamasın): en açık ton yaklaşık #E6EEF2.

## Doğrulama
- Her dosya için `validate_iso.py --kind ground --max-colors 6`, `style.py check --family zemin`.
- Döşeme testi: her çift 8x8 karma; dikiş parlaklık oranı sayıyla.
- Geçiş testi: yaz çimeni ile sonbahar çimeni ve karlı çimen yan yana 8x8 (yarısı biri, yarısı öteki); aynı aileden görünüyor mu.

## Notlar
- Adlar `outputs` listesindeki gibi. Pivot elmas merkezi (32,16).
- Kodda karşılığı (mevsim karosu seçimi, kar birikimi) bu iş review'a geçince yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
