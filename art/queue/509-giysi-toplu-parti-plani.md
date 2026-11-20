---
id: "509"
title: Giysi toplu partisi: plan ve bölme (belge 48 §13.2, 51 §5)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["500", "501", "503", "504"]
max_attempts: 1
pixellab: false
outputs:
  - art/reports/509-giysi-toplu-parti-plani.md
created: 2026-11-20
---
# 509 Giysi toplu partisi: plan ve bölme (belge 48 §13.2, 51 §5)

## Amaç
Pilotlar kabul edildikten sonra kalan çizimler: baş 2. dalga (16), pelerin ve omuz (13), boyun (3), bel çantası ve kese (2), portre eşleri (~33), doku örnekleri (4), çocuk boyu (çiçek tacı, küçük şal, 4 baş modeli), bebek mankeninin kalan yönleri, 2 baston daha. Toplam ~1.900 gen.

## Tarif
PixelLab çağrılmaz. `art/budget.md` kurallarıyla her biri `cost_max <= 60` iş dosyalarına böl (model başına bir iş), `approved_by: pending` yaz; kullanıcının bütçe kararı (18.12 kota sıfırlanmadan önce) gelince `user` yapılır. Model listesi ve öncelik: `Assets/Resources/Content/clothing.json` (`flags: wave1` olanlar 43b'de).

## Doğrulama
Rapor: iş listesi, toplam gen, dönem bölünmesi önerisi.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
