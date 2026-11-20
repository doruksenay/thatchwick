---
id: "704"
title: Kedi gömme ve köpek çömelme kareleri
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: hayvan
style_gate: false
cost_est: 40
cost_max: 60
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_animal_cat_1_bury_se_00.png
  - Assets/Art/Iso/iso_animal_cat_1_bury_se_01.png
  - Assets/Art/Iso/iso_animal_cat_1_bury_se_02.png
  - Assets/Art/Iso/iso_animal_cat_1_bury_se_03.png
  - Assets/Art/Iso/iso_animal_dog_1_squat_se_00.png
  - Assets/Art/Iso/iso_animal_dog_1_squat_se_01.png
created: 2026-11-20
---
# 704 Kedi gömme ve köpek çömelme kareleri

## Amaç
Belge 57d §5 (kullanıcı kararı: "kedi gömsün, etkisi olmasın ama animasyonu olmalı"). Kod hazır: kedi gömerken ve köpek çömelirken `OddScenes` pozu ("bury", "squat") kurulur, `IsoArt.AnimalPose` bu adlardaki kareleri kendiliğinden çizer; kareler yokken düz duruş ve toprak parçacığı.

## Tarif
Hayvan ailesi stil eşlemesi; var olan kedi ve köpek sprite'ları referans. Kedi: 4 karelik şerit, arka ayaklarıyla toprak eşeler. Köpek: 2 karelik şerit, çömelme.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family hayvan <dosya>`.

## Notlar
Ayrı kare dosyaları (şerit değil): `IsoArt.PoseFrame` `<set>_<poz>_se_NN` arar. Kedi 32x32, pivot (15, 2); köpek 40x40, pivot (19, 4): var olan `iso_animal_cat_1` ve `iso_animal_dog_1` ile aynı tuval ve pivot. Güneybatı yönü aynanın yansıması.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
