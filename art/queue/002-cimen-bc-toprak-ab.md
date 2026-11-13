---
id: "002"
title: Çimen b, c ve toprak a, b
status: blocked
approved_by: auto
family: zemin
style_gate: false
cost_est: 10
cost_max: 15
depends_on: ["001"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_grass_b.png
  - Assets/Art/Iso/iso_ground_grass_c.png
  - Assets/Art/Iso/iso_ground_dirt_a.png
  - Assets/Art/Iso/iso_ground_dirt_b.png
created: 2026-11-08
---
# 002 Çimen b, c ve toprak a, b

## Amaç
Zemin çeşitliliği: aynı stilde 2 çimen varyasyonu ve 2 toprak karosu.

## Tarif
- Araç: `create_tiles_pro`, stil modu. `style_images` = kabul edilmiş `iso_ground_grass_a.png` (64x32).
- Numaralı istek: 1) çimen varyasyonu b, 2) çimen varyasyonu c (aynı ton ailesi, a'dan hafif farklı yaprak deseni), 3) toprak a (kuru kahverengi toprak), 4) toprak b (toprak a'nın varyasyonu).
- Ayarlar 001 ile aynı (64 px, açı 30, derinlik 0, `tile_flat_top_px=2`, az tonlu, gölge yok).

## Doğrulama
- Her dosya için `validate_iso.py --kind ground --max-colors 6`.
- Döşeme testi: a, b, c karma karışık 8x8; toprak a, b karma 8x8; dikiş parlaklık oranı sayıyla.
- Palet 001 ile aynı aileden mi (görsel not).

## Notlar
Adlar `outputs` listesindeki gibi. Pivot elmas merkezi (32,16).

## Sorular
BLOKE (2026-11-08, Unity/geliştirme sohbeti): 002 işi zemin oturumunda yürüyor: adaylar _aday altında, seçimler yapıldı (çimen b=bc_cand1, c=bc_cand4, toprak a=dirt_cand6, toprak b=dirt_cand1 + en çok 3 piksellik delik doldurma). Çift harcama olmasın diye bloke. Zemin oturumu bitince kullanıcı durumu accepted/rejected yapar ya da pending'e geri alır.
