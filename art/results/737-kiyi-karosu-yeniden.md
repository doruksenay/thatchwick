unity-import: pending

# 737 Kıyı karosu yeniden (elmasa oturan su-kara kenarı)

- Araç: `create_tiles_pro`, stil modu, `style_images` = `iso_ground_water_a`, `iso_ground_grass_a`, `iso_ground_dirt_a` (satır içi base64, 64x32), `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`. Tek çağrı, 16 varyasyon (tile id `005b1a32-5734-466c-a5aa-d0f27145485d`). Maliyet çağrıdan önce okundu: 10-25 (tuval), gerçek 10; cost_max 20 içinde.
- Harcanan: 10 gen. `get_balance` sonrası (çağrı öncesi 9954 okundu, ortak bakiye, paralel şeritler): okunan 9887.
- Seçim: a (`_v2`) = t7 (maske tam, kara sağ üst kenarda, saz kümeleri çamurda); b (`_v2_b`) = t4 (su rengi water_a'ya en yakın, 2 elmas-kenarı pikseli komşu su rengiyle dolduruldu; maskeyle ilgili tek düzeltme). t5 saz için denendi ama su rengi ±6 dışında (-9) kaldı. Ham sayfa: `art/results/737-ham-sayfa.png`.
- Yan yana sayfası: `art/results/737-yanyana.png`. Döşeme testleri: `art/results/737-doseme-*.png` (su zemin, karo sağ üst/aynası sol üst, çim öte; yerleşim kabadır, kod/kabul aşamasında oyunda bakılmalı).

| Dosya | Boyut | Pivot (x, y alttan) | validate_iso | Maske (opak / dışı / boş / üst-orta-alt) | Stil denetimi | Dikiş oranı |
|---|---|---|---|---|---|---|
| iso_ground_water_shore_v2.png | 64x32 | 32, 16 | geçti | 1024 / 0 / 0 / 2-62-2 | GEÇTİ (%100, x0,72) | 1,049 |
| iso_ground_water_shore_v2_b.png | 64x32 | 32, 16 | geçti | 1024 / 0 / 0 / 2-62-2 | GEÇTİ (%100, x0,69) | 1,042 |

- Su kısmı ortalaması water_a'ya göre (sol alt su bölgesi, R/G/B): v2 -4,7/-5,2/-5,9; v2_b -2,5/-3,0/-3,2 (ölçüt ±6: ikisi içinde).
- Gölge pişirilmemiş, ışık sol-üstten, yarı saydam piksel yok.
- Sorun/not: kara şeridi tam köşelere eşit uzaklıkta değil, hafif eğri (t7'de alta doğru biraz geniş); kullanıcı yan yana sayfasından bakmalı. Masaüstü kopyası: yerel.
