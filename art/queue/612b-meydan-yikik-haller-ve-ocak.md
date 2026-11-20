---
id: "612b"
title: "Köy meydanı: 7 yapının yıkık hâli ve ortak ocak (612 devamı)"
status: review
claimed_at: 2026-11-20T17:44Z
approved_by: user        # otomatik akış (art/budget.md); 612'nin eksikleri, karar: kullanıcı 20.11.2026 (Project belgesi 64c §3)
priority: 15
family: yapi
style_gate: false
cost_est: 16
cost_max: 32
depends_on: ["612"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_square_hearth_se.png
  - Assets/Art/Iso/_aday/iso_square_{well,hearth,cellar,millwheel,belltower,market_hall,bridge}_ruin_se.png
created: 2026-11-20
---
# 612b Köy meydanı: yıkık hâller ve ortak ocak

## Amaç
612 kısmi bitti (`art/results/612-meydan-onarimi-yapilari.md`): onarılmış 6 yapı ve 2 iskele teslim; ortak ocak ve yedi yıkık hâl teslim edilmedi. `init_image` gücü 150-200 ile yıkık çıktılar onarılmış hâlden ayırt edilemedi.

## Tarif
1. **Ortak ocak** (`iso_square_hearth_se`, 2x2, 128x128): 612'deki tarif. Ek kısıt: **sönük**; kor, alev, kırmızı ya da turuncu piksel yok (`style.py check --light` gerekmez). `--max-colors 48` içinde kalmalı. `tools/art/footprint_guide.py` kılavuzuyla üret; 2x2 elması doldursun, kendi zemin tabanını taşımasın.
2. **Yıkık hâller:** onarılmış hâl (`_aday/iso_square_<ad>_se.png`, ocak için 1. adımın çıktısı) taban alınır.
   - Önce `edit_image` (ya da `inpaint_image_pro_flash`) ile, maske üst yapıyı kapsar, taban elması korunur. Metin: 612 tablosundaki "Yıkık" sütunu.
   - Tutmazsa `create_image_pixflux` + `init_image`, güç **50-80** (612'de 150-200 fazla benzer çıktı verdi).
   - Ölçüt: yan yana bakınca yapı **açıkça yıkık** okunmalı (eksik çatı, devrik direk, yığılmış taş), ama ayak izi ve taban elması aynı piksellerde.
3. Köprü yıkık hâli 612'de 60 renkti: renk sayısı `--max-colors 48` içine indirilmeli (`style.py fit` ya da renk azaltma).

## Doğrulama
`validate_iso --kind object --max-colors 48`, `style.py check --family yapi`. Yan yana sayfası `art/results/612b-yanyana.png`: her satırda yıkık | onarılmış | yıkık + iskele bindirmesi. Kuyu ve kilerin 2x2 elmasa göre küçük kaldığı 612'de not edildi; yıkık hâlleri onarılmış hâlin ölçeğini izler (612 dosyaları değişmez).

## Sorular
Kısmi sonuç: 4/8 teslim (çark, çan kulesi, pazar yeri, köprü yıkık). Ocak, kuyu ve kiler yıkığı teslim edilmedi; ayrıntı `art/results/612b-meydan-yikik-haller-ve-ocak.md`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
