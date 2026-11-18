---
id: "345"
title: Lazımlık, yıkanma kulübesi, havluluk
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (tuvalet ve su, 2026-11-17); "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 6
cost_max: 9
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_chamber_pot_se.png
  - Assets/Art/Iso/_aday/iso_item_bath_hut_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_towel_rack_se.png
created: 2026-11-17
---
# 345 Tuvalet ve banyo eşyaları

## Amaç
Yeni Tuvalet ihtiyacı ve banyo için su taşıma. Lazımlık yatak odasında durur (geceleri dışarı çıkmadan tuvalet), dolunca boşaltılır, gübre verir. Yıkanma kulübesi bahçede durur; karakter önce kuyudan, göletten ya da dereden bir kova su getirir, sonra içinde yıkanır. Havluluk ıslak karakteri kurular. Dış hela (`item.outhouse`) 304'te var, yeni görsel gerekmez.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- lazımlık (1x1, kil, `finishGroup: clay`) = small glazed clay chamber pot with a lid, on the floor;
- yıkanma kulübesi (1x2, ahşap, dış mekân) = small wooden bathing shed with a plank door ajar, a wooden tub visible inside, a water bucket by the door;
- havluluk (1x1, ahşap) = simple wooden towel rack with a hanging linen towel.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri). Kulübe 1x2: ayak izi iki karo.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
