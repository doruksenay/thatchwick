---
id: "315"
title: Arayüz ikon seti (ihtiyaç, ruh hali, etkinlik, hava, gösterge)
status: running
claimed_at: 2026-11-18T12:46Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: arayuz
style_gate: false
cost_est: 14
cost_max: 22
depends_on: ["210"]      # 2026-11-17: arayüz stili artık 210 (karakter arayüzü pilotu) ile seçiliyor
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_need_{hunger,energy,warmth,social,fun,hygiene}.png
  - Assets/Art/Iso/_aday/ui_icon_mood_{great,good,ok,bad,awful}.png
  - Assets/Art/Iso/_aday/ui_icon_act_{sleep,eat,warm,social,fun,chop,farm,cook,craft,work,walk,care,love,idle,dead}.png
  - Assets/Art/Iso/_aday/ui_icon_weather_{sun,rain,snow,storm}.png
  - Assets/Art/Iso/_aday/ui_icon_misc_{festival,market,winter,coin,xp,heart}.png
created: 2026-11-08
---
# 315 Arayüz ikon seti

## Amaç
`StatusHud` ve `VillagePanel` şu an metin ve düz renkli çubuklarla çiziyor. Anahtarlar koddadır: `ActivityDescriber` etkinlik anahtarları (`sleep, eat, warm, social, fun, chop, farm, cook, craft, work, walk, care, love, idle, dead`), `WeatherSystem.Icon` (`sun, rain, snow, storm`), ihtiyaçlar (`NeedType`), ruh hali 5 kademe. Toplam **35 ikon**, 16x16 tuval (Unity'de 2x/3x tam sayı ölçek).

## Tarif
- Araç: `create_image_pixflux` ya da `create_map_object` (şeffaf, 16x16 desteklenmiyorsa 32x32 üretip en yakın komşuyla 16'ya indir; ikincisi tercih sırası değil geri dönüş). 210'un yönüne uy (`art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen`) (renk, kontur kalınlığı). Tek renk koyu kontur, düz gölgeleme, ışık sol-üstten.
- Çizim dili: kasıtlı sade, 3-5 renk, hepsi aynı kontur. Anlamı renkten değil şekilden okunmalı (körlük dostu): ihtiyaç çubuğu ikonları ayrı silüetli olmalı.
- Gruplar tek çağrıda 5-8'lik sayfa olarak üretilirse (sprite sheet) çıktı ikonlara 1 piksel boşlukla kesilir; kesim betiği `tools/art/` altına eklenir (repoya yazmak serbest, geçici betik değil).
- Ruh hali yüzleri: great = gülen, good = hafif gülen, ok = düz ağız, bad = hafif kaşlı, awful = kaşlı ve ağlamaklı. Renk: yeşil→kırmızı yerine ton değişimi + şekil.
- Maliyet: ~35 ikon / 3 gen (sayfa başı) ≈ 14, yeniden deneme payı → `cost_max` 22.

## Doğrulama
Her ikon 16x16 mı, yarı saydam yok mu, renk sayısı ≤12, hepsi 3x ölçekle koyu (#333333) ve açık zemin üstünde okunuyor mu, ihtiyaç ikonları birbirinden ayırt ediliyor mu. Toplu önizleme x4.

## Notlar
- Anahtar → dosya adı eşlemesi kodda `ui_icon_<grup>_<anahtar>` olarak yapılır; anahtar eklemek dosya eklemekle aynı.
