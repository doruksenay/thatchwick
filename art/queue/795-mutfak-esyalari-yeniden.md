---
id: "795"
title: "Mutfak eşyaları yeniden: sac, bıçak, kesme tahtası, oklava tahtası, kevgir (_v2) ve ilk kez yıkama taşı, kirli ve temiz bulaşık sepeti (8)"
status: review
claimed_at: 2026-11-20T23:08Z
approved_by: user        # kullanıcı 21.11.2026: 600 "kabul + sac yeniden", 601 "kısmi kabul + kalanlar yeniden" (seçmeli liste)
priority: 5          # kullanıcı 21.11 02:02: "işçiye ürettir" (mutfak işleri önce)
family: esya
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["600", "601"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_griddle_iron_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_kitchen_knife_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_cutting_board_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_rolling_board_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_colander_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_washing_stone_se.png
  - Assets/Art/Iso/_aday/iso_item_dish_basket_dirty_se.png
  - Assets/Art/Iso/_aday/iso_item_dish_basket_clean_se.png
  - art/results/795-*.md
created: 2026-11-21
---
# 795 Mutfak eşyaları yeniden (8)

## Amaç
600 ve 601'in zayıf ya da teslim edilmemiş kalemleri (raporlar `art/results/600-*.md`, `601-*.md`). Kabul edilenler (kaşık, kürek, boş sepet, leğen, 600'ün geri kalanı) değişmez. Var olan PNG'lerin üzerine yazılmaz: yeniden üretilenler `_v2` adıyla.

## Tarif
601 ile aynı yöntem: `create_map_object` stil eşleme, `tools/art/footprint_guide.py` kılavuzu ve maskesi, aile `esya`, `high top-down`, `low detail`, `basic shading`, `single color outline`, tarife `isometric 2:1 pixel art, light from top-left, no cast shadow` eki. Masa üstü ölçüsü ve referanslar 601'deki gibi (`iso_item_copper_pot_se`, `iso_item_ladle_se`, `iso_item_wooden_bowl_se`).

| Dosya | Ayak izi | Açıklama | 600/601'deki sorun |
|---|---|---|---|
| `iso_item_griddle_iron_se_v2.png` | masa üstü (ocak üstü) | `round flat wrought iron griddle disc with a short side handle, slightly raised rim, dull grey metal with warm brown seasoning, lying flat` | soluk (x0,47), düz koyu disk; ocakta okunmuyor. Kenar ve sap seçilmeli, siyah değil koyu gri-kahve |
| `iso_item_kitchen_knife_se_v2.png` | masa üstü | `small medieval kitchen knife lying flat on its side diagonally, broad bright iron blade and a wooden handle, seen from above` | dik duruyordu; yatık ve köşegen olmalı |
| `iso_item_cutting_board_se_v2.png` | masa üstü | `thick rectangular wooden chopping board lying flat, light wood with knife marks, a round hanging hole near one end` | yatmıyor, aşırı doygun (x1,39) |
| `iso_item_rolling_board_se_v2.png` | masa üstü | `round low wooden dough board lying flat with a long thin wooden rolling pin across it and white flour dust` | oklava seçilmiyordu, turuncu dikdörtgen (x1,50); tahta yuvarlak, oklava açıkça ayrı çubuk |
| `iso_item_colander_se_v2.png` | masa üstü | `round tinned copper colander bowl with clearly visible rows of small holes in its side and two small side handles` | delik ve kulp seçilmiyordu |
| `iso_item_washing_stone_se.png` | 1x1 (yere, düz) | 601'deki açıklama | 601'de teslim edilmedi (`601-red/`) |
| `iso_item_dish_basket_dirty_se.png` | 1x1 (küçük) | 601'deki açıklama | teslim edilmedi |
| `iso_item_dish_basket_clean_se.png` | 1x1 (küçük) | 601'deki açıklama | teslim edilmedi |

Sepetin kirli ve temiz hâlleri kabul edilen boş sepetle (`iso_item_dish_basket_empty_se.png`) aynı sepettir: aynı boyut ve pivot. Boş sepet arka plan, yalnızca sepetin içi maske olarak kullanılabilir (578'in iç-maske yöntemi); tutmazsa ayrı üretilir ve rapora yazılır.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...` (KALDI teslim edilmez). Ek ölçüt: masa üstü aletler 48 px pencerede tanınmalı (yan yana sayfasında ilk hâliyle birlikte gösterilir: `art/results/795-yanyana.png`).

## Notlar
Kod eşlemesi (eşya kimliği → görsel) kabulden sonra kod oturumunda; `_v2` kabul edilirse eşleme `_v2`'yi gösterir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
