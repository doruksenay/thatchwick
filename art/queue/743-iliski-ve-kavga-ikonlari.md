---
id: "743"
title: "İlişki, kavga ve muhabbet ikonları (10 adet)"
status: pending
approved_by: user        # kullanıcı 20.11.2026 ("şimdi alınabilir"); Project belgeleri 67a, 67b
priority: 50
family: arayuz
style_gate: false
cost_est: 30
cost_max: 45
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Ui/social_fight.png
  - Assets/Art/Ui/social_argue.png
  - Assets/Art/Ui/social_sulk.png
  - Assets/Art/Ui/social_make_up.png
  - Assets/Art/Ui/social_group_chat.png
  - Assets/Art/Ui/social_gossip.png
  - Assets/Art/Ui/social_jab.png
  - Assets/Art/Ui/mood_bruised.png
  - Assets/Art/Ui/mood_beard_rash.png
  - Assets/Art/Ui/mood_scared_fight.png
created: 2026-11-20
---
# 743 İlişki, kavga ve muhabbet ikonları (10 adet)

## Amaç
Sosyal menü ve iç ses (moodlet) ikonları (Project belgeleri 67a §5-6, 67b §5).

## Tarif
Arayüz ailesi stil eşlemesi; 55b ve 706 ile aynı boyut ve çerçeve.
- Kavga et: toz bulutu ve fırlayan küçük yıldız.
- Tartış: iki karşılıklı konuşma balonu, içlerinde zikzak.
- Küs: sırt sırta iki figür.
- Barış: el sıkışan iki el.
- Muhabbet: halka olmuş üç küçük figür.
- Dedikodu: kulağa fısıldayan ağız, üç nokta.
- Laf sok: diken çıkan konuşma balonu.
- Morarmış (iç ses): mor bir göz morluğu, çizgi film.
- Sakal battı (iç ses): sakal ve kızarmış yanak, minik çizgiler.
- Kavgadan korktu (çocuk iç sesi): büyük gözlü, ellerini kulağına kapatmış çocuk.

## Doğrulama
`python tools/art/style.py check --family arayuz <dosya>`.

## Notlar
Çıktı yolu 55b ikonlarının klasörüyle aynı olmalı; farklıysa işçi oradaki yolu kullanır ve raporuna yazar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
