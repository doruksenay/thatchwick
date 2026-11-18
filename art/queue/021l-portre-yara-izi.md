---
id: "021l"
title: Portre yara izi: yarık
status: pending
approved_by: user
family: karakter
style_gate: false
cost_est: 35
cost_max: 40
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_forehead_64.png
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_left_brow_64.png
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_right_cheek_64.png
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_left_cheek_64.png
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_nose_bridge_64.png
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_chin_64.png
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_jaw_left_64.png
  - Assets/Art/Iso/_aday/portrait_layer_scar_slash_neck_64.png
created: 2026-11-18
---
# 021l Portre yara izi: yarık

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Desen: slash (yarık=derin kesik, çizik=ince paralel çizgiler, yanık=kabarık düzensiz leke, ısırık=yarım daire diş izi). 8 bölge: forehead, left_brow, right_cheek, left_cheek, nose_bridge, chin, jaw_left, neck; her bölge için küçük elle maske (yalnız o bölge). Açık pembe-gri, ten üstüne bindirilir. Açıklama: `slash scar on the <bölge>, neutral light`.


Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
