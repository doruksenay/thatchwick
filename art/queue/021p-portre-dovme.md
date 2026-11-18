---
id: "021p"
title: Portre dövme: face
status: review
claimed_at: 2026-11-18T08:33Z
approved_by: user
family: karakter
style_gate: false
cost_est: 35
cost_max: 40
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_celtic_knot_face_64.png
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_woad_spiral_face_64.png
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_rune_face_64.png
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_cross_face_64.png
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_bird_face_64.png
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_snake_face_64.png
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_wheat_ear_face_64.png
  - Assets/Art/Iso/_aday/portrait_layer_tattoo_heraldic_crest_face_64.png
created: 2026-11-18
---
# 021p Portre dövme: face

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Maske: yanak ve şakak bölgesi. Açıklama: `small <desen> tattoo, dark woad blue-grey ink, simple`. Desenler: celtic_knot, woad_spiral, rune, cross, bird, snake, wheat_ear, heraldic_crest. Kol dövmeleri bu listede yok (dünya sprite'ında betikle).


Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
