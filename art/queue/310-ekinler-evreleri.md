---
id: "310"
title: Ekin büyüme evreleri (şalgam, lahana, pırasa, bezelye, kabak; 3 evre)
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: bitki
style_gate: true
cost_est: 15
cost_max: 24
depends_on: ["301"]      # renk/ölçek ve yöntem 301 sonrası netleşir
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_turnip_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_cabbage_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_leek_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_pea_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_pumpkin_{1,2,3}_se.png
created: 2026-11-08
---
# 310 Ekin büyüme evreleri

## Amaç
`items.json` bitkileri: `item.crop_turnip`, `item.crop_cabbage`, `item.crop_leek`, `item.crop_pea`, `item.crop_pumpkin` (hepsi `plant`, `tilled: true`, `growDays` 4-12). Oyun büyümeyi yüzde olarak bilir; 3 görsel evre yeter: **1 = filiz (0-33%)**, **2 = büyüyor (33-99%)**, **3 = olgun, hasada hazır (100%)**. Kış ölümünde (`diesInWinter`) ayrı sprite yok; kod ekini kaldırır.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

- Ayak izi 1x1, tuval 64x64, kılavuz `--n 1`, yükseklik: evre 1 → 10, evre 2 → 22, evre 3 → 32 (kabak ve lahana 28 ile sınırlı, bezelye çitli 40).
- Taban: işlenmiş toprak rengi elmas (kılavuz rengi sonra silinir); bitki toprakta durur, toprak sprite'ın parçası olarak çizilmez (zemin `iso_ground_dirt_*` ile verilir). Bu yüzden açıklamaya "no soil" yazılır.
- Açıklama metinleri (her biri + ortak ek; ortak ekte "dark brown wood" yerine "muted green and earthy colours" yaz):
  - turnip: `{1: tiny sprout with two small leaves, 2: leafy green tops with a pale purple-white bulb top just visible, 3: large round white-purple turnip half out of the ground with full leafy tops}`
  - cabbage: `{1: two small round leaves, 2: loose green rosette of broad leaves, 3: tight round pale-green cabbage head wrapped in outer leaves}`
  - leek: `{1: thin grass-like shoots, 2: several upright flat green blades, 3: thick white stalks with long blue-green leaves}`
  - pea: `{1: small curly sprout with a short wooden stick, 2: green vine climbing a stick trellis, 3: tall vine on a trellis with visible plump pea pods}`
  - pumpkin (oyunda "Su kabağı", *Lagenaria*; dosya adı aynı kalır): `{1: two round seed leaves, 2: broad soft leaves with a curling vine and white flowers, 3: pale green bottle gourd with a narrow neck lying among big leaves}` (dönem: turuncu balkabağı Amerika kökenli. Karar 17.11.2026, Project 20)
- Aynı bitkinin üç evresi aynı renk paleti ve kamerayla; evreler arasında taban elması ve piksel sınır kutusu orta çizgide hizalı olmalı (animasyonsuz geçiş).
- Maliyet: 15 parça × 1 gen = 15. İkinci deneme payı 9 → `cost_max` 24.

## Doğrulama
301 doğrulama tablosu + şu ek: her bitkinin 3 evresi yan yana önizleme; evre boyları artan sırada mı; olgun evre ürün ikonuyla (316) aynı renkte mi.

## Notlar
- Şalgam evresi `item.crop_turnip` için ilk görseldir; dosya adındaki `crop_turnip` pivot tablosu önek eşleşmesini kullanır.
- 2026-11-18 (tamir oturumu, 0 gen): stil kapısı bitki ailesinde yanlış ayarlıydı (palette 3 yeşil, aile kilidi kurulmamış). Reddedilen adayların doygunluk düzeltilmiş hâlleri `art/results/310-duzeltme/`, karşılaştırma `art/results/310-duzeltme-once-sonra.png`, ayrıntı `art/results/bitki-duzeltme.md`. Durum değişmedi; kabul/red kararı kullanıcının.
- 2026-11-20: bitki stil kapısı düzeltildiği için yeniden üretime alındı (kullanıcı: "hepsinin üretimini başlat"). Eski adaylar ve `310-duzeltme` duruyor.
