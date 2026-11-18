---
id: "395c"
title: Dördüncü tur: ahşap çatkı rötuşu, kandil ve kömür yığını (referans görselle)
status: running
claimed_at: 2026-11-18T19:40Z
approved_by: user        # kullanıcı 18.11.2026: "bir tur daha deneyelim"
priority: 20
family: esya
style_gate: false
cost_est: 15
cost_max: 20
depends_on: ["364"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_mine_support_wood_d_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_lamp_d_se.png
  - Assets/Art/Iso/_aday/iso_item_charcoal_clamp_d_2x2_se.png
  - art/results/395c-kazi-isleme-dorduncu-tur.md
created: 2026-11-18
---
# 395c Dördüncü tur

## Amaç
395b'nin `## Sorular` bölümündeki öneriler (kullanıcı onayı 18.11.2026). Var olan PNG'lerin üzerine yazılmaz (`_d`).

## Tarif
Yöntem ve doğrulama ana işleriyle aynı (301; `validate_iso.py`, `style.py check --family esya`; kandilde `--light`).
1. **Ahşap çatkı:** 395b'nin `_c` çıktısı (`art/results/395b-red/iso_item_mine_support_wood_c_se.png`, kiriş düzelmiş hâli) üstünde ikinci `inpaint`: maske çapraz kafes ve iki dikme; metin `two plain straight vertical posts of fresh light brown pine wood, no diagonal braces, empty space between the posts`. Sonuç iki dikme + bir kiriş değilse teslim edilmez.
2. **Kandil:** `create_map_object`, `background_image` = kabul edilen mum (`_aday/iso_item_mine_candle_se.png`), mumun durduğu bölge maskeli; metin `small plain grey-brown clay oil lamp with a tiny flame, same lighting and palette as the background`. Tuval 48x48. Sonra `reduce_colors` 32, `fit --family esya`.
3. **Kömür yığını:** `create_map_object`, `background_image` = 364'te kabul edilen kireç ocağı (`_aday/iso_item_lime_kiln_2x2_se.png`; kavurma yığını teslim edilmemişti, aynı dış mekân ve toprak paleti), yığın bölgesi maskeli; metin 396 ile aynı (`low dome mound covered with dark earth and grass turf, a few log ends visible at the base, thin grey smoke`). Sonra `reduce_colors` 40, `fit --family esya`.
Araç şeması tarifteki parametreyi desteklemiyorsa çağırma, `## Sorular` ekle (395b'deki `color_image` hatası tekrarlanmasın).

## Doğrulama
Ana işlerle aynı. Yan yana sayfası: üç yeni görsel ve yanında referansları (395b çatkı, mum, kireç ocağı).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
