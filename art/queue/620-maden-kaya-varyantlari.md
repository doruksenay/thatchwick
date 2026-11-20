---
id: "620"
title: "Maden kaya ve damar varyantları (18 karo)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("dediklerini uygula tamamen"; belge 64a §4 Aşama A madde 6)
family: zemin
style_gate: false
cost_est: 18
cost_max: 28
depends_on: ["360","361"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_rock_soil_b.png
  - Assets/Art/Iso/_aday/iso_rock_soil_c.png
  - Assets/Art/Iso/_aday/iso_rock_soft_b.png
  - Assets/Art/Iso/_aday/iso_rock_soft_c.png
  - Assets/Art/Iso/_aday/iso_rock_stone_b.png
  - Assets/Art/Iso/_aday/iso_rock_stone_c.png
  - Assets/Art/Iso/_aday/iso_rock_hard_b.png
  - Assets/Art/Iso/_aday/iso_rock_hard_c.png
  - Assets/Art/Iso/_aday/iso_rock_bedrock_b.png
  - Assets/Art/Iso/_aday/iso_rock_bedrock_c.png
  - Assets/Art/Iso/_aday/iso_rock_vein_iron_ore_b.png
  - Assets/Art/Iso/_aday/iso_rock_vein_coal_b.png
  - Assets/Art/Iso/_aday/iso_rock_vein_limestone_b.png
  - Assets/Art/Iso/_aday/iso_rock_vein_flint_b.png
  - Assets/Art/Iso/_aday/iso_rock_vein_clay_b.png
  - Assets/Art/Iso/_aday/iso_rock_vein_ochre_b.png
  - Assets/Art/Iso/_aday/iso_rock_vein_gypsum_b.png
  - Assets/Art/Iso/_aday/iso_rock_vein_slate_stone_b.png
created: 2026-11-20
---
# 620 Maden kaya ve damar varyantları

## Amaç
Belge 64 §6 madde 9: kaya türü başına tek görsel var (360, 361); kat geniş alanlarda tekrar ediyor. Üreteç v2 bantlı
kaya verdiği için aynı türden geniş alanlar oluşuyor; varyantlar konuma göre sabit seçilecek.

## Tarif
360 ve 361'in yöntemi ve referansları aynen (alçak kesik blok, üst yüz dokulu). Her kaya türüne iki yeni çeşit (b, c):
aynı renk ve sertlik okunuşu, farklı çatlak ve taş deseni. Her damara bir çeşit (b): damar şeridinin konturu farklı.

## Doğrulama
`validate_iso.py`; 6x6 karışık döşeme testi (a/b/c dağınık): ızgara ve tekrar seçilmemeli.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
