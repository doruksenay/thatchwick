---
id: "021b"
title: Portre yüz tabanları: long, heart
status: running
claimed_at: 2026-11-18T08:11Z
approved_by: user
family: karakter
style_gate: false
cost_est: 19
cost_max: 24
depends_on: ["020"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_base_long_child_64.png
  - Assets/Art/Iso/_aday/portrait_base_long_youth_64.png
  - Assets/Art/Iso/_aday/portrait_base_long_adult_64.png
  - Assets/Art/Iso/_aday/portrait_base_long_elder_64.png
  - Assets/Art/Iso/_aday/portrait_base_heart_child_64.png
  - Assets/Art/Iso/_aday/portrait_base_heart_youth_64.png
  - Assets/Art/Iso/_aday/portrait_base_heart_adult_64.png
  - Assets/Art/Iso/_aday/portrait_base_heart_elder_64.png
created: 2026-11-18
---
# 021b Portre yüz tabanları: long, heart

## Amaç
Karakter yaratma portre parçası (proje dokümanı 14, bölüm 2, 3D-3F ve 6). 021 planından; 020 yöntemi (katman, "only changes" + maske) tuttu.

## Tarif
`create_image_pixflux`, 1 gen/çağrı, ayarlar 020 A ile aynı (64x64, no_background, south, medium detail, basic shading, palet = `iso_char_farmer_a2_56_cand_se.png`). Açıklama: `<yüz şekli> face shape, <yaş>, completely bald head, no hair, clean shaven NO beard NO moustache, neutral expression, plain undyed linen collar, neutral grey skin tone will be recoloured` + 020 ortak eki. **Taban sakalsız olmalı** (020 sorunu 1: iki denemede sakallı çıktı): sakal/bıyık görünürse tek yeniden deneme (+1 gen), yine olmazsa raporla. Gri tonlu ten, 020 sorunu 2'deki doygunluk sapmasını da azaltır.
Yaş: child (about 8), youth (about 18), adult (about 35), elder (about 70, wrinkles, thin). En çok 2 deneme/taban.

Her parça en çok 1 çağrı (tek deneme). Çıktılar `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Her dosya 64x64, yarı saydam piksel 0, renk sayısı ≤ 32 (değilse `reduce_colors` 32).
- Maske dışı piksel 0.
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Tabana birleşim önizlemesi (x4): boşluk, kayma ya da çift dış hat yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
