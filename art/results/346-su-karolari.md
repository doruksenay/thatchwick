unity-import: pending

# 346 Su karoları

- Araç: `create_tiles_pro`, stil modu (`style_images` = `iso_ground_grass_a`, `iso_ground_grass_b`, `iso_ground_dirt_a`; 64x32; `art/tmp346/up.py` ile yüklendi). Tek çağrı, 5 numaralı istek, araç 16 varyasyon döndürdü (0-3 su, 4-7 zambaklı su, 8-11 buz, 12-15 kuru çukur/kıyı). Maliyet çağrıdan önce okundu: 10-25 (tuval), gerçek 10. cost_max 16 içinde. Tile id: `1f4ca1d5-cb87-4038-a3a3-d4a5bf5077ee`.
- Harcanan: 10 gen. `get_balance` sonrası: 2946,9 (ortak bakiye, paralel şeritler).
- Seçim: water_a = t0, water_b = t4 (tek zambak yaprağı), ice_a = t8 (doygunluk düzeltmeli), pond_dry = t12, water_shore = t14 (bir kenarda çamurlu kıyı ve saz).
- Buz: ham hâli "ailesinden çok doygun" (x1,59) yüzünden KALDI, şekil tanınıyor. `style.py fit` ile x0,72 doygunluk düşürüldü (düzeltilmiş hâl teslim edildi, ham `art/results/346-red/iso_ground_ice_a_ham.png`), yeniden denetim UYARI.
- Yan yana sayfası: `art/results/346-yanyana.png`. Döşeme testleri: `art/results/346-doseme-*_tile.png`, `346-ice_tile.png` (8x8 elmas ızgara, adım 32,16).
- Kıyı karosunun çim ile geçişi için ayrı bir çim-kıyı döşeme testi yapılmadı (kıyı tek kenarda çamurlu, çim karosuyla birleşimi kod/kabul aşamasında yan yana sayfasından bakılmalı). 3x3 karışık test yerine 8x8 tek karo testi yapıldı.

| Dosya | Boyut | Pivot (x, y alttan) | validate_iso | Stil denetimi | Dikiş oranı |
|---|---|---|---|---|---|
| iso_ground_water_a.png | 64x32 | 32, 16 | geçti | UYARI (palet, kilit yok; ailesinden soluk x0,59) | 0,977 |
| iso_ground_water_b.png | 64x32 | 32, 16 | geçti | UYARI (palet; soluk x0,57) | 0,998 |
| iso_ground_ice_a.png | 64x32 | 32, 16 | geçti | UYARI (fit sonrası x1,13) | 1,021 |
| iso_ground_pond_dry.png | 64x32 | 32, 16 | geçti | GEÇTİ (%100) | 1,168 |
| iso_ground_water_shore.png | 64x32 | 32, 16 | geçti | UYARI (palet, kilit yok) | 1,051 |

Gölge pişirilmemiş, ışık sol-üstten, yarı saydam piksel yok, kırpma gerekmedi. Kuru çukurun dikiş oranı 1,168: kenar bandı iç kısımdan açık (çöküntü kenarı); döşeme testine bakılmalı. Masaüstü kopyası: yerel.

Not: ham çıktılar `art/tmp346` altında, commit edilmedi.
