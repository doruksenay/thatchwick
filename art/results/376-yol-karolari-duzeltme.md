unity-import: pending

# 376 Yol karoları düzeltme (2/5 teslim)

Yan yana sayfası: [376-yanyana.png](376-yanyana.png) (referanslar + path_b_v2, gravel_worn, path_a). Ham 16 karo: [376-ham-16.png](376-ham-16.png) (satır-öncelikli dizilim: sütun 0 patika, 1 çakıl, 2 yıpranmış çakıl, 3 kaldırım; tile_11/15 bozuk kaldırım). Döşeme: [path_b_v2](376-doseme-path-b.png), [gravel_worn](376-doseme-gravel-worn.png).

## Araç ve ayarlar
- `create_tiles_pro`, stil modu (340 ile aynı). `style_images` = grass_a, dirt_a, road_path_a, road_paved_worn (64x32, inline base64); `style_options` = renk paleti kapalı, outline/detail/shading açık (taş ve çakıl tonları çimen paletinden çıkamasın diye).
- Tek çağrı, 5 numaralı açıklama, 16 karo. Maliyet **10 generation** (cost_est 12, cost_max 18 içinde). Hız sınırı hatası yok.
- `get_balance`: çağrıdan önce 4665,90; paralel şeritler aynı hesabı kullandığı için sonrası yalnızca bu işi yansıtmaz (~4655,9).

## Seçim
| Dosya | Kaynak | Durum |
|---|---|---|
| iso_road_path_b_v2 | tile_0 | TESLİM: çıplak toprak, kenarda çim yok, 8 renk |
| iso_road_gravel_worn | tile_6 | TESLİM: izli, çamur çukurlu çakıl; maske dışı 5 piksel maskeyle kırpıldı |
| iso_road_gravel_a_v2 | tile_1/5/9/13 | TESLİM EDİLMEDİ: dördünde de maske içinde 8-12 boş piksel (validate BAŞARISIZ). Doku çakıl olarak iyi okunuyor |
| iso_road_paved_a | tile_3/7 | TESLİM EDİLMEDİ: tile_7'de 3 boş piksel, tile_3'te 37 boş + 32 dışarıda. Karolar kahverengimsi ve koyu kenarlı (tarife uymuyor) |
| iso_road_paved_broken_v2 | tile_11/15 | TESLİM EDİLMEDİ: 69 / 130 boş piksel; koyu kenar |

Tek deneme kuralı gereği yeniden üretilmedi, boşluk doldurulmadı. Reddedilen ham karolar `art/results/376-red/` (tile_1, 3, 7, 11). Öneri: gravel_a_v2, paved_a ve paved_broken_v2 için devam işi (outline_mode=segmentation denenebilir: kenarsız yüzey).

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/iso_road_path_b_v2.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_road_gravel_worn.png | 64x32 | 32 | 16 |

## Doğrulama
| Dosya | validate_iso (ground) | Renk | Stil denetimi | Doygunluk / aile | Dikiş oranı (kenar/iç) |
|---|---|---|---|---|---|
| path_b_v2 | geçti | 8 | GEÇTİ (kilit kurulmadı, palet uyumu uyarı niteliğinde) | x0.93 | 0,996 |
| gravel_worn | geçti | 33 | GEÇTİ | x0.86 | 1,055 |

Döşeme testi 8x8 elmas ızgara, (32,16) adımı; ikisi hedef 0,95-1,05 içinde (gravel_worn sınırda). Gölge pişirilmemiş, ışık sol-üstten. 3x3 yol + çim testi yapılmadı (path_b_v2 kenarında çim yok; kullanıcı gözüyle). Masaüstü kopyası: yerel.
