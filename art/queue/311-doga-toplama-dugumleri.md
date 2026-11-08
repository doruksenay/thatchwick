---
id: "311"
title: Toplama düğümleri (çalı, mantar, fındık, ot, yabani kovan, kuru dal)
status: pending
approved_by: pending
cost_est: 6
cost_max: 12
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_berry_bush_se.png
  - Assets/Art/Iso/_aday/iso_forage_mushroom_se.png
  - Assets/Art/Iso/_aday/iso_forage_hazel_se.png
  - Assets/Art/Iso/_aday/iso_forage_herbs_se.png
  - Assets/Art/Iso/_aday/iso_forage_hive_se.png
  - Assets/Art/Iso/_aday/iso_forage_dry_branch_se.png
created: 2026-11-08
---
# 311 Toplama düğümleri

## Amaç
`ForageSystem` her sabah mevsime göre köy kenarına `item.forage_*` düğümleri koyar (`forageSeasons`, `forageMax`). Görselleri yok. Hepsi 1x1, topla etkileşimiyle kalkar; dünyada küçük ama fark edilir olmalı (köylüden küçük, ağaçtan çok küçük).

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

- Tuval 64x56, ayak izi 1x1 (`--n 1`), yükseklik 14-30. Taban rengi sonra silinir; zemin sprite'ın parçası değildir ("no ground").
- Ortak ek: 301'deki ekten "dark brown wood" çıkarılır, "muted natural colours" eklenir.
- Açıklamalar:
  1. berry_bush (ilkbahar-yaz, yükseklik 28): `small round green bush with clusters of red berries, bright enough to notice`
  2. mushroom (sonbahar, 14): `small cluster of three brown-capped forest mushrooms with cream stems`
  3. hazel (sonbahar, 30): `small hazel shrub with a few hanging nut clusters, orange-yellow autumn leaves`
  4. herbs (ilkbahar-yaz, 18): `patch of wild green medicinal herbs with small purple flowers`
  5. hive (yaz, 26): `wild beehive: hollow log section with a ceramic-coloured honeycomb cap, a few small bees, no person`
  6. dry_branch (her mevsim, 12): `a fallen dry brown tree branch with a few twigs lying on the ground`
- Maliyet 6 × 1 gen; ikinci deneme payı 6 → `cost_max` 12.

## Doğrulama
301 tablosu. Ek: 6 düğüm yan yana ve 64x32 çimen karosu ile; her biri 1 karoyu aşmıyor mu, hiçbiri ağaçtan büyük değil mi, çimen üstünde okunuyor mu.

## Notlar
- Kış düğümü yok (`forageSeasons`'ta Winter bulunmaz, kuru dal hariç).
