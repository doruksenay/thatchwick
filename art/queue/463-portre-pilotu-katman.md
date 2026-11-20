---
id: "463"
title: "P4 portre pilotu: 2 yüz × 2 göz × 2 saç, 64 ve 96"
status: pending
approved_by: pending     # kullanıcı kararı 2026-11-19: kota sıfırlanınca (2026-12-18); o gün user yap
family: karakter
style_gate: false
cost_est: 40
cost_max: 60
depends_on: ["461"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_katman_<boyut>_<yüz>_<göz>_<saç>.png
  - art/results/463-portre-pilotu-katman.md
created: 2026-11-20
---
# 463 P4 portre pilotu

## Amaç
Belge 43 §4.4: portre parçaları farklı yüz şekillerine "yapıştırılmış" gibi durmamalı; dünyadaki saçla (461) aynı saç gibi okunmalı. Ayrıca portre boyutu kararı (64 mü 96 mı, belge 43 §7.4).

## Tarif
- 020 portre yöntemi (katman, "only changes" + maske), iki boyutta: 64x64 ve 96x96.
- Yüz şekilleri: oval, köşeli. Gözler: badem (almond), iri (wide). Saçlar: 461'in `short` ve `shoulder` kütlesinin portre eşi (461 sonuçları referans görsel olarak verilir).
- Her yüz şekli için sabit yüz çizgileri: göz hizası, burun ucu, ağız hizası tabanda işaretlenir; göz parçaları bu hizaya göre inpaint edilir.
- Renk: saç referans saç rampasında (461 ile aynı), ten 020 gibi gri tonlu.

## Doğrulama
- 2 × 2 × 2 = 8 birleşim, iki boyutta yan yana sayfa.
- Kullanıcıya iki soru: boyut (64/96) ve parçalar yüze oturuyor mu.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
