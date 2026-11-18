---
id: "021k"
title: Portre sakal ve takı
status: review
claimed_at: 2026-11-18T08:26Z
approved_by: user
family: karakter
style_gate: false
cost_est: 55
cost_max: 60
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_beard_stubble_64.png
  - Assets/Art/Iso/_aday/portrait_layer_beard_short_64.png
  - Assets/Art/Iso/_aday/portrait_layer_beard_full_64.png
  - Assets/Art/Iso/_aday/portrait_layer_beard_long_64.png
  - Assets/Art/Iso/_aday/portrait_layer_beard_forked_64.png
  - Assets/Art/Iso/_aday/portrait_layer_beard_goatee_64.png
  - Assets/Art/Iso/_aday/portrait_layer_earring_1_64.png
  - Assets/Art/Iso/_aday/portrait_layer_earring_2_64.png
  - Assets/Art/Iso/_aday/portrait_layer_earring_3_64.png
  - Assets/Art/Iso/_aday/portrait_layer_earring_4_64.png
  - Assets/Art/Iso/_aday/portrait_layer_nosering_1_64.png
  - Assets/Art/Iso/_aday/portrait_layer_necklace_cord_64.png
created: 2026-11-18
---
# 021k Portre sakal ve takı

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Maske: sakal maskesi: burnun altından çene ucuna ve yanaklara
Açıklama: `<ad> beard, neutral grey` (<ad> her parçanın adıdır).
Takı: küpe 4 (kulak maskesi, açıklama `small earring variant <ad>`), burun halkası 1 (burun kanadı), ip kolye 1 (boyun/yaka). Takılar gri değil, kendi renginde (metal/ip) çizilir. 12 parça x 5 = 60.

Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
