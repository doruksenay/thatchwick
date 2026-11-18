---
id: "395b"
title: Üçüncü tur: ahşap çatkı, kandil, kömür yığını
status: running
claimed_at: 2026-11-18T18:41Z
approved_by: user        # kullanıcı 18.11.2026: "bunu da yap" (iki denemede teslim edilmeyen üç parça)
priority: 10
family: esya
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["362", "363", "364", "395", "396"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_mine_support_wood_c_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_lamp_c_se.png
  - Assets/Art/Iso/_aday/iso_item_charcoal_clamp_c_2x2_se.png
created: 2026-11-18
---
# 395b Üçüncü tur: ahşap çatkı, kandil, kömür yığını

## Amaç
362/395 (ahşap çatkı), 363/395 (kandil) ve 364/396'da (kömür yığını) iki denemede teslim edilemeyen parçalar. Metinle yeniden üretmek iki kez tutmadı; bu tur **kabul edilmiş görsellerden türetir**. Var olan PNG'lerin üzerine yazılmaz (`_c` adı).

## Tarif
Yöntem ve doğrulama ana işleriyle aynı (301 yöntemi, `validate_iso.py`, `style.py check`; ışıklıda `--light`).

1. **Ahşap çatkı:** şekil 362'de kabul edilen çürüyen çatkıdan (`_aday/iso_item_mine_support_rotten_se.png`), çünkü iki dikme + kiriş orada doğru okunuyor.
   - Önce ücretsiz yol: `python tools/art/style.py fit --family esya` ile çürüyen çatkıyı aileye çek, ardından sarkan kirişi düz göstermek için PixelLab `inpaint` (yalnızca kiriş bölgesi maskeli, metin `straight sound pine beam, fresh light brown wood`).
   - Sonuç tam iki dikme + bir kiriş değilse teslim edilmez.
2. **Kandil:** 363'te kabul edilen mum (`_aday/iso_item_mine_candle_se.png`) renk ve ölçek referansı. `create_map_object`, `color_image` = mum görseli (palet zorlaması), metin `small plain grey-brown clay oil lamp on a stone, tiny flame`, tuval 48x48. Sonra `reduce_colors` 32 ve `fit --family esya`; `check --light`.
3. **Kömür yığını:** 364'te kabul edilen kavurma yığını (`_aday/iso_item_roasting_heap_2x2_se.png`) `color_image` olarak palet referansı. Metin 396 ile aynı (`low dome mound covered with dark earth and grass turf, a few log ends visible at the base, thin grey smoke`). Sonra `reduce_colors` 40 ve `fit --family esya`. 396-red'deki ham aday da `fit` + `reduce_colors 32` ile denenebilir (0 gen); hangisi denetimden geçerse o teslim edilir.

## Doğrulama
Ana işlerle aynı. Yan yana sayfası: üç yeni görsel, yanında ilgili kabul edilmiş referans (çürüyen çatkı, mum, kavurma yığını).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
