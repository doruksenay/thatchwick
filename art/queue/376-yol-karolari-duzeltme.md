---
id: "376"
title: Yol karoları düzeltme (patika b, çakıl, sağlam ve bozuk kaldırım)
status: review
claimed_at: 2026-11-18T13:17Z
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 ("yap bunu": yol karoları düzeltmesi)
family: zemin
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["340"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_road_path_b_v2.png
  - Assets/Art/Iso/iso_road_gravel_a_v2.png
  - Assets/Art/Iso/iso_road_gravel_worn.png
  - Assets/Art/Iso/iso_road_paved_a.png
  - Assets/Art/Iso/iso_road_paved_broken_v2.png
created: 2026-11-18
---
# 376 Yol karoları düzeltme

## Amaç
340'ın karoları oyunda denendi (Unity, 18.11.2026, `Assets/Screenshots/gorsel-parti2-yollar.png`). Sorunlar:
- `iso_road_path_b`: kenarında çim şeritleri var. Yan yana döşenince yol boyunca çim çizgileri ve dikiş görünüyor.
- `iso_road_gravel_a`: neredeyse düz bej, çakıl olarak okunmuyor.
- `iso_road_paved_broken`: koyu kenarlı; ızgara çizgisi görünüyor (340 raporu: dikiş oranı 0,86).
- Sağlam kaldırım (`paved_a`) ve yıpranmış çakıl (`gravel_worn`) 340'ta teslim edilmemişti.

Var olan PNG'nin üzerine yazılmaz. Yenileri `_v2` ekiyle gelir, kod tarafı review sonrası adları değiştirir.

## Tarif
- Yöntem **340 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları ve 340'tan iyi çıkan `iso_road_path_a`, `iso_road_paved_worn`. 64x32 elmas, gölgesiz. Çağrıdan önce maliyeti yaz.
- **Bütün karolar:** yolun malzemesi elmasın kenarına kadar devam eder. Kenarda çim, koyu çerçeve ya da kontur yok; yan yana döşenince tek yüzey gibi okunmalı.

| Dosya | Açıklama metni |
|---|---|
| `iso_road_path_b_v2.png` | `trodden bare earth footpath, variation of the first path tile, faint footprints, edges are bare earth too, no grass` |
| `iso_road_gravel_a_v2.png` | `compacted gravel road made of many small grey and tan pebbles with visible individual stones, no border` |
| `iso_road_gravel_worn.png` | `worn gravel road with ruts, scattered pebbles and a few muddy potholes, no border` |
| `iso_road_paved_a.png` | `intact medieval cobblestone road of rounded grey stones with thin dark joints, even surface, no border` |
| `iso_road_paved_broken_v2.png` | `broken medieval cobblestones with missing stones, mud and a small puddle, same light grey tone as the intact cobbles, no dark edge` |

- Maliyet 340 ile orantılı (5 karo): `cost_est` 12, `cost_max` 18. Tek deneme.

## Doğrulama
- 340 doğrulaması (`validate_iso.py --kind ground`, `style.py check --family zemin`).
- Her karo için 8x8 döşeme testi ve dikiş parlaklık oranı. Hedef 0,95-1,05.
- 3x3 yol + çim kenarı testi: yol karosu çimle yan yana.
- Yeni ve eski yan yana: `path_a` ile `path_b_v2`, `gravel_a` ile `gravel_a_v2`, `paved_worn`, `paved_a`, `paved_broken_v2`.

## Notlar
- Pivot elmas merkezi (32,16).
- Kod tarafı (`IsoArt.Road`): sağlam kaldırım `paved_a`, yıpranmış çakıl `gravel_worn` ve v2 adları review sonrası.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
