---
id: "345"
title: Lazımlık, yıkanma kulübesi, havluluk
status: accepted
approved_by: user        # 2026-11-18 kullanıcı: toplu işçi mobilya pilotu ("geç")
worker: batch            # Claude'suz toplu işçi; bulut ajanı almaz
priority: 20
family: esya
style_gate: false
cost_est: 6
cost_max: 9
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_chamber_pot_se.png
  - Assets/Art/Iso/_aday/iso_item_bath_hut_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_towel_rack_se.png
created: 2026-11-17
---
# 345 Tuvalet ve banyo eşyaları

## Amaç
Yeni Tuvalet ihtiyacı ve banyo için su taşıma. Lazımlık yatak odasında durur (geceleri dışarı çıkmadan tuvalet), dolunca boşaltılır, gübre verir. Yıkanma kulübesi bahçede durur; karakter önce kuyudan, göletten ya da dereden bir kova su getirir, sonra içinde yıkanır. Havluluk ıslak karakteri kurular. Dış hela (`item.outhouse`) 304'te var, yeni görsel gerekmez.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- lazımlık (1x1, kil, `finishGroup: clay`) = small glazed clay chamber pot with a lid, on the floor;
- yıkanma kulübesi (1x2, ahşap, dış mekân) = small wooden bathing shed with a plank door ajar, a wooden tub visible inside, a water bucket by the door;
- havluluk (1x1, ahşap) = simple wooden towel rack with a hanging linen towel.

## Parametreler (toplu işçi)
Tarifin makine okunur hâli (`tools/art/batch_worker.py`); tuval ve yükseklik 301 oranlarıyla Claude tarafından eklendi.

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
      "name": "chamber_pot",
      "out": "Assets/Art/Iso/_aday/iso_item_chamber_pot_se.png",
      "canvas": [
        80,
        64
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 18
      },
      "description": "small glazed brown clay chamber pot with a lid, standing on the floor"
    },
    {
      "name": "bath_hut",
      "out": "Assets/Art/Iso/_aday/iso_item_bath_hut_1x2_se.png",
      "canvas": [
        112,
        128
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "y",
        "height": 64
      },
      "description": "small wooden bathing shed with a plank door ajar, a wooden tub visible inside, a water bucket by the door"
    },
    {
      "name": "towel_rack",
      "out": "Assets/Art/Iso/_aday/iso_item_towel_rack_se.png",
      "canvas": [
        80,
        88
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 40
      },
      "description": "simple wooden towel rack on two feet with a hanging undyed linen towel",
      "fill": false
    }
  ]
}
```

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri). Kulübe 1x2: ayak izi iki karo.

- 2026-11-18 (toplu işçi): başladı

- 2026-11-18 (toplu işçi): 2/3 teslim, 8 gen, Claude'suz. Rapor: art/results/345-tuvalet-banyo-esyalari.md

- 2026-11-18 (toplu işçi): başladı

- 2026-11-18 (toplu işçi): 1/1 teslim, 1 gen, Claude'suz. Rapor: art/results/345-tuvalet-banyo-esyalari.md

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
