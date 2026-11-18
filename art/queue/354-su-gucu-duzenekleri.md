---
id: "354"
title: Su gücüyle çalışan düzenekler: su hızarı, dink, su çekici
status: running
claimed_at: 2026-11-18T11:58Z
approved_by: user        # otomatik akış (2026-11-17); su şebekesi PR (feat/su-sebekesi)
priority: 20
family: esya
style_gate: false
cost_est: 6
cost_max: 10
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_sawmill_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_fulling_mill_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_trip_hammer_2x1_se.png
created: 2026-11-17
---
# 354 Su gücüyle çalışan düzenekler

## Amaç
Su çarkı (ya da yel değirmeni) gücü alınca her sabah kendiliğinden işleyen yeni düzenekler (Project belgesi 20): **su hızarı** kütüğü tahtaya biçer, **dink** yünü keçeye döver, **su çekici** cevheri demire döver. Hepsi Orta Çağ'da su gücüyle bilinen makineler (13. yüzyıl).

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır**; 339'daki su çarkı ve değirmen taşıyla aynı ahşap tonu ve dişli dili.
- su hızarı (2x2) = medieval water-powered sawmill: wooden frame with a vertical reciprocating saw blade, a log on a carriage, wooden gears on one side;
- dink (2x1) = fulling stocks: two heavy wooden hammers on a cam shaft over a wooden trough with wet cloth;
- su çekici (2x1) = water-powered trip hammer: a long wooden helve with an iron hammer head over a stone anvil block, cam shaft at the back.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzu (301 ölçütleri); 339 su çarkıyla yan yana uyum.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
