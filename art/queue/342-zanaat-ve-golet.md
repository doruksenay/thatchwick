---
id: "342"
title: Gölet, balık sepeti, çıkrık, dokuma tezgâhı, imbik, iksir rafı
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (yeni beceriler, 2026-11-17); "user" yapınca işçi alır
worker: batch            # 2026-11-18: toplu işçi biçimine çevrildi (Claude'suz); onay hâlâ kullanıcıda
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 14
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_pond_3x2_se.png
  - Assets/Art/Iso/_aday/iso_item_fish_trap_se.png
  - Assets/Art/Iso/_aday/iso_item_spinning_wheel_se.png
  - Assets/Art/Iso/_aday/iso_item_loom_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_alembic_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_potion_shelf_se.png
created: 2026-11-17
---
# 342 Yeni becerilerin eşyaları

## Amaç
Balıkçılık (gölet, balık sepeti), dokumacılık (çıkrık, dokuma tezgâhı) ve otacılık (imbik, iksir rafı) istasyonları. Gölet aynı zamanda sulama kanalına su verir. Ahşap olanlar ham, açık tonlu (`finishGroup: wood`).

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- gölet (3x2) = small natural pond with reeds and stones at the edge, clear water, flush with the ground; balık sepeti (1x1) = woven willow fish trap basket half in water with a stake; çıkrık (1x1) = wooden spinning wheel with a bit of wool; dokuma tezgâhı (2x1) = wooden floor loom with half-woven cloth; imbik (2x1) = copper alembic still on a small brick stove with glass bottles; iksir rafı (1x1) = small wooden shelf with colored potion bottles and dried herbs.

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
      "name": "pond",
      "out": "Assets/Art/Iso/_aday/iso_item_pond_3x2_se.png",
      "canvas": [
        176,
        112
      ],
      "guide": {
        "n": 3,
        "m": 2,
        "axis": "x",
        "height": 6
      },
      "description": "small natural pond with reeds and stones at the edge, clear water, flush with the ground",
      "fill": false
    },
    {
      "name": "fish_trap",
      "out": "Assets/Art/Iso/_aday/iso_item_fish_trap_se.png",
      "canvas": [
        80,
        80
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 22
      },
      "description": "woven willow fish trap basket half in water with a stake",
      "fill": false
    },
    {
      "name": "spinning_wheel",
      "out": "Assets/Art/Iso/_aday/iso_item_spinning_wheel_se.png",
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
      "description": "wooden spinning wheel with a bit of wool",
      "fill": false
    },
    {
      "name": "loom",
      "out": "Assets/Art/Iso/_aday/iso_item_loom_2x1_se.png",
      "canvas": [
        112,
        120
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 44
      },
      "description": "wooden floor loom with half-woven cloth"
    },
    {
      "name": "alembic",
      "out": "Assets/Art/Iso/_aday/iso_item_alembic_2x1_se.png",
      "canvas": [
        112,
        112
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 36
      },
      "description": "copper alembic still on a small brick stove with glass bottles"
    },
    {
      "name": "potion_shelf",
      "out": "Assets/Art/Iso/_aday/iso_item_potion_shelf_se.png",
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
      "description": "small wooden shelf with colored potion bottles and dried herbs"
    }
  ]
}
```

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
