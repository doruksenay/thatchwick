---
id: "775"
title: "HUD meşe: Yapı ve Defter düğmeleri, rozet"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21: HUD yönü B (oyma meşe), Project belgesi 66 §5 S5
priority: 1
family: arayuz
style_gate: false
cost_est: 6
cost_max: 15
depends_on: ["745"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_mese_btn_build.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_btn_build_down.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_btn_book.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_btn_book_down.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_badge.png
created: 2026-11-21
---
# 775 HUD meşe: Yapı ve Defter düğmeleri, rozet

## Amaç
Sağ alt köşedeki iki kalıcı düğme (Yapı modu, Defter penceresi) ve Defter'deki yeni içerik rozeti (Project belgesi 66 §3). 745'te sayfa 2 düğme üretmedi; bu iş onları **parça parça** üretir.

## Ders (745/744/746 raporları)
`create_ui_asset` sayfa üretiminde parçaları ayrı çizmiyor, tek panel çiziyor. Bu yüzden **her düğme ayrı çağrı**. Ölçek düşürme yok: araç hangi boyutta ürettiyse o native kabul edilir (774'teki kural).

## Tarif
- **Araç:** `create_image_pixflux`, saydam arka plan, `seed=0`, palet `art/style/palette_arayuz.json`, `max_colors=32` (araç izin veriyorsa). Boyut: **48x48** (aracın alt sınırı daha büyükse alt sınır; ölçek düşürme yapılmaz, boyut raporda yazılır).
- **Ortak metin eki:** `carved dark oak round shield button, leaf and vine carving on the rim, muted moss green leaves (not bright green), small wrought iron rivets, dark warm brown outline (no pure black), light from top-left, pixel art game HUD icon, transparent background, no text, no letters`
- **Çağrılar (4 + 1):**
  1. `btn_build`: ortada çapraz çekiç ve mala, demir başlı ahşap saplı, krem iç yuvarlak zemin üstünde.
  2. `btn_build_down`: (1) ile aynı istem + `pressed state: inner area slightly darker, shadow smaller, icon 1 px lower`. Mümkünse (1)'in çıktısı `init_image` olarak verilir (güç ≤ 300) ki aynı düğme olsun.
  3. `btn_book`: ortada kapalı deri kaplı kitap, bordo kapak, metal köşeli, krem iç yuvarlak zemin üstünde.
  4. `btn_book_down`: (3) + basılı hâl, (2) gibi.
  5. `badge`: `small round wrought iron rivet badge with a dark red center and a cream exclamation mark`, **24x24** (alt sınır daha büyükse alt sınır).
- Basılı hâller (2, 4) `init_image` ile tutmazsa: normal hâli Python ile kopyala, iç dairesel alanı paletten bir basamak koyulaştır, ikonu 1 px aşağı kaydır, alt gölgeyi 1 px azalt. Bu yol seçilirse raporda yazılır (0 gen).

## Doğrulama
- `python tools/art/style.py check --family arayuz <dosyalar>`: GEÇMELİ. Geçmeyen parça için en fazla bir yeniden deneme (yalnızca o parça).
- `art/results/775-yanyana.png`: dört düğme ve rozet 1×, 2×, 3×; rozet `btn_book`'un sağ üstüne bindirilmiş hâliyle de.
- Ölçüt: çekiç+mala ve kitap 1×'te tek bakışta tanınmalı; normal ve basılı hâl yan yana farkı görülmeli.
- Yeşil yapraklar limon yeşili değil, soluk yosun yeşili (palet). Stardew'in düz ahşap düğmelerine benzememeli: oyma yaprak kenar ve demir perçin şart.

## Notlar
- Masaüstü kopyası: `C:\Users\User\OneDrive\Desktop\art\hud_pilot\` (yerel işçide).
- 744 ve 746'nın düğmeleri reddedilen yönlere ait; referans alınmaz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
