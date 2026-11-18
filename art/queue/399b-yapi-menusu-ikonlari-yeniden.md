---
id: "399b"
title: Yapı menüsü ikonları yeniden, bölüm 2/2 (21; 392-394 retleri)
status: pending
approved_by: user
priority: 15
family: arayuz
style_gate: false
cost_est: 27
cost_max: 46
depends_on: ["392", "393", "394"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_tool_copy_room.png
  - Assets/Art/Iso/_aday/ui_icon_tool_floor.png
  - Assets/Art/Iso/_aday/ui_icon_tool_roof.png
  - Assets/Art/Iso/_aday/ui_icon_tool_fence.png
  - Assets/Art/Iso/_aday/ui_icon_tool_gate.png
  - Assets/Art/Iso/_aday/ui_icon_tool_road_path.png
  - Assets/Art/Iso/_aday/ui_icon_tool_road_gravel.png
  - Assets/Art/Iso/_aday/ui_icon_tool_road_paved.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_wood.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_clay.png
  - Assets/Art/Iso/_aday/ui_icon_tool_zone_pasture.png
  - Assets/Art/Iso/_aday/ui_icon_cat_animals.png
  - Assets/Art/Iso/_aday/ui_icon_cat_barn.png
  - Assets/Art/Iso/_aday/ui_icon_cat_decor_wall.png
  - Assets/Art/Iso/_aday/ui_icon_cat_farm.png
  - Assets/Art/Iso/_aday/ui_icon_cat_hygiene.png
  - Assets/Art/Iso/_aday/ui_icon_cat_rug.png
  - Assets/Art/Iso/_aday/ui_icon_cat_storage.png
  - Assets/Art/Iso/_aday/ui_icon_cat_structure.png
  - Assets/Art/Iso/_aday/ui_icon_cat_surfaces.png
  - Assets/Art/Iso/_aday/ui_icon_cat_work.png
created: 2026-11-18
---
# 399b Yapı menüsü ikonları yeniden

## Amaç
392, 393 ve 394'te 42 ikon okunmadığı için teslim edilmedi (`art/392-red/`, `art/results/393-red/`, `art/results/394-red/`). Kullanıcı kararı (2026-11-18, sohbet): eksikler yeni yöntemle yeniden üretilsin. Teslim edilen 36 ikon referans alınır.

## Tarif
- **Neden reddedildiler:** soyut kavramlar (geri al, kesit, odalar, filtre, taşı/kopyala) ve uzun açıklamalar modelde yaratık ya da yanlış nesne üretti. Bu yüzden her açıklama **tek somut nesne**, kısa.
- **Araç sırası:** önce `create_map_object` (32x32, saydam, `view: side`, `outline: single color black outline`, `shading: basic shading`, `detail: medium detail`). Okunmazsa aynı ikon için ikinci deneme `create_image_pixflux` 32x32 `no_background=true`. İkon başına en çok 2 deneme (bu iş için kullanıcı izni; `max_attempts: 2`).
- **Stil referansı:** teslim edilen ikonlardan `ui_icon_bgroup_floor.png` ve `ui_icon_cat_seating.png` (_aday) `style_image` olarak, araç destekliyorsa.
- Palet açıklamaya: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey, leaf green`.
- Ortak ek: `, single game icon, one object centered, no frame, no background, no text`.
- **Renk azaltma:** teslim öncesi `python tools/art/style.py fit` ya da Pillow ile en çok 16 renge indir (yarı saydam 0). (36 ikonun 16 renk kopyası 399a'da yapılır.)
- Teslim: `Assets/Art/Iso/_aday/<ad>.png`, `cp -n` (var olanı ezme), `.meta` yazma.

### İkonlar (21)
| Dosya | Anahtar | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_tool_copy_room | CopyRoom | `two identical small wooden houses side by side` |
| ui_icon_tool_floor | Floor | `a square of brown wooden planks` |
| ui_icon_tool_roof | Roof | `a bundle of golden straw tied with string` |
| ui_icon_tool_fence | Fence | `a short brown wooden fence with three posts` |
| ui_icon_tool_gate | Gate | `a small wooden gate with a latch` |
| ui_icon_tool_road_path | RoadPath | `a strip of brown dirt with footprints` |
| ui_icon_tool_road_gravel | RoadGravel | `a pile of small grey pebbles` |
| ui_icon_tool_road_paved | RoadPaved | `four grey square cobblestones` |
| ui_icon_tool_conduit_wood | Conduit WoodPipe | `a hollow wooden log with water inside` |
| ui_icon_tool_conduit_clay | Conduit ClayPipe | `an orange terracotta clay pipe` |
| ui_icon_tool_zone_pasture | ZonePasture | `a brass cow bell on a leather strap` |
| ui_icon_cat_animals | animals | `a white hen with a red comb` |
| ui_icon_cat_barn | barn | `a small red wooden barn` |
| ui_icon_cat_decor_wall | decor_wall | `a small framed painting of a landscape` |
| ui_icon_cat_farm | farm | `a wooden pitchfork` |
| ui_icon_cat_hygiene | hygiene | `a wooden bucket with soap suds` |
| ui_icon_cat_rug | rug | `a small red rectangular woven rug with fringe` |
| ui_icon_cat_storage | storage | `a woven wicker basket` |
| ui_icon_cat_structure | structure | `a short wooden staircase` |
| ui_icon_cat_surfaces | surfaces | `a small wooden table with four legs` |
| ui_icon_cat_work | work | `a wooden spinning wheel` |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk.
- Göz kontrolü x3, koyu ve keten zeminde: nesne tanınıyor mu, insan/hayvan/yaratık değil mi (cat_animals hariç). Okunmayan, iki denemeden sonra da okunmuyorsa teslim edilmez, raporda listelenir.
- `art/results/399b-yanyana.png`: yeni ikonlar + 392-394'te teslim edilmiş 36 ikon tek sayfada (dil birliği kontrolü).
- `style.py check --family arayuz` (UYARI raporda).

## Notlar
- Ad kuralı `ui_icon_<grup>_<anahtar>`; kod `Assets/Scripts/Game/BuildMode.cs` anahtar → dosya eşlemesini yapar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
