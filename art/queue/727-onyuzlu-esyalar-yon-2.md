---
id: "727"
title: "Ön yüzlü eşyalar: yönü belli görseller, bölüm 2 (8 eşya)"
status: review
claimed_at: 2026-11-20T18:14Z
approved_by: user        # kullanıcı onayı 2026-11-20 ("assetleri ürettir. evet bunu yap"; belge 64b, yön sorunu)
family: esya
style_gate: false
cost_est: 30
cost_max: 42
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_fireplace_large_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_kneading_trough_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_cutting_table_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_cloth_bolt_shelf_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_sewing_table_sw.png
  - Assets/Art/Iso/_aday/iso_item_alembic_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_glass_bench_2x1_sw.png
  - Assets/Art/Iso/_aday/iso_item_glass_furnace_2x2_sw.png
created: 2026-11-20
---
# 727 Ön yüzlü eşyalar: yönü belli görseller (2)

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
| 1 | `item.fireplace_large` | iso_item_fireplace_large_2x1_sw | 2x1 | 112x112 | `large stone fireplace with a wide hearth opening, unlit, iron pot hook` |
| 2 | `item.kneading_trough` | iso_item_kneading_trough_2x1_sw | 2x1 | 112x112 | `long wooden dough kneading trough on four legs with its lid propped open` |
| 3 | `item.cutting_table` | iso_item_cutting_table_2x1_sw | 2x1 | 112x112 | `long wooden tailor's cutting table with folded cloth, iron shears and chalk` |
| 4 | `item.cloth_bolt_shelf` | iso_item_cloth_bolt_shelf_2x1_sw | 2x1 | 112x112 | `wooden shelf stacked with bolts of dyed wool and linen cloth` |
| 5 | `item.sewing_table` | iso_item_sewing_table_sw | 1x1 | 80x96 | `small wooden sewing table with a pincushion, thread spools and scissors` |
| 6 | `item.alembic` | iso_item_alembic_2x1_sw | 2x1 | 112x112 | `copper distilling alembic on a brick firebox with a cooling tub beside it` |
| 7 | `item.glass_bench` | iso_item_glass_bench_2x1_sw | 2x1 | 112x112 | `glassblower's workbench with blowpipes, tongs and a marver stone` |
| 8 | `item.glass_furnace` | iso_item_glass_furnace_2x2_sw | 2x2 | 160x160 | `domed brick glass furnace with a glowing working hole, outdoor` |

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Ek denetim: görsel, ayak izi elmasının üstünde ön yüz sol-aşağıda olacak şekilde
oturuyor mu (2x1'de uzun kenar sağ-aşağı → sol-yukarı). Sonuçta her eşya için tek satır "yön: sw, uzun kenar: y" notu.


<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
