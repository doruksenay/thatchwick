---
id: "778"
title: "Simge yapılar (619'un toplu işçiyle yeniden çalışması; 9 eşya)"
status: pending
approved_by: user        # kullanıcı 21.11 00:09 "devam et harita oluşturmaya"; 619 kararıyla aynı onay (belge 64a)
worker: batch            # 619 bulut ajanında kılavuz yükleyemediği için bloklandı (619 Sorular); toplu işçi REST ile yükler
priority: 10
family: yapi
style_gate: false
cost_est: 12
cost_max: 24
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_boundary_stone_se.png
  - Assets/Art/Iso/_aday/iso_item_lightning_oak_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_hunter_hut_ruin_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_sunken_boat_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_broken_boardwalk_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_watchtower_ruin_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_old_quarry_cleft_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_vineyard_house_ruin_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_beacon_fire_place_se.png
created: 2026-11-21
---
# 778 Simge yapılar (toplu işçi)

## Amaç
Belge 64 §3 Katman 2: her bölgede bir benzersiz yer. 619 aynı listeyle bloklandı (bulut ajanında kılavuz ve maske
yüklenemiyor); bu iş `tools/art/batch_worker.py` ile REST üzerinden çalışır. Meydan çeşmesi (619 #10) çıkarıldı: meydanda kuyu var.
Kod: `items.json` `item.landmark_*` (etiket `landmark`, `pending`), `maps/<bölge>/markers.json` items.

| # | Dosya | Ayak izi | Tuval | Açıklama |
|---|---|---|---|---|
| 1 | `iso_item_boundary_stone_se` | 1x1 | 80x112 | `tall weathered standing boundary stone with worn carved lines, lichen, small offering stones at its base` |
| 2 | `iso_item_lightning_oak_2x2_se` | 2x2 | 160x192 | `huge old oak split by lightning, half dead blackened trunk, one side still green` |
| 3 | `iso_item_hunter_hut_ruin_2x2_se` | 2x2 | 160x144 | `ruined small hunter's hut of logs, collapsed roof, antlers above the door` |
| 4 | `iso_item_sunken_boat_2x1_se_nw` | 2x1 | 112x80 | `old wooden rowing boat half sunk at a lake shore, reeds growing through` |
| 5 | `iso_item_broken_boardwalk_2x1_se_nw` | 2x1 | 112x72 | `collapsed section of an old wooden boardwalk sinking into marsh water` |
| 6 | `iso_item_watchtower_ruin_2x2_se` | 2x2 | 160x224 | `ruined round stone watchtower, only two walls standing, ivy` |
| 7 | `iso_item_old_quarry_cleft_2x2_se` | 2x2 | 160x128 | `old abandoned quarry cleft cut into grey rock, square tool marks, rubble` |
| 8 | `iso_item_vineyard_house_ruin_2x2_se` | 2x2 | 160x160 | `small abandoned stone vineyard hut, roofless, wild vine climbing its walls` |
| 9 | `iso_item_beacon_fire_place_se` | 1x1 | 80x96 | `stone ring of an old beacon fire on rocks, charred wood inside` |

## Parametreler (toplu işçi)
301 yöntemi (stil eşleme, ayak izi kılavuzu + maske). Renk sınırı 48; aile `yapi`.

```json
{
  "tool": "map_object",
  "family": "yapi",
  "max_attempts": 2,
  "common": {
    "view": "high top-down",
    "detail": "low detail",
    "shading": "basic shading",
    "outline": "single color outline",
    "suffix": ", medieval European countryside, worn and old, earthy muted palette, few colors, light from top-left, single dark outline, no ground, no shadow, transparent background"
  },
  "items": [
    {
      "name": "boundary_stone",
      "out": "Assets/Art/Iso/_aday/iso_item_boundary_stone_se.png",
      "canvas": [
        80,
        112
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 64
      },
      "description": "tall weathered standing boundary stone with worn carved lines, lichen, small offering stones at its base",
      "fill": false
    },
    {
      "name": "lightning_oak",
      "out": "Assets/Art/Iso/_aday/iso_item_lightning_oak_2x2_se.png",
      "canvas": [
        160,
        192
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 112
      },
      "description": "huge old oak split by lightning, half dead blackened trunk, one side still green",
      "fill": false
    },
    {
      "name": "hunter_hut_ruin",
      "out": "Assets/Art/Iso/_aday/iso_item_hunter_hut_ruin_2x2_se.png",
      "canvas": [
        160,
        144
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 64
      },
      "description": "ruined small hunter's hut of logs, collapsed roof, antlers above the door",
      "fill": false
    },
    {
      "name": "sunken_boat",
      "out": "Assets/Art/Iso/_aday/iso_item_sunken_boat_2x1_se_nw.png",
      "canvas": [
        112,
        80
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 24
      },
      "description": "old wooden rowing boat half sunk at a lake shore, reeds growing through",
      "fill": false
    },
    {
      "name": "broken_boardwalk",
      "out": "Assets/Art/Iso/_aday/iso_item_broken_boardwalk_2x1_se_nw.png",
      "canvas": [
        112,
        72
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 16
      },
      "description": "collapsed section of an old wooden boardwalk sinking into marsh water",
      "fill": false
    },
    {
      "name": "watchtower_ruin",
      "out": "Assets/Art/Iso/_aday/iso_item_watchtower_ruin_2x2_se.png",
      "canvas": [
        160,
        224
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 144
      },
      "description": "ruined round stone watchtower, only two walls standing, ivy",
      "fill": false
    },
    {
      "name": "old_quarry_cleft",
      "out": "Assets/Art/Iso/_aday/iso_item_old_quarry_cleft_2x2_se.png",
      "canvas": [
        160,
        128
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 48
      },
      "description": "old abandoned quarry cleft cut into grey rock, square tool marks, rubble",
      "fill": false
    },
    {
      "name": "vineyard_house_ruin",
      "out": "Assets/Art/Iso/_aday/iso_item_vineyard_house_ruin_2x2_se.png",
      "canvas": [
        160,
        160
      ],
      "guide": {
        "n": 2,
        "m": 2,
        "axis": "x",
        "height": 80
      },
      "description": "small abandoned stone vineyard hut, roofless, wild vine climbing its walls",
      "fill": false
    },
    {
      "name": "beacon_fire_place",
      "out": "Assets/Art/Iso/_aday/iso_item_beacon_fire_place_se.png",
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
      "description": "stone ring of an old beacon fire on rocks, charred wood inside",
      "fill": false
    }
  ]
}
```

## Doğrulama
`validate_iso.py`, `style.py check --family yapi`. Dönem kuralı: hayali ortaçağ, yazıt yalnızca aşınmış çizgi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
