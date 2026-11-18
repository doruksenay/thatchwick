---
id: "357"
title: Arazi engelleri (kaya, çalı, devrik kütük, eski kök kütüğü)
status: review
approved_by: user        # otomatik akış; kullanıcı onayı sohbette 2026-11-18 ("yap bunu", görsel analiz partisi)
worker: batch            # 2026-11-18 toplu işçi pilotu (Claude'suz); bulut ajanı almaz
family: esya
style_gate: false
cost_est: 7
cost_max: 14
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_terrain_boulder_small_se.png
  - Assets/Art/Iso/_aday/iso_terrain_boulder_se.png
  - Assets/Art/Iso/_aday/iso_terrain_boulder_large_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_terrain_bramble_se.png
  - Assets/Art/Iso/_aday/iso_terrain_hawthorn_se.png
  - Assets/Art/Iso/_aday/iso_terrain_fallen_log_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_terrain_old_stump_se.png
created: 2026-11-18
---
# 357 Arazi engelleri

## Amaç
Görsel analizde (Project `26-gorsel-analiz.md`) haritadaki koyu mavi-gri lekelerin yol bulma testinden kalan engeller olduğu çıktı; kod partisinde kaldırıldılar. Engeller dünyada gerçek nesne olarak dönecek: kazılıp taş veren kayalar (madencilik 53.1 yüzey taşıyla uyumlu), kesilip odun ya da çalı çırpı veren çalılar ve kütükler. Köy kenarına ve ormana serpiştirilir, yürümeyi engeller, haritayı çeşitlendirir.

## Tarif
Yöntem **301 ile aynı** (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu). Bu dosya yalnızca farkları verir. Kabul edilmiş eşyalar (tabure, ocak, yatak, masa) renk ve ölçek referansıdır.

- Ortak ekte "dark brown wood" yerine "muted natural colours, grey-brown stone, dull green foliage" yazılır.
- Hepsi SE yönünde tek görsel; kayalar ve çalılar simetrik sayılır, ayna gerekmez.

| # | Gelecek id | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni (+ ortak ek) |
|---|---|---|---|---|---|---|---|
| 1 | `item.boulder_small` | `iso_terrain_boulder_small_se.png` | 1x1 | 64x56 | `--n 1` | 12 | `small rounded grey field stone half sunk in the ground, a few lichen spots` |
| 2 | `item.boulder` | `iso_terrain_boulder_se.png` | 1x1 | 80x72 | `--n 1` | 24 | `large grey granite boulder with cracks and green-grey lichen, knee high to a villager` |
| 3 | `item.boulder_large` | `iso_terrain_boulder_large_2x1_se_nw.png` | 2x1 | 112x96 | `--n 2 --axis x` | 32 | `long low outcrop of two weathered grey rocks fused together, moss in the cracks` |
| 4 | `item.bramble` | `iso_terrain_bramble_se.png` | 1x1 | 72x64 | `--n 1` | 24 | `tangled thorny bramble bush with arching dark green canes, no berries, no flowers` |
| 5 | `item.hawthorn` | `iso_terrain_hawthorn_se.png` | 1x1 | 72x80 | `--n 1` | 34 | `dense round hawthorn shrub with small dark green leaves and twisted grey stems, taller than a stool, smaller than a tree` |
| 6 | `item.fallen_log` | `iso_terrain_fallen_log_2x1_se_nw.png` | 2x1 | 112x80 | `--n 2 --axis x` | 16 | `fallen oak log lying on its side with bark, moss patches and a broken branch stub, no axe` |
| 7 | `item.old_stump` | `iso_terrain_old_stump_se.png` | 1x1 | 64x56 | `--n 1` | 14 | `old weathered tree stump with spreading roots, moss and a small bracket fungus; clearly different from a chopping block (no axe, no flat clean top)` |

- Maliyet 7 × 1 gen; ikinci deneme payı 7 → `cost_max` 14.

