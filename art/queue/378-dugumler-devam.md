---
id: "378"
title: Toplama düğümleri devam (fındık, böğürtlen, kestane)
status: accepted
claimed_at: 2026-11-19T07:32Z
priority: 10
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 (onay grubu 1: "Kabul, 3'ü yeniden")
family: bitki
style_gate: false
cost_est: 3
cost_max: 9
depends_on: ["373"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_hazel_se.png
  - Assets/Art/Iso/_aday/iso_forage_berry_bush_v2_se.png
  - Assets/Art/Iso/_aday/iso_forage_chestnut_v2_se.png
created: 2026-11-18
---
# 378 Toplama düğümleri devam

## Amaç
373'te üç düğüm tutmadı:
- **fındık:** iki denemede de ağaç ve çim elması çıktı;
- **böğürtlen:** eğreltiye benziyor, kullanıcı reddetti;
- **kestane:** mor görünüyor, kullanıcı reddetti.

Reddedilen dosyaların üzerine yazılmaz, `_v2` adlarıyla gelir.

## Tarif
Yöntem **373 ile aynı** (bitki referansları, açıklama eki, `fit`, `check`). Farklar:
- **Kılavuz:** 373 raporunun önerisi. Kılavuz yok; saydam 64x64 tuval, maske tuvalin tamamı, pivot (32,16). Düğümler küçük, taban elması onları ağaca ve çim parçasına itiyordu.
- **Açıklamalar:**
  1. `iso_forage_hazel_se.png`: `low hazel bush, several thin upright stems from one base, round serrated leaves and a few brown hazelnuts in green husks, a shrub not a tree, no grass` (yükseklik 28)
  2. `iso_forage_berry_bush_v2_se.png`: `blackberry bramble: tangled arching thorny canes with broad three-part leaves and clusters of glossy black and red berries, low and wide, no fern, no grass` (yükseklik 24)
  3. `iso_forage_chestnut_v2_se.png`: `three fallen sweet chestnuts: shiny reddish-brown nuts half out of spiky pale green husks, lying on the ground, brown and green only, no purple` (yükseklik 10)
- Maliyet 3 × 1 gen. İkinci deneme payı (yalnızca biçim): `cost_max` 9.

## Doğrulama
- 373 doğrulaması.
- Yan yana sayfası: kabul edilen 373 düğümleri (otlar, mürver, çilek, kuşburnu, ısırgan) ve köylü ile.
- Fındık ağaçtan küçük ve çalı gibi. Böğürtlen eğrelti değil, dikenli ve meyveli. Kestane kahve ve yeşil.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
