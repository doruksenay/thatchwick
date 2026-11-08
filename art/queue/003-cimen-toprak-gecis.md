---
id: "003"
title: Çimen-toprak geçiş seti
status: blocked
approved_by: auto
cost_est: 10
cost_max: 15
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_grass_dirt_00.png ... iso_ground_grass_dirt_15.png
created: 2026-11-08
---
# 003 Çimen-toprak geçiş seti

## Amaç
Haritada çimen ile toprak arasında dikişsiz geçiş (yol ve işlenen arsa kenarları). Adım 10-11'de kenar bilgisine dayalı otomatik karo yerleşimi bunu kullanacak.

## Tarif
- Araç: `create_tiles_pro`, `tile_feature="tileset"` (16 karoluk köşe geçiş seti).
- Bu modda `style_images` verilemez (araç açıklaması). Bunun yerine `palette` = kabul edilmiş `iso_ground_grass_a.png` ve `iso_ground_dirt_a.png` karolarındaki renkler; her piksel bu renklerden olmalı.
- Ayarlar: isometric, 64 px, açı 30, `tile_depth_ratio=0`, `tile_flat_top_px=2`. Kenar kıvrımlılığı için parametre yok: açıklama metnine "gently curved, organic edge between grass and dirt, no hard straight line" yaz.
- Karo sayısını ve hangi köşe kombinasyonunu verdiğini rapora yaz (kodlama: hangi karo hangi köşe maskesi).

## Doğrulama
- Her karo için `validate_iso.py --kind ground`.
- Dikiş: tüm 16 karoyu köşe kodlarına göre 6x6 ızgarada döşe, komşu karolar arasında görünür çizgi ya da renk sıçraması var mı, oranı yaz.
- Karolar çimen/toprak karolarıyla aynı palette mi.

## Notlar
Adlar `iso_ground_grass_dirt_<nn>.png` (nn = 00-15, köşe kodu sırası). Dikişsiz değilse `failed` yaz, tekrar deneme.

## Sorular
BLOKE (2026-11-08, Unity/geliştirme sohbeti): 003 işi zemin oturumuna verildi (create_tiles_pro tileset, palette modu). Çift harcama olmasın diye bloke. Zemin oturumu bitince kullanıcı durumu accepted/rejected yapar ya da pending'e geri alır.
