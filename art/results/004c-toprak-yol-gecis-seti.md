unity-import: pending

# 004c Toprak yol, çim geçiş seti (stil pilotu, 4 karo)

- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation` (koyu çerçeve yok; 004'ün sorunu), `boundary_amplitude=0.25` (dalgalı sınır), palet = `iso_ground_grass_a` + `iso_ground_dirt_a` renkleri (8 renk). Tarif: sıkışmış toprak yol / çim, sarkan çim uçları, küçük çakıl.
- Çağrı: 1, 10 generation (araç açıklamasıyla aynı, `cost_est` 12 altında). Tile id: 44c79782-3afd-4ad4-b5f3-9b35e2325fad. Çıktı 16 karo; 4 tanesi teslim edildi, kalan 12'si 004d için bu id'den tekrar alınabilir (ham dosyalar teslim edilmedi).
- Harcanan: 10. `get_balance` sonrası: 3768,5 (paralel şeritlerin harcamaları da içinde).
- Yan yana sayfası: art/results/004c-yanyana.png
- Masaüstü kopyası: yerel

## Maske eşlemesi (önemli, 004d için)
PixelLab'in maske etiketi ("set bit = ilk arazi") gerçekte **çimi** işaretliyor: tile_0 (mask 0) tam toprak, tile_15 tam çim çıktı. Köşe adları: NW = üst (kuzey), NE = sağ (doğu), SW = sol (batı), SE = alt (güney); `mask = NW<<3|NE<<2|SW<<1|SE`, set bit = çim. İş dosyasındaki numara (bit 0 = K, 1 = D, 2 = G, 3 = B; 1 = toprak) şuna çevrilir: `tile_N` için `N = çim köşeleri` = `(1-toprak) üzerinden`. Teslim edilenler:
- fill_a = tile_0 (tam toprak)
- wang_03 (K+D toprak; çim G+B) = tile_3
- wang_01 (yalnız K toprak) = tile_7
- wang_07 (K+D+G toprak; çim B... iş dosyası bit düzeniyle: yalnız B çim) = tile_2 (yalnız sol köşe çim, ekranda W)

Not: wang_07'de çim köşe ekranda sol (batı), iş dosyasındaki bit düzenine göre toprak K+D+G (bit 0,1,2), yani çim = B (bit 3) = sol. Tutarlı.

## Çıktı ve kırpma
Karolar 64x32 geldi, köşelerde maske dışı 64 opak piksel vardı (her karoda); yalnızca maskeyle silindi, içerik yeniden boyanmadı.

## Dosyalar (pivot x, pivot y alttan)
| Dosya | Boyut | Pivot x | Pivot y | validate_iso --kind ground |
|---|---|---|---|---|
| Assets/Art/Iso/iso_path_dirt_fill_a.png | 64x32 | 32 | 16 | geçti |
| Assets/Art/Iso/iso_path_dirt_wang_03.png | 64x32 | 32 | 16 | geçti |
| Assets/Art/Iso/iso_path_dirt_wang_01.png | 64x32 | 32 | 16 | geçti |
| Assets/Art/Iso/iso_path_dirt_wang_07.png | 64x32 | 32 | 16 | geçti |

(Pivot, `validate_iso` çıktısı: elmas merkezi, x=32 y=16.)

## Stil denetimi (zemin ailesi, kilit kurulmadı: palet uyumu yalnızca uyarı; kırpma öncesi dosyalarla)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| fill_a | %100 | 4,9 | x0,94 | GEÇTİ |
| wang_03 | %95 | 6,4 | x0,95 | GEÇTİ |
| wang_01 | %90 | 10,7 | x0,91 | UYARI (palete uzak, teslim edildi) |
| wang_07 | %97 | 4,6 | x0,96 | GEÇTİ |

## Döşeme testi
8x8 elmas ızgara, adım (32,16), yalnızca `fill_a` ve `iso_ground_grass_a` döşendi (PNG art/tmp004c, commit edilmedi). Kenar/iç parlaklık oranı: fill_a 0,94 (iş dosyasındaki ±0,03 hedefinin dışında; kenar %6 koyu), grass_a 1,01 (kıyas). Gölge pişirilmemiş, ışık sol üstten. Karma Wang döşemesi (düz yol, L, 2x2) bu turda üretilmedi; kullanıcı yan yana sayfasına bakıp karar vermeli.

## Sorunlar
- Dolgu tek varyant (a); b/c yok. Seti 004d'de ayrıca üretmek gerekir.
- fill_a kenar/iç oranı 0,94 hedefi karşılamıyor; Wang karolarının toprak kenarı ile `iso_ground_dirt_a` tonu birebir değil (fill_a daha açık). Kullanıcı gözle değerlendirmeli.
- Süsler (3. madde) bu pilotta yok.
- İş dosyasının tarifi "iki ayrı arazi adı" bekliyor; araç açıklamayı virgülden böldü, arazi adları bozuk ama sonuç doğru toprak/çim çıktı.
