unity-import: pending

# 340 Yol karoları (5/7 teslim; gravel_worn ve paved_a TESLİM EDİLMEDİ)

Yan yana sayfası: [340-yanyana.png](340-yanyana.png). Döşeme testleri: [path](340-doseme-path.png), [gravel](340-doseme-gravel.png), [paved](340-doseme-paved.png).

## Araç ve ayarlar
- `create_tiles_pro`, stil modu (333/334 ile aynı). `style_images` = `iso_ground_grass_a.png`, `iso_ground_dirt_a.png` (64x32); `style_options` = renk paleti kapalı, outline/detail/shading açık (taş ve çakıl tonları çimen paletinden çıkamasın diye).
- Tek çağrı, 7 numaralı açıklama, 16 karo döndü. Maliyet: **10 generation** (cost_est 16, cost_max 22 içinde). Hız sınırı hatası yok; ilk çağrı yalnızca istemci şema hatası verdi (style_images biçimi), üretim başlamadı, harcama yok.
- `get_balance`: çağrıdan önce 905,70; paralel şeritler aynı hesabı kullandığı için sonrası yalnızca bu işi yansıtmaz (~895,70).

## Seçim
| Dosya | Kaynak karo | Not |
|---|---|---|
| iso_road_path_a | tile_1 | kuru toprak patika, silik izler, çim püskülleri |
| iso_road_path_b | tile_0 | patika, kenarda çim şeritleri |
| iso_road_gravel_a | tile_4 | sıkışmış açık çakıl |
| iso_road_paved_worn | tile_10 | açık renk arnavut, aralarda çamur (yıpranmış) |
| iso_road_paved_broken | tile_15 | gri kaldırım, eksik taşlar, çamur |

**Teslim edilmeyenler:** `iso_road_gravel_worn` (tile_6: maske içinde 61 boş piksel, validate BAŞARISIZ), `iso_road_paved_a` (tile_8/9: maske içinde 4 boş piksel, validate BAŞARISIZ) ve alternatif kaldırım karosu tile_12 (43 boş piksel). Tek deneme kuralı gereği yeniden üretilmedi, boşluk doldurulmadı. Öneri: bu iki dosya için küçük bir devam işi (gravel_worn: tile_7 de 75 boş; paved_a: tile_8'in 4 pikseli).
Not: kaldırım için gri tile_8/9 "sağlam" karoya en uygun olandı; sağlam kaldırım şu an yok, `paved_worn` açık tonlu.

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/iso_road_path_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_road_path_b.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_road_gravel_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_road_paved_worn.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_road_paved_broken.png | 64x32 | 32 | 16 |

## Doğrulama
Ham karolarda maske dışı opak piksel vardı (2-157); yalnızca maskeyle kırpıldı (maske dışı silindi, içerik yeniden boyanmadı).

| Dosya | validate_iso (ground) | Renk | Stil denetimi | Doygunluk / aile |
|---|---|---|---|---|
| path_a | geçti | 19 | GEÇTİ | x1.21 |
| path_b | geçti | 22 | GEÇTİ | x1.11 |
| gravel_a | geçti | 10 | GEÇTİ | x1.02 |
| paved_worn | geçti | 28 | GEÇTİ | x1.11 |
| paved_broken | geçti | 28 | UYARI (palete uzak) | x0.69 |

Döşeme testi (8x8 elmas ızgara, (32,16) adımı, karışık): dikiş parlaklık oranı (kenar bandı / iç) path 0.98 ve 1.00, gravel 0.99, paved 1.12 ve 0.86 (paved_broken koyu kenarlı; ızgara çizgisi görünebilir). Çim kenarı ile uyum (3x3 yol + çim) ayrıca görsel kontrol bekliyor: path_b kenarında çim şeritleri var, çim karosuyla yan yana test edilmedi (kullanıcı gözüyle). Gölge pişirilmemiş, ışık sol-üstten (görsel kontrol). Masaüstü kopyası: yerel.
