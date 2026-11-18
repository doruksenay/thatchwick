---
id: "341"
title: Ambar, buzluk, buzhane
status: running
claimed_at: 2026-11-18T09:53Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 20
family: esya
style_gate: false
cost_est: 5
cost_max: 8
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_granary_3x2_se.png
  - Assets/Art/Iso/_aday/iso_item_ice_box_se.png
  - Assets/Art/Iso/_aday/iso_item_ice_house_2x2_se.png
created: 2026-11-17
---
# 341 Ambar ve soğuk depo

## Amaç
Yiyecek depoya otomatik yerleşir: en çabuk bozulan en soğuk yere. Ambar büyük ve havadardır (x0,85), buzluk ve buzhane buz varken çok soğuktur (x0,2). Buz kışın kuyudan ya da su yalağından kesilir. Ahşap olanlar ham, açık tonlu (`finishGroup: wood`).

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- ambar (3x2) = raised wooden granary on stone stilts with a thatched roof and a small ladder (Karadeniz serenderi gibi); buzluk (1x1) = wooden ice chest with iron bands, lid slightly open, ice blocks and straw inside; buzhane (2x2) = small half-buried stone ice house with a thatched conical roof and a wooden door.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
