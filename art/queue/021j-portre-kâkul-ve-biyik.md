---
id: "021j"
title: Portre kâkül ve bıyık
status: pending
approved_by: user
family: karakter
style_gate: false
cost_est: 50
cost_max: 55
depends_on: ["020", "021a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_layer_bangs_straight_64.png
  - Assets/Art/Iso/_aday/portrait_layer_bangs_side_swept_64.png
  - Assets/Art/Iso/_aday/portrait_layer_bangs_curtain_64.png
  - Assets/Art/Iso/_aday/portrait_layer_bangs_wispy_64.png
  - Assets/Art/Iso/_aday/portrait_layer_bangs_choppy_64.png
  - Assets/Art/Iso/_aday/portrait_layer_bangs_long_covering_64.png
  - Assets/Art/Iso/_aday/portrait_layer_moustache_thin_64.png
  - Assets/Art/Iso/_aday/portrait_layer_moustache_handlebar_64.png
  - Assets/Art/Iso/_aday/portrait_layer_moustache_walrus_64.png
  - Assets/Art/Iso/_aday/portrait_layer_moustache_pencil_64.png
  - Assets/Art/Iso/_aday/portrait_layer_moustache_droopy_64.png
created: 2026-11-18
---
# 021j Portre kâkül ve bıyık

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
Ortak ayarlar (020 raporu): `inpaint_image_pro_flash`, taban `source_image_id` ile (base64 bozuldu), `output_method="Modify current layer, only changes"`, 64x64, 5 gen/çağrı, maskeler betikle (saf siyah/beyaz, geçici klasör). Gri tonlu çiz, açıklamaya `neutral grey, will be recoloured` ekle. Açıklama eki: `, pixel art portrait layer, front view, flat simple shading, clean dark outline, light from top-left, no dithering, no background`. Taban: 021a tabanlarından oval yetişkin (`portrait_base_oval_adult_64`).

Maske: alın bandı (başın üst %30'u, yüz bölgesinin üst kenarı)
Açıklama: `<ad> fringe bangs, neutral grey` (<ad> her parçanın adıdır).
Bıyık: maske burnun altı ve ağzın üstü; açıklama `<ad> moustache, neutral grey`. 11 parça x 5 = 55.

Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
