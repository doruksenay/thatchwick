---
id: "771"
title: "Köy meydanında hâlâ kutu çizilen eşyalar (3. deneme)"
status: pending
approved_by: user        # otomatik akış; kullanıcı 21.11.2026 (Project belgesi 66, 66b)
priority: 10
family: esya
style_gate: false
cost_est: 12
cost_max: 24
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_grain_sack_se.png
  - Assets/Art/Iso/_aday/iso_item_wicker_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_yarn_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_garment_stand_se.png
  - Assets/Art/Iso/_aday/iso_item_cloth_bolt_shelf_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_cloth_drying_line_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_rug_rush_se.png
  - Assets/Art/Iso/_aday/iso_item_glass_bottle_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_tapestry_sw_ne.png
created: 2026-11-21
---
# 771 Meydan yer tutucuları (3. deneme)

## Amaç
Tester raporu 66 §1.3: köy meydanındaki 612 eşyanın 72'si dokusuz kutu. Bu dokuzu 431/732/735/736'da renk sayısı ve palet kilidi
yüzünden teslim edilemedi (736 `review` ama çıktısı yok). Kalanların (yatak çerçevesi, kurutma askısı, bakır kaplar, yalak, sini,
halat, fıçı masa) işi 733'te sürüyor; bu iş onlara dokunmaz.

## Tarif
**492 yöntemi** (`create_map_object`, stil eşleme, kılavuz + maske `tools/art/footprint_guide.py`, `high top-down`, `low detail`,
`basic shading`, `single color outline`), 736'daki ortak ek ve renk önlemleri aynen. Bu denemenin farkı:
- **Önce `style.py fit --family esya`, sonra gerekirse `reduce_colors` (36 renk, 1 çağrı).** 736 yalnızca reduce_colors diyordu;
  kalmaların çoğu doygunluktan (x1,5-2,2) geldi, fit doygunluğu aileye indirir ve renk tonunu korur.
- Referans olarak aynı dükkânın kabul edilmiş komşuları verilir: terzide `iso_item_loom_2x1_se`, erzakta `iso_item_flour_sack_se`,
  `iso_item_barrel_se`.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.grain_sack` | `iso_item_grain_sack_se.png` | 1x1 | 80x72 | 40, 24 | `open hemp sack full of wheat grain, a wooden scoop in it` |
| 2 | `item.wicker_basket` | `iso_item_wicker_basket_se.png` | 1x1 | 80x64 | 40, 24 | `empty round wicker basket with two handles` |
| 3 | `item.yarn_basket` | `iso_item_yarn_basket_se.png` | 1x1 | 80x64 | 40, 24 | `low wicker basket with balls of undyed and madder-red wool yarn` |
| 4 | `item.garment_stand` | `iso_item_garment_stand_se.png` | 1x1 | 80x96 | 40, 24 | `simple wooden clothes stand with a plain linen tunic hanging on it` |
| 5 | `item.cloth_bolt_shelf` | `iso_item_cloth_bolt_shelf_2x1_se.png` | 2x1 | 112x104 | 56, 32 | `wooden shelf against a wall with rolled bolts of muted linen and wool cloth` |
| 6 | `item.cloth_drying_line` | `iso_item_cloth_drying_line_2x1_se_nw.png` | 2x1 | 112x96 | 56, 32 | `two wooden posts with a rope, dyed cloth strips drying on it` |
| 7 | `item.rug_rush` | `iso_item_rug_rush_se.png` | 1x1 | 64x32 düz | 32, 16 | `flat woven rush floor mat, natural straw color` (yer karosu gibi, yükseklik yok) |
| 8 | `item.glass_bottle` | `iso_item_glass_bottle_se.png` | yüzey | 32x32 | 16, 7 | `small greenish medieval glass bottle with a cork` |
| 9 | `item.tapestry` | `iso_wall_item_tapestry_sw_ne.png` | duvar | 48x64 | 24, 32 | `small woven wall tapestry with a muted floral border, hanging from a rod` |

## Doğrulama
`validate_iso.py ... --max-colors 48`, `style.py check --family esya`, `tools/art/scale_check.py`. Teslimden sonra kod oturumu
`python tools/art/art_link.py --apply` çalıştırır (eşleme ve `pending` temizliği; tester raporu 66).

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
