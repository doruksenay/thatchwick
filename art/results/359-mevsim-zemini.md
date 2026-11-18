unity-import: pending

# 359 Mevsim zemini

- Araç: `create_tiles_pro`, stil modu, tek çağrı, 6 numaralı istek. `style_images` = `iso_ground_grass_a/b`, `iso_ground_dirt_a` (inline base64). `style_options`: renk paleti kapalı (yoksa referans yeşili kar ve sonbahar rengini ezerdi), çizgi/detay/gölgeleme açık. `outline_mode: segmentation` (zeminde ızgara çizgisi olmasın), açı 30, derinlik 0, `tile_flat_top_px=2`.
- Maliyet: aracın bildirdiği 10 generation (tahmin 15, tavan 20). `get_balance` sonrası 4668,9; ancak aynı anda başka şeritler de harcadığı için fark (25) bu işe ait değil.
- Çağrı 16 varyasyon döndürdü; numaralı istek sırasına göre seçilenler: sonbahar a=t0, b=t1; karlı çimen a=t4, b=t6; karlı toprak a=t8, b=t9 (ham: `art/tmp/359/`, commit edilmez).
- Düzeltmeler (yeniden boyama yok): (1) maske dışındaki 120-124 piksel silindi (maskeyle kırpma); (2) ham renk sayısı 13-23 idi, tarifteki en çok 6 renk sınırı için Pillow median-cut ile 6'ya (karlılarda 5) indirildi, dithering yok. Bu bir renk kuantizasyonudur; ince tonlar kaybolmuş olabilir, yan yana sayfasına bakın. Ham dosyalar teslim edilmedi.
- Kar en açık tonu #E2EDF2 (226,237,242), hedef ~#E6EEF2 ile uyumlu, saf beyaz yok.
- Yan yana sayfası: [359-yanyana.png](359-yanyana.png)

## Dosyalar (hepsi 64x32, pivot x=32, y=16 ALTTAN 16)
| Dosya | Renk | validate_iso (ground, max 6) |
|---|---|---|
| iso_ground_grass_autumn_a.png | 6 | geçti |
| iso_ground_grass_autumn_b.png | 6 | geçti |
| iso_ground_grass_snow_a.png | 5 | geçti |
| iso_ground_grass_snow_b.png | 5 | geçti |
| iso_ground_dirt_snow_a.png | 5 | geçti |
| iso_ground_dirt_snow_b.png | 5 | geçti |

## Stil denetimi (`style.py check --family zemin`, kilit kurulmadı: yalnızca uyarı)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk/aile | Sonuç |
|---|---|---|---|---|
| grass_autumn_a | %67 | 18.8 | x1.19 | UYARI |
| grass_autumn_b | %100 | 14.3 | x1.16 | UYARI |
| grass_snow_a | %88 | 17.0 | x0.98 | UYARI |
| grass_snow_b | %86 | 16.8 | x1.00 | UYARI |
| dirt_snow_a | %49 | 17.0 | x0.83 | UYARI |
| dirt_snow_b | %63 | 15.5 | x0.77 | UYARI |

## Döşeme testi (8x8 karma, çiftler; kenar bandı / iç parlaklık oranı)
- sonbahar a+b: 0.705 (kenar iç kısımdan belirgin koyu; dikiş görünebilir, kullanıcı bakmalı)
- karlı çimen a+b: 0.967
- karlı toprak a+b: 1.109
- Çıktılar `art/tmp/359/fix/tile_*.png` (commit edilmez). Geçiş testi (yaz çimeni/sonbahar ve karlı yarı yarıya): `art/tmp/359/fix/gecis_*.png`, commit edilmez; yan yana sayfasında aile uyumu görülür.

## Sorunlar / notlar
- Karlı toprak ve karlı çimende bazı varyasyonlar (t5, t7, t9-15) kar yamalı; seçilen çiftler ince karlı, fakat karlı çimen "çimen uçları" zayıf (çoğunlukla beyaz).
- Sonbahar çimeni ailenin yaz çimeninden hafif doygun (x1.16-1.19), `fit` uygulanmadı (UYARI, KALDI değil).
- Masaüstü kopyası: yerel.
