---
id: "353"
title: Su işleri eşyaları: çeşme, yalak-lüle, akarsulu hela, sarnıç, yağmur fıçısı, su terazisi
status: blocked
claimed_at: 2026-11-18T11:57Z
approved_by: user        # otomatik akış (2026-11-17); su şebekesi PR (feat/su-sebekesi)
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_fountain_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_kitchen_spout_se.png
  - Assets/Art/Iso/_aday/iso_item_flush_privy_se.png
  - Assets/Art/Iso/_aday/iso_item_cistern_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_rain_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_water_tower_se.png
created: 2026-11-17
---
# 353 Su işleri eşyaları

## Amaç
Su şebekesi tesisatı (Project belgesi 20). Boruya bağlı **çeşme** köyün ortak suyu (kova buradan dolar), mutfakta **taş yalak ve lüle**, içeride **akarsulu hela** (manastır helası gibi: su gelirken yıkanır), yağmur ve ağ fazlasını biriktiren **sarnıç** ve **yağmur fıçısı**, üst kata basınç veren **su terazisi**.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- çeşme (2x1) = medieval stone wall fountain with a carved stone basin and a small bronze spout pouring water into a trough;
- taş yalak ve lüle (1x1) = small carved stone sink with a clay spout on a short wall pipe, kitchen use;
- akarsulu hela (1x1) = wooden privy seat box with a lid, a small wooden water trough running behind it (monastery latrine);
- sarnıç (2x2) = low square stone cistern with a wooden lid and a hand winch;
- yağmur fıçısı (1x1) = wooden barrel with iron hoops under a short wooden gutter spout;
- su terazisi (1x1, uzun) = slender square stone pressure tower about two storeys tall with a small lead tank on top (Ottoman water balance tower), narrow base.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri). Su terazisi 1x1 tabanından taşmamalı, yüksekliği iki kat kadar.

## Sorular
- 2026-11-18 (bulut, şerit 2): `get_balance` plan olarak **Tier 2: Pixel Artisan**, 4999/5000 generation (yenilenme 2026-12-18) döndürdü. `art/budget.md` planı Tier 1 / 2000 gen / yenilenme 2026-12-08 olarak yazıyor ve işçi kuralı "Tier 1 değilse dur". Hiçbir PixelLab çağrısı yapılmadı, harcama 0. Kullanıcı `art/budget.md` "Plan" bölümünü (ve rezerv/günlük tavan geçerliliğini) güncelleyince iş `pending`e alınıp yeniden çalıştırılabilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
