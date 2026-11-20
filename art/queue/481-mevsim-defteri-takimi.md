---
id: "481"
title: "Mevsim Defteri takımı: dört mevsimin ödül süsü (4)"
status: running
claimed_at: 2026-11-20T11:06Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("tüm kod işlerini hallet geriye sadece asset üretme ve test etme kalsın"), Project belgesi 46 §6.3
priority: 40
family: esya
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["301", "306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_defter_spring_ribbon_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_defter_summer_sheaf_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_defter_autumn_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_defter_winter_lantern_se.png
created: 2026-11-20
---
# 481 Mevsim Defteri takımı (4)

## Amaç
Belge 46 §6.3: hane bir mevsimde Mevsim Defteri'nden en az 5 iş bitirirse mevsim takımından bir parça kazanır (`RequestCatalog.SeasonSet`). İlk parça bu dört yeni süs, sonrakiler var olan mevsim süsleri (405). Satılmaz, yalnızca ödül (`craftonly`, fiyat 0). Kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Duvar eşyaları **306 ile aynı** (`sw_ne`, 1 kenar), masa ve raf üstü **301 ile aynı** (yuva S). Teslim `cp -n`, `.meta` yok. Ortak not: nakış ve köy işi; mevsim renkleri soluk, parlak renk yok.

| # | items.json id | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.defter_spring_ribbon` (ilkbahar) | `iso_wall_item_defter_spring_ribbon_sw_ne.png` | 1 kenar, orta | `long embroidered linen ribbon with small green leaves and white blossoms, hung from a wooden peg` |
| 2 | `item.defter_summer_sheaf` (yaz) | `iso_wall_item_defter_summer_sheaf_sw_ne.png` | 1 kenar, orta | `small bound sheaf of golden wheat ears tied with a red woollen cord, hung on the wall` |
| 3 | `item.defter_autumn_basket` (sonbahar) | `iso_item_defter_autumn_basket_se.png` | yuva S | `tiny wicker basket with chestnuts, a red apple and a bunch of grapes` |
| 4 | `item.defter_winter_lantern` (kış) | `iso_item_defter_winter_lantern_se.png` | yuva S | `small iron and horn-pane lantern with a lit tallow candle inside, tied with a sprig of juniper` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`; 4 numara alev içerir: `--light` ekle. Dönem: boynuz camlı fener ve don yağı mumu ortaçağ; kestane, elma, üzüm, buğday, ardıç Eski Dünya. Cam fener değil boynuz levha.

## Notlar
- Eşya → dosya eşlemesi (`items.json` `art` alanı ya da `IsoArt.cs`) ve pivotlar kabulden sonra kod oturumunun işidir (405 ile aynı).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
