unity-import: pending

# 359b Mevsim zemini yeniden

- Araç: `create_tiles_pro`, stil modu, tek çağrı, 2 numaralı istek (karlı toprak a/b), `style_images` = `iso_ground_dirt_a` + `iso_ground_grass_snow_a` (inline), `style_options` renk paleti kapalı, çizgi/detay/gölge açık, `outline_mode: segmentation`, 64x32, açı 30, derinlik 0, `tile_flat_top_px=2`. Tarif: düzensiz yumuşak kenarlı kar yamaları, şerit yok.
- Maliyet: 10 generation (tahmin 15, tavan 20). `get_balance` sonrası 2451,9 (paralel şeritler de harcıyor).
- **Çimen karoları (sonbahar a/b, karlı çimen a/b) yeniden üretilmedi**: kullanıcı 359'daki hâllerini iyi bulduğu için 359 dosyaları birebir `_v2` adıyla kopyalandı (0 gen).
- Karlı toprak seçimi: a = t0 (seyrek), b = t11 (yoğun, yaklaşık yarı kaplı); 16 varyasyondan. Ham: `art/tmp/359b/`, commit edilmez.
- Düzeltmeler: maske dışı piksel silindi (b: 3 px, maskeyle kırpma); ham 18-22 renk, median-cut ile 6'ya indirildi (dithering yok, 5 renk çıktı); karlı toprak a'daki gri kar tonu (#b4b5ae) ve b'deki #c9d3d2, karlı çimenin kar rengine (#E2EDF2 / #C3D8DD) eşlendi, böylece iki karlı zeminde kar rengi aynı.
- Yan yana sayfası (359'in reddedilen hâliyle değil, referans zeminlerle + yeni dosyalar): [359b-yanyana.png](359b-yanyana.png). Reddedilen karlı toprak için bkz. [359-yanyana.png](359-yanyana.png).

## Dosyalar (hepsi 64x32, pivot x=32, y=16 ALTTAN 16; `Assets/Art/Iso/_aday/`)
| Dosya | validate_iso (ground, max 6) | Stil denetimi |
|---|---|---|
| iso_ground_grass_autumn_a_v2.png | geçti | UYARI (palet dışı, x1.19) |
| iso_ground_grass_autumn_b_v2.png | geçti | UYARI (x1.16) |
| iso_ground_grass_snow_a_v2.png | geçti | UYARI (x0.98) |
| iso_ground_grass_snow_b_v2.png | geçti | UYARI (x1.00) |
| iso_ground_dirt_snow_a_v2.png | geçti | GEÇTİ (x1.11) |
| iso_ground_dirt_snow_b_v2.png | geçti | UYARI (x0.99) |

## Döşeme testi (kenar/iç parlaklık oranı, 8x8 çift karma; `art/tmp/359b/`)
karlı toprak a 0.846, karlı çimen a 0.977, sonbahar a 0.808.

## Sorunlar / notlar
- **Sonbahar çimeni karolarının alt kenarında koyu (siyah) 2 px kontur var** (359'dan miras, ~62 px; işin "koyu kontur olmamalı" şartına aykırı). Kullanıcı hâlini beğendiği için dokunulmadı; kabul öncesi bakılmalı, istenirse ayrı iş (yeniden üretim) açılır.
- Karlı toprak a çok seyrek karlı (birkaç küçük yama); b belirgin yamalı.
- Kilit kurulmamış aile: palet uyumu yalnızca uyarı. Masaüstü kopyası: yerel.
