---
id: "313"
title: Mezar taşı + festival süsleri (flama, şenlik direği, şenlik ateşi, kış feneri)
status: pending
approved_by: pending
family: esya
style_gate: false
cost_est: 6
cost_max: 14
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_grave_se.png
  - Assets/Art/Iso/_aday/iso_deco_bunting_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_deco_maypole_se.png
  - Assets/Art/Iso/_aday/iso_deco_bonfire_se.png
  - Assets/Art/Iso/_aday/iso_deco_lantern_se.png
created: 2026-11-08
---
# 313 Mezar ve festival süsleri

## Amaç
- `item.grave` (`memorial`): `GraveService` ölen karakterin mezarını köy kenarına koyar; küçük taş + toprak höyük. Soy sekmesi mezar adını yazar.
- `FestivalService`: `festival.seed` (ilkbahar), `festival.fair` (yaz), `festival.harvest` (sonbahar), `festival.winter_night` (kış). Festival gününde meydana (`Places["square"]`) süs konur; kod bu 4 görseli festivale göre seçer: seed → flama, fair → şenlik direği, harvest → şenlik ateşi (kuru ot ve kabak ile), winter_night → fener.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

| # | Dosya | Ayak izi | Tuval | Yükseklik | Açıklama |
|---|---|---|---|---|---|
| 1 | `iso_item_grave_se.png` | 1x1 | 64x64 | 26 | `small weathered grey stone grave marker with a rounded top and a low mound of earth in front, a few tiny wildflowers, no text` |
| 2 | `iso_deco_bunting_2x1_se_nw.png` | 2x1 | 112x96 | 56 | `two wooden poles connected by a sagging line of small faded triangular cloth flags in muted red, ochre and blue` |
| 3 | `iso_deco_maypole_se.png` | 1x1 | 80x120 | 90 | `tall slim wooden maypole with a wreath at the top and long muted ribbons hanging down` |
| 4 | `iso_deco_bonfire_se.png` | 1x1 | 80x72 | 34 | `stack of logs forming a bonfire with a moderate orange flame, ring of stones around it` |
| 5 | `iso_deco_lantern_se.png` | 1x1 | 64x88 | 56 | `wooden post with a hanging iron lantern with a warm glowing candle inside` |

- Mezar için açıklamadaki "no text" zorunlu; sprite'ta yazı olmaz, ad arayüzde gösterilir.
- Ateş ve fener gece aydınlatması kodla (ışık) yapılır; sprite'a pişmiş parlama eklenmez (301'deki "pişmiş gölge/alev yasağı" bu iki sprite için **alev serbest**, parlama yasak).
- Maliyet 5 gen + payı → `cost_max` 14.

## Doğrulama
301 tablosu + bonfire/lantern için sıcak renk oranı (alev dışında kırmızı kayma ⚠), mezarda yazı olmaması.
