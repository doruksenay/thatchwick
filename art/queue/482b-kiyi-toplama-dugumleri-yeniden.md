---
id: "482b"
title: "Deniz kıyısı toplama düğümleri yeniden (3)"
status: accepted
claimed_at: 2026-11-20T10:18Z
approved_by: user        # kullanıcı kararı 2026-11-20: 482 reddedildi, yeniden üretim
priority: 20
family: zemin
style_gate: false
cost_est: 3
cost_max: 6
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_tidepool_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_driftline_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_sea_sand_se_v2.png
created: 2026-11-20
---
# 482b Deniz kıyısı toplama düğümleri yeniden (3)

## Neden yeniden
482 kullanıcı tarafından reddedildi (2026-11-20, sohbette toplu inceleme). Reddedilen çıktılar karşılaştırma için `art/results/482-yanyana.png` içinde.

## Tarif
**482 iş dosyasındaki tarifin aynısı** (amaç, ölçüler, yöntem, doğrulama). Yalnızca şu farklar geçerli:

- **Gelgit havuzu (tidepool):** mor taş kutu ya da çerçeve olmamalı. Doğal kıyı kayalarının arasında, kenarları düzensiz, sığ bir su birikintisi olmalı. Kayalar gri-kahve ve ıslak görünmeli, üstünde yeşil-kahve yosun, havuzda bir iki midye ve küçük bir deniz yıldızı olsun. Kayalar karonun sınırını kutu gibi doldurmamalı, kum zemin aralardan görünmeli. Mor ve parlak nane yeşili kullanılmamalı.
- **Sahil çizgisi (driftline) ve deniz kumu (sea_sand):** aynı tarifle yeniden üretilir. Kum, 482b'deki havuzla aynı renk ailesinden olmalı.

- Çıktı adları `_v2` ekiyle `_aday/` altına yazılır; var olan dosyaların üstüne yazılmaz. Kabul edilince kod eşlemesi `_v2` dosyalarına çevrilir.
- Raporda 482'in reddedilen çıktısıyla yan yana sayfası (`art/results/482b-yanyana.png`) olmalı.

## Notlar

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı: -->
- 2026-11-21: kullanıcı kararı (sohbet). **Sahil çizgisi (driftline_v2) kabul**, oyunda kullanılıyor. **Gelgit havuzu v2 saklanır** ("belki kullanırız"; şimdilik oyunda bağlı) ama kayaların üstünde tarifte olmayan yeşil çalılar var, karo blok gibi duruyor: çalısız yenisi 482c. **Deniz kumu v2 reddedildi**: kenarında öbür zemin karolarında olmayan koyu dış çizgi var; yenisi 482c.
