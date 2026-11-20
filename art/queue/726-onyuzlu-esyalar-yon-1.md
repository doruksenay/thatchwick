---
id: "726"
title: "Ön yüzlü eşyalar: yönü belli görseller, bölüm 1 (8 eşya)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("assetleri ürettir. evet bunu yap"; belge 64b, yön sorunu)
family: esya
style_gate: false
cost_est: 22
cost_max: 34
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_chair_painted_sw.png
  - Assets/Art/Iso/_aday/iso_item_throne_chair_sw.png
  - Assets/Art/Iso/_aday/iso_item_bench_wood_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_cushioned_divan_3x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_chest_iron_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_spice_chest_sw.png
  - Assets/Art/Iso/_aday/iso_item_bedside_stand_sw.png
  - Assets/Art/Iso/_aday/iso_item_bread_rack_sw.png
created: 2026-11-20
---
# 726 Ön yüzlü eşyalar: yönü belli görseller (1)

## Amaç
Belge 64b (dokuma tezgâhı sorunu): çalışma, oturma ya da açma tarafı olan eşyaların görseli hangi yöne baktığını taşımalı.
Bu eşyaların ya hiç görseli yok ya da eski adayları reddedildi/teslim edilmedi (303, 404, 410, 431, 399, 506). Veri hazır:
`items.json`'da her biri `"w": "<dosya>"`, `"mirror": true`, `"pending": true`. Dosya gelince oyun kendiliğinden çizer;
ayna N yönünü verir, E ve S (arkası dönük) bu işte yok.

## Yön şartı (bağlayıcı)
- Eşyanın **ön yüzü** (oturulan, çalışılan, açılan taraf) ekranda **sol-aşağıya** (sw) baksın. Kamera ön yüzü ve sağ yanını görür.
- 2x1 ve 3x1 eşyanın **uzun kenarı ekranda sağ-aşağıdan sol-yukarıya** uzanır (ızgara y ekseni) ve ön yüz o uzun kenardır.
  Uzun kenarı sol-aşağıdan sağ-yukarıya çizilmiş görsel reddedilir (dokuma tezgâhı hatası).
- Sol/sağ simetrik çizilsin (ayna geçerli olsun): yazı, tek yana dönük kol ya da sap yok.

## Tarif
Yöntem 431 ile aynı (`create_map_object`, stil eşleme; referanslar `iso_item_clothes_press_2x1_se`, `iso_item_pantry_shelf_2x1_se_nw`,
`iso_item_workbench_2x1_se_nw`). Ortak ek: `, front side facing the lower-left of the image, medieval European, high top-down isometric view, light from top-left, single dark outline, transparent background`
Palet ≤ 48 renk (`fit` sonrası); 404 ve 431'deki ret nedeni renk sayısıydı.

| # | Eşya | Dosya | Ayak izi | Tuval | İstem |
|---|---|---|---|---|---|
| 1 | `item.chair_painted` | iso_item_chair_painted_sw | 1x1 | 80x96 | `simple wooden chair painted dark red with a carved back` |
| 2 | `item.throne_chair` | iso_item_throne_chair_sw | 1x1 | 80x96 | `tall carved walnut high-backed chair with armrests, head of household seat` |
| 3 | `item.bench_wood` | iso_item_bench_wood_2x1_sw | 2x1 | 112x112 | `long plain wooden bench with a backrest` |
| 4 | `item.cushioned_divan` | iso_item_cushioned_divan_3x1_sw | 3x1 | 144x112 | `long low divan with patterned wool cushions and bolsters` |
| 5 | `item.chest_iron` | iso_item_chest_iron_2x1_sw | 2x1 | 112x112 | `long wooden chest bound with black iron bands and a heavy lock` |
| 6 | `item.spice_chest` | iso_item_spice_chest_sw | 1x1 | 80x96 | `small brass-bound wooden chest with many little drawers` |
| 7 | `item.bedside_stand` | iso_item_bedside_stand_sw | 1x1 | 80x96 | `small square wooden bedside stand with one drawer` |
| 8 | `item.bread_rack` | iso_item_bread_rack_sw | 1x1 | 80x96 | `tall narrow wooden bread rack with round loaves on three shelves` |

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Ek denetim: görsel, ayak izi elmasının üstünde ön yüz sol-aşağıda olacak şekilde
oturuyor mu (2x1'de uzun kenar sağ-aşağı → sol-yukarı). Sonuçta her eşya için tek satır "yön: sw, uzun kenar: y" notu.


<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
