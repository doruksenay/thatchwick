---
id: "021d"
title: Portre kaş ve burun
status: running
claimed_at: 2026-11-18T08:19Z
approved_by: user
family: karakter
style_gate: false
cost_est: 55
cost_max: 60
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_brow_thin_arched_64.png
  - Assets/Art/Iso/_aday/portrait_layer_brow_thick_straight_64.png
  - Assets/Art/Iso/_aday/portrait_layer_brow_bushy_64.png
  - Assets/Art/Iso/_aday/portrait_layer_brow_angled_64.png
  - Assets/Art/Iso/_aday/portrait_layer_brow_soft_round_64.png
  - Assets/Art/Iso/_aday/portrait_layer_brow_sparse_64.png
  - Assets/Art/Iso/_aday/portrait_layer_nose_small_64.png
  - Assets/Art/Iso/_aday/portrait_layer_nose_straight_64.png
  - Assets/Art/Iso/_aday/portrait_layer_nose_broad_64.png
  - Assets/Art/Iso/_aday/portrait_layer_nose_hooked_64.png
  - Assets/Art/Iso/_aday/portrait_layer_nose_button_64.png
  - Assets/Art/Iso/_aday/portrait_layer_nose_long_64.png
created: 2026-11-18
---
# 021d Portre kaş ve burun

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Maske: kaş bandı (göz üstü)
Açıklama: `<ad> eyebrows` (<ad> her parçanın adıdır).
Burunlar: maske burun alanı (gözlerin altı, ağzın üstü), açıklama `<ad> nose`. 12 parça x 5 = 60.

Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
