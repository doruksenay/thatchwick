---
id: "021c"
title: Portre göz şekilleri
status: accepted
claimed_at: 2026-11-18T08:18Z
approved_by: user
family: karakter
style_gate: false
cost_est: 35
cost_max: 40
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_eyes_round_64.png
  - Assets/Art/Iso/_aday/portrait_layer_eyes_almond_64.png
  - Assets/Art/Iso/_aday/portrait_layer_eyes_narrow_64.png
  - Assets/Art/Iso/_aday/portrait_layer_eyes_wide_64.png
  - Assets/Art/Iso/_aday/portrait_layer_eyes_droopy_64.png
  - Assets/Art/Iso/_aday/portrait_layer_eyes_hooded_64.png
  - Assets/Art/Iso/_aday/portrait_layer_eyes_upturned_64.png
  - Assets/Art/Iso/_aday/portrait_layer_eyes_small_64.png
created: 2026-11-18
---
# 021c Portre göz şekilleri

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Maske: göz bölgesi (kaş altından yanak üstüne). Göz akı ve iris ayrı maske: iris pikselleri sabit `#808080`, ak daha açık gri; iris rampa takasıyla renklenir.
Açıklama: `<ad> shaped eyes, white of the eye light grey, iris separate dark disc` (<ad> her parçanın adıdır).


Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
