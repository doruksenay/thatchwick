---
id: "862"
title: "Zemin leke çıkartmaları: ağaç gölgesi, çiğnenmiş çim, uzun çayır, yonca (doğal zemin çeşitliliği)"
status: pending
approved_by: user        # kullanıcı 21.11.2026 02:41: "bizim haritamız doğal durmuyor"
priority: 10
family: zemin
style_gate: false
cost_est: 12
cost_max: 30
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_decal_shade_{a,b}.png
  - Assets/Art/Iso/_aday/iso_decal_worn_{a,b}.png
  - Assets/Art/Iso/_aday/iso_decal_tallgrass_{a,b}.png
  - Assets/Art/Iso/_aday/iso_decal_clover_patch_{a,b}.png
  - art/results/862-*.md
created: 2026-11-21
---
# 862 Zemin leke çıkartmaları

## Amaç
Çiftlik haritası (96x72) ve vahşi bölgeler tek tip düz çim: gerçek arazide orman altı koyu ve gölgeli, yol ve ev çevresi çiğnenmiş, açıklıkta uzun çayır ve yonca öbekleri olur. Çim-çayır geçiş seti (770) gelene kadar bu çeşitlilik **karo değiştirmeden**, çimin üstüne konan yarı saydam olmayan, kenarı düzensiz çıkartmalarla verilir. Kod bunları gürültüyle öbek öbek koyar (komşu karolardaki çıkartmalar üst üste biner, öbeğin kenarı yumuşak ve düzensiz görünür; elmas kenarı seçilmez).

## Tarif
- Araç: `create_map_object` ya da `create_tiles_pro` değil; mevcut çıkartmalarla aynı yöntem (`iso_decal_clover_v2`, `iso_decal_fern_v2` hangi araçla üretildiyse, `art/results/`'tan bak). Tuval **64x32**, saydam arka plan, gölge yok, dış çizgi yok.
- Biçim: elmasın ortasında, elmasın **%60-85'ini** kaplayan düzensiz, yırtık kenarlı bir leke; kenarlarda tek tük piksel kümeleri (dithering değil, küçük tutamlar). Hiçbir kenar elmas kenarına paralel düz çizgi olmaz. a ve b: aynı tür, farklı biçim.
  - `shade`: ağaç altı gölgeli çim, `dark cool green shaded grass under tree canopy, soft irregular patch` (ana çimden 2-3 ton koyu, aynı renk ailesi).
  - `worn`: çiğnenmiş çim, `trampled short grass with bits of bare warm earth showing, irregular patch` (yol kenarı ve avlu).
  - `tallgrass`: uzun çayır, `patch of taller uncut meadow grass blades, slightly yellow-green, irregular` (açıklık, tarla kenarı).
  - `clover_patch`: yonca öbeği, `dense low clover leaves patch, a few tiny white clover flowers, irregular` (mevcut `iso_decal_clover_v2`'den büyük ve yoğun).
- Renk: `python tools/art/style.py fit --family zemin`; ton ana çim karosuyla (`iso_ground_grass_a`) aynı ailede kalır.

## Doğrulama
- `validate_iso.py <dosya> --kind ground`, `style.py check --family zemin`.
- Öbek testi (`art/results/862-obek.png`): 16x16 çim döşemesi üstüne her tür için gürültüyle 20-40 karoluk bir öbek (komşu karolarda aynı türün a/b'si karışık). Öbeğin dış kenarında elmas biçimi ya da ızgara seçilmemeli; seçiliyorsa nedenini yaz.

## Notlar
Kod bağlantısı (`GameRootYard.FloraAt`: orman bandına yakın `shade`, yol ve bina çevresinde `worn`, açıklıkta gürültüyle `tallgrass`/`clover_patch`) görseller gelince ayrı PR'da.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