## Parametreler (toplu işçi)
Tarifin makine okunur hâli (`tools/art/batch_worker.py`). Tablodaki değerlerle aynıdır; farklar: 64 px genişlikli tuvaller 80 px (64 px tuval, kenar boşluklu taban elmasını kırpıyordu) ve çalı ile alıç stil denetiminde `bitki` ailesiyle ölçülür.

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
    "suffix": ", medieval peasant village object, earthy muted palette, muted natural colours, grey-brown stone, dull green foliage, flat simple shading, single dark outline, light from top-left, no ground, no shadow, no extra objects, transparent background"
  },
  "items": [
    {
      "name": "boulder_small",
      "out": "Assets/Art/Iso/_aday/iso_terrain_boulder_small_se.png",
      "canvas": [
        80,
        56
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 12
      },
      "description": "small rounded grey field stone half sunk in the ground, a few lichen spots"
    },
    {
      "name": "boulder",
      "out": "Assets/Art/Iso/_aday/iso_terrain_boulder_se.png",
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
      "description": "large grey granite boulder with cracks and green-grey lichen, knee high to a villager"
    },
    {
      "name": "boulder_large",
      "out": "Assets/Art/Iso/_aday/iso_terrain_boulder_large_2x1_se_nw.png",
      "canvas": [
        112,
        96
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 32
      },
      "description": "long low outcrop of two weathered grey rocks fused together, moss in the cracks"
    },
    {
      "name": "bramble",
      "out": "Assets/Art/Iso/_aday/iso_terrain_bramble_se.png",
      "canvas": [
        72,
        64
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 24
      },
      "description": "tangled thorny bramble bush with arching dark green canes, no berries, no flowers",
      "family": "bitki"
    },
    {
      "name": "hawthorn",
      "out": "Assets/Art/Iso/_aday/iso_terrain_hawthorn_se.png",
      "canvas": [
        72,
        80
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 34
      },
      "description": "dense round hawthorn shrub with small dark green leaves and twisted grey stems, taller than a stool, smaller than a tree",
      "family": "bitki"
    },
    {
      "name": "fallen_log",
      "out": "Assets/Art/Iso/_aday/iso_terrain_fallen_log_2x1_se_nw.png",
      "canvas": [
        112,
        80
      ],
      "guide": {
        "n": 2,
        "m": 1,
        "axis": "x",
        "height": 16
      },
      "description": "fallen oak log lying on its side with bark, moss patches and a broken branch stub, no axe"
    },
    {
      "name": "old_stump",
      "out": "Assets/Art/Iso/_aday/iso_terrain_old_stump_se.png",
      "canvas": [
        80,
        56
      ],
      "guide": {
        "n": 1,
        "m": 1,
        "axis": "x",
        "height": 14
      },
      "description": "old weathered tree stump with spreading roots, moss and a small bracket fungus; clearly different from a chopping block (no axe, no flat clean top)"
    }
  ]
}
```

## Doğrulama
301 tablosu. Ek kontroller:
- Yedi nesne yan yana, 64x32 çimen karosu ve köylü (`iso_char_farmer_a2_56_cand_se`) ile tek sayfada.
- Kaya 2 köylünün dizini geçmiyor; alıç köylüden kısa, meşe yer tutucusundan küçük.
- Eski kök kütüğü yarma kütüğüyle (`iso_item_chopping_block_se`, 301) karıştırılmıyor.

## Notlar
- Dosya öneki `iso_terrain_`: eşya kataloğunda satılmaz, haritaya serpilir. Kodda karşılığı (kaz, kes, taş ve odun verimi) bu iş review'a geçtikten sonra yazılır.
- Dönem: hepsi Eski Dünya türü (böğürtlen çalısı, alıç, meşe).

## Sorular
- 2026-11-18: `get_balance` Tier 2 (Pixel Artisan, 4999/5000 gen, yenilenme 2026-12-18) döndü; `art/budget.md` planı Tier 1 (2000 gen, yenilenme 2026-12-08) yazıyor. Kural: Tier 1 değilse işçi durur. Paket yükseltilmiş görünüyor; kullanıcı `art/budget.md` plan/bakiye/rezerv bölümünü güncelleyince iş `pending`e alınabilir. Üretim yapılmadı, harcama 0.

- 2026-11-18 (toplu işçi): başladı

- 2026-11-18 (toplu işçi): 2/7 teslim, 10 gen, Claude'suz. Rapor: art/results/357-kaya-cali-kutuk.md

- 2026-11-18 (toplu işçi): başladı

- 2026-11-18 (toplu işçi): 1/5 teslim, 10 gen, Claude'suz. Rapor: art/results/357-kaya-cali-kutuk.md

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-18 (tamir): `art/budget.md` planı Tier 2 olarak güncellendi, ajan kuralı "Tier 1 değilse dur" yerine "budget.md planıyla uyuşmuyorsa dur" oldu; iş yeniden `pending`.
