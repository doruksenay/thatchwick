---
id: "790"
title: "Doğal kaya zemini: taş ocağı tabanı ve kayalık kıyı (6 karo)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-21 (sohbet: "ikisini de yap"; belge 65b H2). Eski numarası 740 (diğer oturumun 740'ıyla çakıştığı için 790'a taşındı)
priority: 15
family: zemin
style_gate: false
cost_est: 6
cost_max: 12
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_rockbed_a.png
  - Assets/Art/Iso/iso_ground_rockbed_b.png
  - Assets/Art/Iso/iso_ground_rockbed_c.png
  - Assets/Art/Iso/iso_ground_quarry_a.png
  - Assets/Art/Iso/iso_ground_quarry_b.png
  - Assets/Art/Iso/iso_ground_quarry_c.png
created: 2026-11-21
---
# 790 Doğal kaya zemini

## Neden
Belge 65b H2: haritalardaki doğal taşlık alanlar (`Tepe ve taşlık` taş ocağı 264 karo, `Deniz kıyısı` kayalığı 83 karo,
`Güney Yamaçları` seki şeritleri) `FloorType.Stone` olarak yükleniyor ve oyunda **düzgün sıralı arnavut kaldırımı**
(yapı modunun taş döşemesi) olarak çiziliyor. Kayalık ya da ocak gibi okunmuyor; çimden cetvelle kesilmiş gibi ayrılıyor.
Zeminde doğal kaya karosu yok; bu iş onu üretir.

## Tarif
- Araç: `create_tiles_pro`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`,
  stil modu; `style_images` = `iso_ground_dirt_a`, `iso_ground_dirt_gravel_a`, `iso_ground_grass_a` (004e/612 ayarları).
  Aynalama ve döndürme yok. Tek karo (dolgu), her türden 3 çeşit, farklı tohum.
- **rockbed** (kayalık kıyı ve sırt): "natural weathered bedrock surface, flat grey-brown stone slabs with irregular
  cracks, small pebbles and a little dry moss in the cracks, no mortar lines, no regular grid, no paving pattern".
  Ton: `iso_ground_dirt_gravel_a`'dan biraz daha gri; doygunluk düşük.
- **quarry** (taş ocağı tabanı): "quarry floor: broken pale limestone chips and rubble on packed dusty ground, a few
  chisel marks, scattered small stone blocks flat on the ground, no walls, no objects sticking out of the tile".
  Ton: `rockbed`'den biraz açık ve sıcak (kireç taşı).
- Çeşitler yan yana dizildiğinde ızgara seçilmemeli (kenar/iç parlaklık 0,97-1,03); döşeme deseni (sıra, harç çizgisi)
  kesinlikle olmamalı.

## Doğrulama
- `validate_iso.py --kind ground` her karoda (opak 1024, elmas dışı 0, boşluk 0).
- `style.py check --family zemin`.
- 6x6 döşeme testi: rockbed a/b/c karışık, ortasında 2x2 quarry; kenarında çim. Sıra ya da ızgara görünmemeli.
- Yan yana sayfasına bugünkü `Stone` görünümüyle (taş döşeme) karşılaştırma koy.

## Notlar (kabulden sonra kod işi)
- Oyunda doğal taş ile yapı modunun taş döşemesi bugün aynı `FloorType.Stone`. Önerilen bağlama: harita taslağındaki
  `rock` pikseli (`MapTerrain.Rock`, şimdilik yalnızca kaya ve kütük serpiyor) zemin olarak da `rockbed` çizsin; taş ocağı
  için `quarry`. Taslaklarda tepe ocağı, kıyı kayalığı ve yamaç şeritleri `stone` → `rock` rengine çevrilir (kod oturumu,
  `tools/maps/draw.py` ya da elle). Oyuncunun yapı modunda döşediği taş zemin değişmez.
- Hangi karonun nereye çizileceği `IsoArt.Ground`'a eklenecek (yeni dal; `FloorType` değişmeden, harita arazisinden okunur
  ya da `FloorStyle` ile işaretlenir; kod oturumu seçer).

## Kod durumu (2026-11-21, fix/uyku-sabah-kademeli)
- Kod hazır: `IsoArt.NaturalGround(kind)` `iso_ground_rockbed_*` / `iso_ground_quarry_*` arar; `WorldView` taslakta `rock` (çim kalan) ve
  yeni `quarry` arazisinde bunu çizer. Görseller gelene kadar bugünkü çim ve taş çizilir. Zemin türü değişmez (kaya sırtı çim, ocak taş).
- Harita: tepedeki taş ocağı (119 karo) `stone` → `quarry` (#A8A090). Kıyı kayalığı main'de zaten `rock`.
- Görseller gelince yalnızca dosyaları koymak yeter; ek kod gerekmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
