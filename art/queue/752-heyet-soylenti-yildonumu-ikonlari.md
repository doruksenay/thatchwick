---
id: "752"
title: "İhtiyar heyeti, söylenti, bilgi ve yıldönümü ikonları (10 adet)"
status: running
claimed_at: 2026-11-21T01:48Z
approved_by: user        # kullanıcı 21.11.2026 (heyet, lobi, söylenti, yıldönümü, eşler arası tetikler: "hepsi olsun"); Project belgeleri 67c, 67d
priority: 50
family: arayuz
style_gate: false
cost_est: 30
cost_max: 45
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Ui/event_council.png
  - Assets/Art/Ui/status_council_member.png
  - Assets/Art/Ui/event_petition.png
  - Assets/Art/Ui/event_imece.png
  - Assets/Art/Ui/social_rumor.png
  - Assets/Art/Ui/social_deny_rumor.png
  - Assets/Art/Ui/info_known.png
  - Assets/Art/Ui/info_secret.png
  - Assets/Art/Ui/event_anniversary.png
  - Assets/Art/Ui/mood_forgot_anniversary.png
created: 2026-11-21
---
# 752 İhtiyar heyeti, söylenti, bilgi ve yıldönümü ikonları (10 adet)

## Amaç
Project belgeleri 67c ve 67d: ihtiyar heyeti toplantısı ve üyeliği, dilekçe, imece çağrısı; söylenti çıkarma ve yalanlama; bilgi sistemi ("biliyor", "sır"); evlilik yıldönümü ve unutulan yıldönümü iç sesi.

## Tarif
Arayüz ailesi stil eşlemesi; 55b, 706, 742, 751 ile aynı boyut ve çerçeve; nakış marka dili (17).
- Heyet toplantısı: çınar altında halka olmuş üç küçük yaşlı figür.
- Heyet üyesi: küçük ahşap asa ve yaprak çelengi.
- Dilekçe: rulo kâğıt ve mühür.
- İmece: kürek ve kazma çaprazlama, üstünde el ele iki el.
- Söylenti çıkar: kıvrımlı duman gibi konuşma balonu, içinde soru işareti yerine karalama (yazı yok).
- Söylentiyi yalanla: üstü çizili duman balonu.
- Biliyor: açık göz ve küçük kâğıt.
- Sır: dudağa konmuş parmak.
- Evlilik yıldönümü: iki yüzük ve küçük çiçek, takvim yaprağı (yazısız).
- Yıldönümünü unuttu (iç ses): solmuş çiçek ve küçük ünlem yerine düşmüş yaprak.

## Doğrulama
`python tools/art/style.py check --family arayuz <dosya>`.

## Notlar
Çıktı yolu 55b ikonlarının klasörüyle aynı olmalı; farklıysa işçi oradaki yolu kullanır ve raporuna yazar. Yazı, harf, rakam yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
