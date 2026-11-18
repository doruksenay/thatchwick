---
id: "344"
title: Yayık, taş yontma tezgâhı, tuğla fırını, boya kazanı ve 7 düzenek
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (tarifler, 2026-11-17); "user" yapınca işçi alır
worker: batch            # 2026-11-18: toplu işçi biçimine çevrildi (Claude'suz); onay hâlâ kullanıcıda
priority: 20
family: esya
style_gate: false
cost_est: 18
cost_max: 24
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_churn_se.png
  - Assets/Art/Iso/_aday/iso_item_stonecutter_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_kiln_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_dye_vat_se.png
  - Assets/Art/Iso/_aday/iso_item_windmill_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_water_gate_se.png
  - Assets/Art/Iso/_aday/iso_item_noria_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_feed_mixer_se.png
  - Assets/Art/Iso/_aday/iso_item_harvest_rake_se.png
  - Assets/Art/Iso/_aday/iso_item_seeder_se.png
  - Assets/Art/Iso/_aday/iso_item_ice_well_2x2_se.png
created: 2026-11-17
---
# 344 Tarif istasyonları ve düzenekler

## Amaç
Süt ürünleri (yayık), zanaat malzemeleri (taş yontma tezgâhı, tuğla fırını, boya kazanı) ve tarifle yapılan düzenekler: yel değirmeni (suya gerek olmadan güç), su kapağı (kanalı aç/kapa), sulama dolabı (su kaynağı, çevresini sular), yem karıştırıcı, hasat tırmığı (olgun ekini biçer), ekim makinesi (tohum eker), buz kuyusu (donda buz toplar). Ahşap olanlar ham, açık tonlu.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- yayık = tall wooden butter churn with a plunger; taş yontma tezgâhı (2x1) = stone mason's bench with chisels and a half-cut block; tuğla fırını (2x2) = small brick kiln with arched opening and stacked bricks; boya kazanı = copper dye vat with colored cloth; yel değirmeni (2x2) = small wooden post windmill with cloth sails; su kapağı = wooden sluice gate in a short stone channel; sulama dolabı (2x2) = wooden water wheel noria lifting buckets from a pit; yem karıştırıcı = wooden hopper with a crank; hasat tırmığı = iron harvest rake frame with gears; ekim makinesi = wooden seed drill box with iron coulters; buz kuyusu (2x2) = round stone ice pit with a thatched cover.

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
      "name": "churn",
      "out": "Assets/Art/Iso/_aday/iso_item_churn_se.png",
      "canvas": [
        80,
        96
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 34
      },
      "description": "tall wooden butter churn with a plunger",
      "fill": false
    },
    {
      "name": "stonecutter",
      "out": "Assets/Art/Iso/_aday/iso_item_stonecutter_2x1_se.png",
      "canvas": [
        112,
        104
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 28
      },
      "description": "stone mason's bench with chisels and a half-cut block"
    },
    {
      "name": "kiln",
      "out": "Assets/Art/Iso/_aday/iso_item_kiln_2x2_se.png",
      "canvas": [
        144,
        136
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 48
      },
      "description": "small brick kiln with arched opening and stacked bricks"
    },
    {
      "name": "dye_vat",
      "out": "Assets/Art/Iso/_aday/iso_item_dye_vat_se.png",
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
      "description": "copper dye vat with colored cloth",
      "fill": false
    },
    {
      "name": "windmill",
      "out": "Assets/Art/Iso/_aday/iso_item_windmill_2x2_se.png",
      "canvas": [
        144,
        184
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 96
      },
      "description": "small wooden post windmill with cloth sails"
    },
    {
      "name": "water_gate",
      "out": "Assets/Art/Iso/_aday/iso_item_water_gate_se.png",
      "canvas": [
        80,
        80
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 24
      },
      "description": "wooden sluice gate in a short stone channel"
    },
    {
      "name": "noria",
      "out": "Assets/Art/Iso/_aday/iso_item_noria_2x2_se.png",
      "canvas": [
        144,
        144
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 56
      },
      "description": "wooden water wheel noria lifting buckets from a pit"
    },
    {
      "name": "feed_mixer",
      "out": "Assets/Art/Iso/_aday/iso_item_feed_mixer_se.png",
      "canvas": [
        80,
        96
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 34
      },
      "description": "wooden hopper with a crank"
    },
    {
      "name": "harvest_rake",
      "out": "Assets/Art/Iso/_aday/iso_item_harvest_rake_se.png",
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
      "description": "iron harvest rake frame with gears",
      "fill": false
    },
    {
      "name": "seeder",
      "out": "Assets/Art/Iso/_aday/iso_item_seeder_se.png",
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
      "description": "wooden seed drill box with iron coulters"
    },
    {
      "name": "ice_well",
      "out": "Assets/Art/Iso/_aday/iso_item_ice_well_2x2_se.png",
      "canvas": [
        144,
        120
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 28
      },
      "description": "round stone ice pit with a thatched cover",
      "fill": false
    }
  ]
}
```

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
