---
id: "751"
title: "Gönül yolu ve aile ikonları (10 adet)"
status: pending
approved_by: user        # kullanıcı 20.11.2026; Project belgeleri 67b, 67c
priority: 50
family: arayuz
style_gate: false
cost_est: 30
cost_max: 45
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Ui/status_sweetheart.png
  - Assets/Art/Ui/status_engaged.png
  - Assets/Art/Ui/event_asking.png
  - Assets/Art/Ui/event_wedding.png
  - Assets/Art/Ui/event_henna_night.png
  - Assets/Art/Ui/mood_salty_coffee.png
  - Assets/Art/Ui/event_invitation.png
  - Assets/Art/Ui/status_ex_spouse.png
  - Assets/Art/Ui/mood_forgave.png
  - Assets/Art/Ui/mood_regret.png
created: 2026-11-20
---
# 751 Gönül yolu ve aile ikonları (10 adet)

## Amaç
İlişki durumu, olay bildirimi ve iç ses ikonları (Project belgeleri 67b §3 ve §6).

## Tarif
Arayüz ailesi stil eşlemesi; 55b, 706, 742 ile aynı boyut ve çerçeve; nakış marka dili (17).
- Sevgili: işlemeli mendil ve küçük kalp.
- Nişanlı: kurdeleyle bağlı iki yüzük.
- İsteme: kapı ve çiçek demeti.
- Düğün: davul ve zurna çaprazlama.
- Kına gecesi: avuç içinde kına ve mum.
- Tuzlu kahve (iç ses): fincan ve tuz taneleri, küçük buruşuk yüz.
- Davet: mühürlü küçük mektup ve çiçek.
- Eski eş: ikiye ayrılmış yüzük.
- Affetti (iç ses): uzanan el ve beyaz çiçek.
- Pişman (iç ses): başı öne eğik figür, küçük yağmur bulutu.

## Doğrulama
`python tools/art/style.py check --family arayuz <dosya>`.

## Notlar
Çıktı yolu 55b ikonlarının klasörüyle aynı olmalı; farklıysa işçi oradaki yolu kullanır ve raporuna yazar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
