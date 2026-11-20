---
id: "322b"
title: "Toplama düğümleri ikinci set yeniden (kestane hariç)"
status: running
claimed_at: 2026-11-20T10:33Z
approved_by: user        # kullanıcı kararı 2026-11-20: 322 reddedildi, yeniden üretim
priority: 20
family: bitki
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["301", "311"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_wild_garlic_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_nettle_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_elderflower_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_strawberry_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_rosehip_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_pinecone_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_cress_se_v2.png
created: 2026-11-20
---
# 322b Toplama düğümleri ikinci set yeniden (kestane hariç)

## Neden yeniden
322 kullanıcı tarafından reddedildi (2026-11-20, sohbette toplu inceleme). Reddedilen çıktılar karşılaştırma için `art/results/322-yanyana.png` içinde.

## Tarif
**322 iş dosyasındaki tarifin aynısı** (amaç, ölçüler, yöntem, doğrulama). Yalnızca şu farklar geçerli:

- **Mürver (elderflower):** büyük beyaz zambak ya da kala çiçeği olmamalı. Koyu yeşil, tüylü yapraklı bir çalı; üstünde düz, yassı **şemsiye biçimli öbekler** hâlinde çok sayıda minik krem-beyaz çiçek (her öbek 6-10 piksel genişliğinde, tek tek çiçekler 1 piksel).
- **Kestane bu işte üretilmez:** 378'deki `forage_chestnut_v2` geçerli.
- **Yabani sarımsak, ısırgan, yaban çileği, kuşburnu, kozalak, tere:** 322'deki hâlleri iyi bulundu. Aynı görünümü koru, `art/results/322-yanyana.png` referans.
- Hepsi yandan-üstten (iso SE) bakış; tepeden yıldız biçimi olmamalı.

- Çıktı adları `_v2` ekiyle `_aday/` altına yazılır; var olan dosyaların üstüne yazılmaz. Kabul edilince kod eşlemesi `_v2` dosyalarına çevrilir.
- Raporda 322'in reddedilen çıktısıyla yan yana sayfası (`art/results/322b-yanyana.png`) olmalı.

## Notlar
