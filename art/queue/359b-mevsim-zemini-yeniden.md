---
id: "359b"
title: "Mevsim zemini yeniden (sonbahar çimeni, karlı çimen, karlı toprak)"
status: review
claimed_at: 2026-11-20T11:15Z
approved_by: user        # kullanıcı kararı 2026-11-20: 359 reddedildi, yeniden üretim
family: zemin
style_gate: false
cost_est: 15
cost_max: 20
depends_on: ["002"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_ground_grass_autumn_a_v2.png
  - Assets/Art/Iso/_aday/iso_ground_grass_autumn_b_v2.png
  - Assets/Art/Iso/_aday/iso_ground_grass_snow_a_v2.png
  - Assets/Art/Iso/_aday/iso_ground_grass_snow_b_v2.png
  - Assets/Art/Iso/_aday/iso_ground_dirt_snow_a_v2.png
  - Assets/Art/Iso/_aday/iso_ground_dirt_snow_b_v2.png
created: 2026-11-20
---
# 359b Mevsim zemini yeniden (sonbahar çimeni, karlı çimen, karlı toprak)

## Neden yeniden
359 kullanıcı tarafından reddedildi (2026-11-20, sohbette toplu inceleme). Reddedilen çıktılar karşılaştırma için `art/results/359-yanyana.png` içinde.

## Tarif
**359 iş dosyasındaki tarifin aynısı** (amaç, ölçüler, yöntem, doğrulama). Yalnızca şu farklar geçerli:

- **Karlı toprak (dirt_snow_a/b):** beyaz-kahve paralel şeritler olmamalı. Toprağın üstünde düzensiz, kenarları yumuşak kar yamaları olmalı, aralardan toprak görünmeli. a seyrek karlı, b daha yoğun karlı olsun.
- **Sonbahar çimeni ve karlı çimen:** 359'daki hâlleri iyi bulundu, aynı görünümü korumak için referans al (`art/results/359-yanyana.png`). Karlı çimen ile karlı toprak yan yana döşendiğinde kar rengi aynı olmalı.
- Zemin karosu kenarlarında koyu kontur ya da yan yüz olmamalı; 001/002 çimen karolarıyla aynı düz elmas biçimi.

- Çıktı adları `_v2` ekiyle `_aday/` altına yazılır; var olan dosyaların üstüne yazılmaz. Kabul edilince kod eşlemesi `_v2` dosyalarına çevrilir.
- Raporda 359'in reddedilen çıktısıyla yan yana sayfası (`art/results/359b-yanyana.png`) olmalı.

## Notlar
