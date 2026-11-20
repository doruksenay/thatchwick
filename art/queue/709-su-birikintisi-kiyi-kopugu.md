---
id: "709"
title: Su birikintisi, çamur ve kıyı köpüğü çıkartmaları
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: zemin
style_gate: false
cost_est: 30
cost_max: 45
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_decal_puddle_a.png
  - Assets/Art/Iso/iso_decal_puddle_b.png
  - Assets/Art/Iso/iso_decal_mud_a.png
  - Assets/Art/Iso/iso_decal_shore_foam_a.png
  - Assets/Art/Iso/iso_decal_shore_foam_b.png
created: 2026-11-20
---
# 709 Su birikintisi, çamur ve kıyı köpüğü çıkartmaları

## Amaç
Belge 57 §9-10: yağmurda yerde su birikintisi ve çamur, su kıyısında köpük. Kodda sıçrama, saçak damlası, ıslak zemin tonu ve su parıltısı var; bu çıkartmalar gelince birikinti ve köpük bağlanır.

## Tarif
Zemin ailesi stil eşlemesi; tek karoya sığan, kenarları saydam, zemine yatık çıkartmalar. Birikinti: düzensiz kenarlı, gökyüzünü yansıtan açık mavi gri. Çamur: koyu kahve, ıslak parlaklık. Kıyı köpüğü: su karosunun kıyı kenarına oturan ince beyaz kıvrım, iki değişke.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family zemin <dosya>`.

## Notlar
Çıkartma katmanı kodu bu görsellerle birlikte yazılacak (zemin ile eşya arasında çizilir).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
