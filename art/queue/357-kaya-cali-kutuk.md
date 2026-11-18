---
id: "357"
title: Arazi engelleri (kaya, çalı, devrik kütük, eski kök kütüğü)
status: running
claimed_at: 2026-11-18T12:02Z
approved_by: user        # otomatik akış; kullanıcı onayı sohbette 2026-11-18 ("yap bunu", görsel analiz partisi)
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

## Doğrulama
301 tablosu. Ek kontroller:
- Yedi nesne yan yana, 64x32 çimen karosu ve köylü (`iso_char_farmer_a2_56_cand_se`) ile tek sayfada.
- Kaya 2 köylünün dizini geçmiyor; alıç köylüden kısa, meşe yer tutucusundan küçük.
- Eski kök kütüğü yarma kütüğüyle (`iso_item_chopping_block_se`, 301) karıştırılmıyor.

## Notlar
- Dosya öneki `iso_terrain_`: eşya kataloğunda satılmaz, haritaya serpilir. Kodda karşılığı (kaz, kes, taş ve odun verimi) bu iş review'a geçtikten sonra yazılır.
- Dönem: hepsi Eski Dünya türü (böğürtlen çalısı, alıç, meşe).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
