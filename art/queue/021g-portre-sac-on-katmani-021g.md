---
id: "021g"
title: Portre saç ön katmanı: long_curly ...
status: pending
approved_by: user
family: karakter
style_gate: false
cost_est: 45
cost_max: 50
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_hair_long_curly_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_braid_single_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_braids_two_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_bun_high_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_bun_low_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_ponytail_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_topknot_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_receding_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_bald_fringe_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_coif_hair_64.png
created: 2026-11-18
---
# 021g Portre saç ön katmanı: long_curly ...

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Maske: saç (başın üst %45'i, kulak hizasına kadar yanlar; uzun, örgü ve topuz biçimleri için omuza kadar)
Açıklama: `<ad> hairstyle, neutral grey hair, front layer` (<ad> her parçanın adıdır).


Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
