---
id: "021h"
title: Portre saç arka katmanı: short_crop ...
status: pending
approved_by: user
family: karakter
style_gate: false
cost_est: 45
cost_max: 50
depends_on: ["020", "021f"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_hair_short_crop_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_short_messy_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_short_side_part_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_bowl_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_buzz_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_medium_straight_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_medium_wavy_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_shoulder_curly_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_long_straight_back_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_long_wavy_back_64.png
created: 2026-11-18
---
# 021h Portre saç arka katmanı: short_crop ...

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Maske: başın arkası ve omuz üstü (yüz bölgesi dışı; taban altında kalacak). Açıklama: `<ad> hairstyle seen from behind the head, neutral grey, back layer only`.


Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
