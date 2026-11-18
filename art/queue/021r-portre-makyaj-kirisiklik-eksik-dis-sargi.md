---
id: "021r"
title: Portre makyaj, kırışıklık, eksik diş, sargı
status: running
claimed_at: 2026-11-18T08:37Z
approved_by: pending     # kullanıcı kararı 2026-11-18: portre işleri sonraki döneme ya da paket yükseltmeye ertelendi; devam için user yap
family: karakter
style_gate: false
cost_est: 35
cost_max: 40
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_makeup_eyeliner_64.png
  - Assets/Art/Iso/_aday/portrait_layer_makeup_blush_64.png
  - Assets/Art/Iso/_aday/portrait_layer_makeup_lip_64.png
  - Assets/Art/Iso/_aday/portrait_layer_makeup_henna_64.png
  - Assets/Art/Iso/_aday/portrait_layer_wrinkles_64.png
  - Assets/Art/Iso/_aday/portrait_layer_missing_tooth_64.png
  - Assets/Art/Iso/_aday/portrait_layer_bandage_64.png
created: 2026-11-18
---
# 021r Portre makyaj, kırışıklık, eksik diş, sargı

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Makyaj gri tonlu, opaklıkla boyanır: sürme (göz çevresi), allık (yanak), dudak boyası (dudak), kına (alın/yanak, süslü). Kırışıklık (yaşlı tabana, `portrait_base_oval_elder_64`), eksik diş (ağız bölgesi), sargı (alın ya da yanak, krem keten). Her biri kendi küçük maskesiyle.


Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
