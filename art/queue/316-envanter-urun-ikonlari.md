---
id: "316"
title: Envanter ve ürün ikonları (hammadde, ürün, yemek, toplanan)
status: pending
approved_by: pending     # 25'in üstünde olabilir; kullanıcı onayı şart
cost_est: 24
cost_max: 40
depends_on: ["315"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_{seed_turnip,seed_cabbage,seed_leek,seed_pea,seed_pumpkin}.png
  - Assets/Art/Iso/_aday/icon_item_{turnip,cabbage,leek,pea,pumpkin}.png
  - Assets/Art/Iso/_aday/icon_item_{pickled_cabbage,pickled_turnip,dried_pea,dried_leek}.png
  - Assets/Art/Iso/_aday/icon_item_{stew_plain,stew_good,stew_fine,soup_pea,soup_pea_good,omelet}.png
  - Assets/Art/Iso/_aday/icon_item_{berries,mushroom,nut,honey,herb,egg}.png
  - Assets/Art/Iso/_aday/icon_item_{log,firewood,plank,sapling_oak}.png
created: 2026-11-08
---
# 316 Envanter ve ürün ikonları

## Amaç
`items.json` `resource` kategorisindeki 36 ürünün ikonu yok (envanter listesi, gün özeti `ItemDeltas`, pazar teklifleri, hediye seçici ve satın alma menüsü bunları gösterir). 32x32 ikonlar.

## Tarif
- 315 ile aynı stil ve araç; 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk.
- Aileler tutarlı: tohumlar küçük keseler (aynı kese, farklı tohum şekli), ham sebzeler doğal renk, turşular kavanoz, kuruluklar bağlanmış demet, yemekler kase (yemek kalitesi kasenin doluluğu ve garnitürle: plain < good < fine), toplananlar doğal.
- Sayfa başı 6 ikon (sprite sheet) üretip kes; ~36 ikon / 6 sayfa × ~2 gen + payı ≈ 24, `cost_max` 40 → **kullanıcı onayı şart** (25'in üstü).

## Doğrulama
Boyut 32x32, ≤16 renk, hepsi koyu/açık zeminde 2x ölçekle okunuyor mu, yemek kalite basamakları ayırt ediliyor mu, 310'daki olgun ekin renkleriyle uyum.

## Notlar
- `item.grave` ve mobilya ikonları bu işte yok (dünya sprite'ı kullanılır).
