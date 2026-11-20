---
id: "516"
title: Avlu ruh hali ikonları (9 adet)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: arayuz
style_gate: false
cost_est: 30
cost_max: 45
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Ui/mood_stomach_ache.png
  - Assets/Art/Ui/mood_scolded.png
  - Assets/Art/Ui/mood_saw_too_much.png
  - Assets/Art/Ui/mood_stepped_in_it.png
  - Assets/Art/Ui/mood_bad_smell.png
  - Assets/Art/Ui/mood_lost_appetite.png
  - Assets/Art/Ui/mood_murky_water.png
  - Assets/Art/Ui/mood_tidy_yard.png
  - Assets/Art/Ui/mood_disgusted.png
created: 2026-11-20
---
# 516 Avlu ruh hali ikonları (9 adet)

## Amaç
items.json `effects`: eff_stomach_ache, eff_scolded, eff_saw_too_much, eff_stepped_in_it, eff_bad_smell, eff_lost_appetite, eff_murky_water, eff_tidy_yard, eff_disgusted.

## Tarif
Arayüz ailesi stil eşlemesi; 55b'deki moodlet ikonlarıyla aynı boyut ve çerçeve. Karın ağrısı: eli karnında figür. Azar işitti: işaret parmağı. Gördüğüne pişman: eliyle kapatılmış göz. Kahretsin, bastım!: ayakkabı ve leke. Kötü koku: dalgalı yeşil çizgiler. İştahı kaçtı: tabak ve sinek. Bulanık su: kova, bulanık su. Tertemiz avlu: süpürge ve ışıltı. Midesi bulandı: yeşile dönmüş yüz.

## Doğrulama
`python tools/art/style.py check --family arayuz <dosya>`.

## Notlar
Çıktı yolu 55b ikonlarının bulunduğu klasörle aynı olmalı; farklıysa işçi oradaki yolu kullanır ve raporuna yazar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
