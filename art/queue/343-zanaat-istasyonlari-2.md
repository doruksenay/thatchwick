---
id: "343"
title: Taş ocağı, demir ocağı, çömlekçi çarkı, küp, mayalama fıçısı, mum tezgâhı, av pususu, rüzgâr fırıldağı, yazı tahtası, kopuz
status: accepted
approved_by: user        # kullanıcı onayı 2026-11-20 ("hepsinin üretimini başlat")
worker: batch            # 2026-11-18: toplu işçi biçimine çevrildi (Claude'suz); onay hâlâ kullanıcıda
priority: 20
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

## Parametreler (toplu işçi)
Tarifin makine okunur hâli (`tools/art/batch_worker.py`). Tuval: ayak izi + yükseklik + 24 px (8'e yuvarlanır); `"fill": false` olan ince ya da doğal eşyalarda ayak izi kaplama kuralı uygulanmaz. Açıklamalar yukarıdaki tariften birebir.

```json
{
  "tool": "map_object",
  "family": "esya",
  "max_attempts": 2,
  "common": {
    "view": "high top-down",
    "detail": "low detail",
    "shading": "basic shading",
    "outline": "single color outline",
    "suffix": ", medieval peasant village object, earthy muted palette, dark brown wood, flat simple shading, single dark outline, light from top-left, no ground, no shadow, no extra objects, transparent background"
  },
  "items": [
    {
      "name": "quarry",
      "out": "Assets/Art/Iso/_aday/iso_item_quarry_2x2_se.png",
      "canvas": [
        144,
        112
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 24
      },
      "description": "small open stone quarry pit with cut blocks and a pickaxe",
      "fill": false
    },
    {
      "name": "forge",
      "out": "Assets/Art/Iso/_aday/iso_item_forge_2x2_se.png",
      "canvas": [
        144,
        128
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 40
      },
      "description": "blacksmith forge with glowing coals, bellows and an anvil"
    },
    {
      "name": "pottery_wheel",
      "out": "Assets/Art/Iso/_aday/iso_item_pottery_wheel_se.png",
      "canvas": [
        80,
        88
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 28
      },
      "description": "wooden potter's kick wheel with a clay pot"
    },
    {
      "name": "storage_jar",
      "out": "Assets/Art/Iso/_aday/iso_item_storage_jar_se.png",
      "canvas": [
        80,
        88
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 30
      },
      "description": "large terracotta storage jar with a wooden lid",
      "fill": false
    },
    {
      "name": "brew_barrel",
      "out": "Assets/Art/Iso/_aday/iso_item_brew_barrel_se.png",
      "canvas": [
        80,
        88
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 30
      },
      "description": "oak fermenting barrel with a tap",
      "fill": false
    },
    {
      "name": "chandler_table",
      "out": "Assets/Art/Iso/_aday/iso_item_chandler_table_se.png",
      "canvas": [
        80,
        88
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 30
      },
      "description": "small table with hanging dipped candles and a wax pot"
    },
    {
      "name": "hunting_blind",
      "out": "Assets/Art/Iso/_aday/iso_item_hunting_blind_se.png",
      "canvas": [
        80,
        96
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 36
      },
      "description": "hunter's blind made of branches and leaves",
      "fill": false
    },
    {
      "name": "weather_vane",
      "out": "Assets/Art/Iso/_aday/iso_item_weather_vane_se.png",
      "canvas": [
        80,
        120
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 64
      },
      "description": "wooden weather vane on a pole with a rooster",
      "fill": false
    },
    {
      "name": "slate",
      "out": "Assets/Art/Iso/_aday/iso_item_slate_se.png",
      "canvas": [
        80,
        96
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 40
      },
      "description": "wooden-framed slate board on an easel",
      "fill": false
    },
    {
      "name": "lute",
      "out": "Assets/Art/Iso/_aday/iso_item_lute_se.png",
      "canvas": [
        80,
        96
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 36
      },
      "description": "Turkic long-necked lute leaning on a small stand",
      "fill": false
    }
  ]
}
```

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

- 2026-11-20 (toplu işçi): başladı

- 2026-11-20 (toplu işçi): 10/10 teslim, 11 gen, Claude'suz. Rapor: art/results/343-zanaat-istasyonlari-2.md

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
