---
id: "742"
title: "His ikonları (12 adet)"
status: review
claimed_at: 2026-11-21T01:48Z
approved_by: user        # kullanıcı 20.11.2026 ("Hisler ... olur", "şimdi alınabilir"); Project belgeleri 67a §4, 67b
priority: 50
family: arayuz
style_gate: false
cost_est: 36
cost_max: 50
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Ui/feel_grateful.png
  - Assets/Art/Ui/feel_trusts.png
  - Assets/Art/Ui/feel_admires.png
  - Assets/Art/Ui/feel_close.png
  - Assets/Art/Ui/feel_misses.png
  - Assets/Art/Ui/feel_hurt.png
  - Assets/Art/Ui/feel_heartbroken.png
  - Assets/Art/Ui/feel_jealous.png
  - Assets/Art/Ui/feel_betrayed.png
  - Assets/Art/Ui/feel_grudge.png
  - Assets/Art/Ui/feel_ashamed.png
  - Assets/Art/Ui/feel_shared_grief.png
created: 2026-11-20
---
# 742 His ikonları (12 adet)

## Amaç
"Hisler": bir karakterin başka biri hakkındaki uzun ya da kısa süreli hissi (Project belgesi 67a §4). İlişki panelinde ve iki kişi yan yana gelince başın üstünde küçük ikon.

## Tarif
Arayüz ailesi stil eşlemesi; 55b ve 706'daki moodlet ikonlarıyla aynı boyut ve çerçeve. Olumlu hisler sıcak (bal, yeşil), olumsuzlar soğuk (mor, gri mavi) zemin; nakış marka dili (17).
- Minnettar: iki el arasında küçük kalp.
- Güveniyor: kenetlenmiş iki el.
- Hayran: parlayan yıldız ve göz.
- Yakın hissediyor: yan yana iki küçük figür, aralarında ilmek.
- Özlüyor: pencere ve uzakta küçük figür.
- Kırgın: çatlamış küçük kalp yerine buruşmuş yaprak.
- Kalbi kırık: ikiye ayrılmış kalp, dikiş izi.
- Kıskanıyor: yeşil göz.
- İhanete uğramış: arkadan kırık bir mendil (gönül yolu, 67b).
- Kin tutuyor: sıkılmış yumruk ve kara bulut.
- Utanıyor: kızarmış yanaklı, eliyle yüzünü kapatan yüz.
- Yası ortak: tek mum ve iki el.

## Doğrulama
`python tools/art/style.py check --family arayuz <dosya>`.

## Notlar
Çıktı yolu 55b ikonlarının klasörüyle aynı olmalı; farklıysa işçi oradaki yolu kullanır ve raporuna yazar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
