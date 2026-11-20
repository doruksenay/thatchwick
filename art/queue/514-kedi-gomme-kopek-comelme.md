---
id: "514"
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
  - Assets/Art/Iso/iso_animal_cat_bury_se.png
  - Assets/Art/Iso/iso_animal_dog_squat_se.png
created: 2026-11-20
---
# 514 Kedi gömme ve köpek çömelme kareleri

## Amaç
Belge 57d §5 (kullanıcı kararı: "kedi gömsün, etkisi olmasın ama animasyonu olmalı"). Şu an kod toprak parçacığı çıkarıyor (`BarnService.CatBuried`); bu kareler gelince gerçek animasyon bağlanır.

## Tarif
Hayvan ailesi stil eşlemesi; var olan kedi ve köpek sprite'ları referans. Kedi: 4 karelik şerit, arka ayaklarıyla toprak eşeler. Köpek: 2 karelik şerit, çömelme.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family hayvan <dosya>`.

## Notlar
Şerit düzeni var olan hayvan animasyon şeritleriyle aynı olmalı (`AnimalViews`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
