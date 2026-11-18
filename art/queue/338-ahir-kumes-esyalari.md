---
id: "338"
title: Ahır ve kümes eşyaları (12)
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 20
family: esya
style_gate: false
cost_est: 14
cost_max: 18
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_nest_box_se.png
  - Assets/Art/Iso/_aday/iso_item_stall_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_horse_stall_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_feed_trough_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_water_trough_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_hay_bale_se.png
  - Assets/Art/Iso/_aday/iso_item_salt_lick_se.png
  - Assets/Art/Iso/_aday/iso_item_barn_lantern_se.png
  - Assets/Art/Iso/_aday/iso_item_saddle_rack_se.png
  - Assets/Art/Iso/_aday/iso_item_animal_bell_se.png
  - Assets/Art/Iso/_aday/iso_item_collection_box_se.png
  - Assets/Art/Iso/_aday/iso_item_feed_chute_2x1_se.png
created: 2026-11-17
---
# 338 Ahır ve kümes eşyaları

## Amaç
Ahır, kümes ve at ahırı artık duvarla kurulan odalardır; içleri bu eşyalarla döşenir (katalogda "Ahır ve kümes"). Bölmeler barınaktır (folluk 2 tavuk, ahır bölmesi 1 büyükbaş, at bölmesi 1 at), öbürleri hayvan konforu ve otomasyon verir. Ahşap olanlar `finishGroup: wood`: motorda boyanacağı için **ham, açık tonlu ahşap** üretilir.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı). Köylü / zanaatkâr ortaçağ ahırı havası, gölge yok.
- folluk (1x1) = wooden nesting box with straw, small roof; ahır bölmesi (1x2) = low wooden cattle stall partition with hay rack; at bölmesi (1x2) = taller wooden horse stall with half door; yemlik (2x1) = long wooden feed trough with grain; su yalağı (2x1) = stone water trough with water; saman balyası (1x1) = rectangular hay bale; tuz yalağı (1x1) = salt lick block on a short wooden post; ahır feneri (1x1) = iron lantern on a wooden post; eyer sehpası (1x1) = wooden saddle stand (boş); hayvan çanı (1x1) = bronze bell hanging on a small wooden frame; toplama sandığı (1x1) = large lidded wooden collection chest; yem oluğu (2x1) = wooden grain hopper with chute.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-18: yeniden `pending` (Claude, kurtarma): 2026-11-18T09:53Z sahiplenmesinden sonuç gelmedi.
