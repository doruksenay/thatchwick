---
id: "353"
title: Su işleri eşyaları: çeşme, yalak-lüle, akarsulu hela, sarnıç, yağmur fıçısı, su terazisi
status: review
claimed_at: 2026-11-18T12:08Z
approved_by: user        # otomatik akış (2026-11-17); su şebekesi PR (feat/su-sebekesi)
worker: batch            # 2026-11-18 toplu işçi pilotu (Claude'suz); bulut ajanı almaz
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_fountain_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_kitchen_spout_se.png
  - Assets/Art/Iso/_aday/iso_item_flush_privy_se.png
  - Assets/Art/Iso/_aday/iso_item_cistern_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_rain_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_water_tower_se.png
created: 2026-11-17
---
# 353 Su işleri eşyaları

## Amaç
Su şebekesi tesisatı (Project belgesi 20). Boruya bağlı **çeşme** köyün ortak suyu (kova buradan dolar), mutfakta **taş yalak ve lüle**, içeride **akarsulu hela** (manastır helası gibi: su gelirken yıkanır), yağmur ve ağ fazlasını biriktiren **sarnıç** ve **yağmur fıçısı**, üst kata basınç veren **su terazisi**.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- çeşme (2x1) = medieval stone wall fountain with a carved stone basin and a small bronze spout pouring water into a trough;
- taş yalak ve lüle (1x1) = small carved stone sink with a clay spout on a short wall pipe, kitchen use;
- akarsulu hela (1x1) = wooden privy seat box with a lid, a small wooden water trough running behind it (monastery latrine);
- sarnıç (2x2) = low square stone cistern with a wooden lid and a hand winch;
- yağmur fıçısı (1x1) = wooden barrel with iron hoops under a short wooden gutter spout;
- su terazisi (1x1, uzun) = slender square stone pressure tower about two storeys tall with a small lead tank on top (Ottoman water balance tower), narrow base.

## Parametreler (toplu işçi)
Tarifin makine okunur hâli (`tools/art/batch_worker.py`). Tablodaki değerlerle aynıdır; tuval ve yükseklik 301/357 oranlarıyla Claude tarafından eklendi.

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
      "name": "fountain",
      "out": "Assets/Art/Iso/_aday/iso_item_fountain_2x1_se.png",
      "canvas": [
        112,
        104
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 40
      },
      "description": "medieval stone wall fountain with a carved stone basin and a small bronze spout pouring water into a trough"
    },
    {
      "name": "kitchen_spout",
      "out": "Assets/Art/Iso/_aday/iso_item_kitchen_spout_se.png",
      "canvas": [
        80,
        72
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 24
      },
      "description": "small carved stone sink with a clay spout on a short wall pipe, kitchen use"
    },
    {
      "name": "flush_privy",
      "out": "Assets/Art/Iso/_aday/iso_item_flush_privy_se.png",
      "canvas": [
        80,
        80
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 28
      },
      "description": "wooden privy seat box with a lid, a small wooden water trough running behind it (monastery latrine)"
    },
    {
      "name": "cistern",
      "out": "Assets/Art/Iso/_aday/iso_item_cistern_2x2_se.png",
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
      "description": "low square stone cistern with a wooden lid and a hand winch"
    },
    {
      "name": "rain_barrel",
      "out": "Assets/Art/Iso/_aday/iso_item_rain_barrel_se.png",
      "canvas": [
        80,
        80
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 32
      },
      "description": "wooden barrel with iron hoops under a short wooden gutter spout"
    },
    {
      "name": "water_tower",
      "out": "Assets/Art/Iso/_aday/iso_item_water_tower_se.png",
      "canvas": [
        80,
        136
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 88
      },
      "description": "slender square stone pressure tower about two storeys tall with a small lead tank on top (Ottoman water balance tower), narrow base"
    }
  ]
}
```

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri). Su terazisi 1x1 tabanından taşmamalı, yüksekliği iki kat kadar.

## Sorular
- 2026-11-18 (bulut, şerit 2): `get_balance` plan olarak **Tier 2: Pixel Artisan**, 4999/5000 generation (yenilenme 2026-12-18) döndürdü. `art/budget.md` planı Tier 1 / 2000 gen / yenilenme 2026-12-08 olarak yazıyor ve işçi kuralı "Tier 1 değilse dur". Hiçbir PixelLab çağrısı yapılmadı, harcama 0. Kullanıcı `art/budget.md` "Plan" bölümünü (ve rezerv/günlük tavan geçerliliğini) güncelleyince iş `pending`e alınıp yeniden çalıştırılabilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-18 (tamir): `art/budget.md` planı Tier 2 olarak güncellendi, ajan kuralı "Tier 1 değilse dur" yerine "budget.md planıyla uyuşmuyorsa dur" oldu; iş yeniden `pending`.
