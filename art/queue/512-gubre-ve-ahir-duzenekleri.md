---
id: "512"
title: Gübre ve ahır düzenekleri (gübre yığını, gübrelik, şerbet fıçısı, saman kıyıcı, gübre kapağı, akıtma oluğu, ağıl çiti, kırlangıç yuvası)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: esya
style_gate: false
cost_est: 55
cost_max: 60
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_dung_heap_se.png
  - Assets/Art/Iso/iso_item_dung_stead_2x2_se.png
  - Assets/Art/Iso/iso_item_slurry_barrel_se.png
  - Assets/Art/Iso/iso_item_chaff_cutter_se.png
  - Assets/Art/Iso/iso_item_manure_hatch_se.png
  - Assets/Art/Iso/iso_item_barn_drain_se.png
  - Assets/Art/Iso/iso_item_fold_hurdle_se.png
  - Assets/Art/Iso/iso_item_swallow_nest_se.png
created: 2026-11-20
---
# 512 Gübre ve ahır düzenekleri (gübre yığını, gübrelik, şerbet fıçısı, saman kıyıcı, gübre kapağı, akıtma oluğu, ağıl çiti, kırlangıç yuvası)

## Amaç
Belge 57 §4, 57d §4: ahır temizliğinin ve gübre zincirinin eşyaları.

## Tarif
Eşya ailesi stil eşlemesi, izometrik 2:1. Gübre yığını: saman karışık koyu yığın, tek karo. Gübrelik: taş çevrili 2x2 çukur, dolu. Şerbet fıçısı: dik fıçı, ağzında bez torba asılı, yanında tahta musluk. Saman kıyıcı: bıçaklı ahşap tezgâh. Gübre kapağı: yerde menteşeli ahşap kapak. Akıtma oluğu: taş oluk parçası. Ağıl çiti: taşınır söğüt örgü panel. Kırlangıç yuvası: saçak altı çamur yuva (duvar eşyası gibi küçük).

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`.

## Notlar
Gübrelik 2x2 için items.json'da `art` henüz yok; pivot ve boyutu işçi raporlar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
