---
id: "776"
title: "HUD meşe: ihtiyaç, hız ve mevsim ikonları (15)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21: HUD yönü B (oyma meşe), Project belgesi 66 §5 S5
priority: 1
family: arayuz
style_gate: false
cost_est: 15
cost_max: 30
depends_on: ["745"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_mese_{need_hunger,need_energy,need_warmth,need_fun,need_hygiene,need_social,need_toilet}.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_{speed_pause,speed_1,speed_2,speed_3}.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_{season_spring,season_summer,season_autumn,season_winter}.png
created: 2026-11-21
---
# 776 HUD meşe: ihtiyaç, hız ve mevsim ikonları (15)

## Amaç
Karakter kartındaki 7 ihtiyaç ikonu, saat yanındaki 4 hız ikonu, saat levhasındaki 4 mevsim ikonu (Project belgesi 66 §3). 744-746'da tek sayfada üretilen ikonlar tanınmaz çıktı; bu iş **her ikonu ayrı çağrıyla** üretir.

## Tarif
- **Araç:** `create_image_pixen`, **16x16** tuval, saydam arka plan, palet `art/style/palette_arayuz.json`. Her ikon ayrı çağrı (15 çağrı, ~1 gen/çağrı). Araç 16'yı kabul etmezse en küçük izin verilen boyut (20 ya da 24); ölçek düşürme yok, boyut raporda yazılır. 15 ikonun hepsi **aynı boyutta** olmalı.
- **Ortak metin eki:** `single simple game HUD icon, flat colors, no texture, 1 px dark warm brown outline (no pure black), light from top-left, readable silhouette, centered, transparent background, no text, no numbers`
- **Stil bağı:** ilk ikon (`need_hunger`) üretildikten sonra kalan 14 çağrıda o çıktı stil referansı olarak verilir (araç destekliyorsa; bulut işçisi görsel aktaramıyorsa yalnızca metin). İkonlar meşe çerçeveye yerleşecek, kendileri ahşap değil: sıcak, doygunluğu düşük renkler.
- **İkonlar:**
  | Dosya | İstem |
  |---|---|
  | `need_hunger` | round loaf of bread |
  | `need_energy` | crescent moon |
  | `need_warmth` | small campfire flame |
  | `need_fun` | wooden spinning top (topaç) |
  | `need_hygiene` | water droplet with a small bar of soap |
  | `need_social` | two simple heads side by side |
  | `need_toilet` | small wooden chamber pot |
  | `speed_pause` | two vertical bars, wrought iron |
  | `speed_1` | one right-pointing triangle, wrought iron |
  | `speed_2` | two right-pointing triangles, wrought iron |
  | `speed_3` | three right-pointing triangles, wrought iron |
  | `season_spring` | green sprout with two leaves |
  | `season_summer` | warm yellow sun |
  | `season_autumn` | orange-brown oak leaf |
  | `season_winter` | pale blue snowflake |
- Hız ikonları birbirinin kopyası gibi görünmeli: `speed_1` çıktısının üçgeni Python ile çoğaltılarak `speed_2`/`speed_3` üretilebilir (0 gen; raporda yaz). 16 px'e üç üçgen sığmıyorsa üçgenler 1 px üst üste biner.

## Doğrulama
- `python tools/art/style.py check --family arayuz <dosyalar>`: GEÇMELİ; geçmeyen ikon için en fazla bir yeniden deneme.
- `art/results/776-ikonlar-x3.png`: 15 ikon 1× ve 3×, adlarıyla; ayrıca meşe `bar` (774) yanına dizilmiş 7 ihtiyaç ikonu satırı (774 henüz yoksa atla).
- Ölçüt: her ikon 1×'te tek bakışta tanınmalı. Tanınmayan ikon raporda "okunaksız" diye işaretlenir.

## Notlar
- Masaüstü kopyası: `C:\Users\User\OneDrive\Desktop\art\hud_pilot\` (yerel işçide).
- Bütçe: 774 + 775 + 776 toplam ≤ 45 gen.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
