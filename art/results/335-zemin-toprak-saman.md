unity-import: pending

# 335 Zemin stilleri: toprak, saman, saz (kısmi teslim)

- Araç: `create_tiles_pro`, stil modu, `style_images` = `iso_ground_dirt_a.png` (64x32, `get_upload_url` ile yüklendi). Maliyet çağrıdan önce okundu: 10-25 (tuval boyutuna göre), gerçek 10. cost_max 20 içinde. Tek çağrı, 6 numaralı istek; araç 16 varyasyon döndürdü (0-3 düz toprak, 4-7 benekli toprak, 8-11 saman, 12-15 saz). Tile id: `eea5397a-94d6-4200-b466-55447937d335`.
- Harcanan: 10 gen. `get_balance` sonrası: 625,1 (üç şerit paralel harcadığı için ortak bakiye).
- Seçim: earth_a = t1, earth_b = t6 (benekli, boş piksel yok), straw_a = t8, rush_a = t12 (maske dışı 21 piksel maskeyle silindi, içeriğe dokunulmadı).
- Teslim edilmeyen: **straw_b** (t9: maske içinde 3 boş piksel) ve **rush_b** (t14: 3 boş piksel). Doğrulama BAŞARISIZ; kurala göre yalnızca maskeyle kırpılır, delik doldurulmadı. Çözüm: 3 pikseli doldurma izni ya da yeni üretim (kullanıcı kararı). Tek deneme hakkı kullanıldı.
- Yan yana sayfası: `art/results/335-yanyana.png`

| Dosya | Boyut | Pivot (x, y alttan) | validate_iso | Stil denetimi |
|---|---|---|---|---|
| iso_floor_earth_a.png | 64x32 | 32, 16 | geçti | GEÇTİ (%100, x0.99) |
| iso_floor_earth_b.png | 64x32 | 32, 16 | geçti | GEÇTİ (%100, x0.99) |
| iso_floor_straw_a.png | 64x32 | 32, 16 | geçti | GEÇTİ (%96, x1,11) |
| iso_floor_rush_a.png | 64x32 | 32, 16 | geçti (21 dış piksel maskelendi) | GEÇTİ (%100, x1,01) |

Döşeme testi (8x8, adım 32,16): dikiş parlaklığı (kenar bandı / iç ortalama): earth_a 0,955, earth_b 0,982, straw_a 0,928, rush_a 0,967. Gölge pişirilmemiş, ışık sol-üstten. Masaüstü kopyası: yerel.

Not: ham çıktılar `art/tmp335` altında, commit edilmedi.
