---
id: "343"
title: Taş ocağı, demir ocağı, çömlekçi çarkı, küp, mayalama fıçısı, mum tezgâhı, av pususu, rüzgâr fırıldağı, yazı tahtası, kopuz
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (ikinci dalga beceriler, 2026-11-17); "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 16
cost_max: 22
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_quarry_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_forge_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_pottery_wheel_se.png
  - Assets/Art/Iso/_aday/iso_item_storage_jar_se.png
  - Assets/Art/Iso/_aday/iso_item_brew_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_chandler_table_se.png
  - Assets/Art/Iso/_aday/iso_item_hunting_blind_se.png
  - Assets/Art/Iso/_aday/iso_item_weather_vane_se.png
  - Assets/Art/Iso/_aday/iso_item_slate_se.png
  - Assets/Art/Iso/_aday/iso_item_lute_se.png
created: 2026-11-17
---
# 343 İkinci dalga beceri istasyonları

## Amaç
Madencilik (taş ocağı), demircilik (demir ocağı ve örs), çömlekçilik (çark, küp), biracılık (mayalama fıçısı), mumculuk (mum tezgâhı), avcılık (av pususu), hava bilgisi (rüzgâr fırıldağı), öğretmenlik (yazı tahtası), müzik (kopuz). Ahşap olanlar ham, açık tonlu.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- taş ocağı (2x2) = small open stone quarry pit with cut blocks and a pickaxe; demir ocağı (2x2) = blacksmith forge with glowing coals, bellows and an anvil; çömlekçi çarkı (1x1) = wooden potter's kick wheel with a clay pot; küp (1x1) = large terracotta storage jar with a wooden lid; mayalama fıçısı (1x1) = oak fermenting barrel with a tap; mum tezgâhı (1x1) = small table with hanging dipped candles and a wax pot; av pususu (1x1) = hunter's blind made of branches and leaves; rüzgâr fırıldağı (1x1) = wooden weather vane on a pole with a rooster; yazı tahtası (1x1) = wooden-framed slate board on an easel; kopuz (1x1) = Turkic long-necked lute leaning on a small stand.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
