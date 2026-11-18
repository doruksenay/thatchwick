---
id: "399a"
title: Yapı menüsü ikonları yeniden, bölüm 1/2 (21; 392-394 retleri)
status: accepted
claimed_at: 2026-11-18T14:36Z
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
  - Assets/Art/Iso/_aday/ui_icon_tab_build.png
  - Assets/Art/Iso/_aday/ui_icon_tab_items.png
  - Assets/Art/Iso/_aday/ui_icon_tab_inventory.png
  - Assets/Art/Iso/_aday/ui_icon_top_level_basement.png
  - Assets/Art/Iso/_aday/ui_icon_top_level_upper.png
  - Assets/Art/Iso/_aday/ui_icon_top_walls_up.png
  - Assets/Art/Iso/_aday/ui_icon_top_walls_cut.png
  - Assets/Art/Iso/_aday/ui_icon_top_walls_down.png
  - Assets/Art/Iso/_aday/ui_icon_top_roof.png
  - Assets/Art/Iso/_aday/ui_icon_top_eyedropper.png
  - Assets/Art/Iso/_aday/ui_icon_top_sledgehammer.png
  - Assets/Art/Iso/_aday/ui_icon_top_rooms.png
  - Assets/Art/Iso/_aday/ui_icon_top_undo.png
  - Assets/Art/Iso/_aday/ui_icon_ui_comfort.png
  - Assets/Art/Iso/_aday/ui_icon_ui_filter.png
  - Assets/Art/Iso/_aday/ui_icon_ui_lock.png
  - Assets/Art/Iso/_aday/ui_icon_tool_room.png
  - Assets/Art/Iso/_aday/ui_icon_tool_wall.png
  - Assets/Art/Iso/_aday/ui_icon_tool_erase_edge.png
  - Assets/Art/Iso/_aday/ui_icon_tool_push_wall.png
  - Assets/Art/Iso/_aday/ui_icon_tool_move_room.png
created: 2026-11-18
---
# 399a Yapı menüsü ikonları yeniden

## Amaç
392, 393 ve 394'te 42 ikon okunmadığı için teslim edilmedi (`art/392-red/`, `art/results/393-red/`, `art/results/394-red/`). Kullanıcı kararı (2026-11-18, sohbet): eksikler yeni yöntemle yeniden üretilsin. Teslim edilen 36 ikon referans alınır.

## Tarif
- **Neden reddedildiler:** soyut kavramlar (geri al, kesit, odalar, filtre, taşı/kopyala) ve uzun açıklamalar modelde yaratık ya da yanlış nesne üretti. Bu yüzden her açıklama **tek somut nesne**, kısa.
- **Araç sırası:** önce `create_map_object` (32x32, saydam, `view: side`, `outline: single color black outline`, `shading: basic shading`, `detail: medium detail`). Okunmazsa aynı ikon için ikinci deneme `create_image_pixflux` 32x32 `no_background=true`. İkon başına en çok 2 deneme (bu iş için kullanıcı izni; `max_attempts: 2`).
- **Stil referansı:** teslim edilen ikonlardan `ui_icon_bgroup_floor.png` ve `ui_icon_cat_seating.png` (_aday) `style_image` olarak, araç destekliyorsa.
- Palet açıklamaya: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey, leaf green`.
- Ortak ek: `, single game icon, one object centered, no frame, no background, no text`.
- **Renk azaltma:** teslim öncesi `python tools/art/style.py fit` ya da Pillow ile en çok 16 renge indir (yarı saydam 0). 392-394'teki teslim edilmiş 36 ikona da aynı azaltmayı uygula, ama **dosyanın üzerine yazma**: `_aday/ui_icon_*_q16.png` olarak yanına koy.
- Teslim: `Assets/Art/Iso/_aday/<ad>.png`, `cp -n` (var olanı ezme), `.meta` yazma.

### İkonlar (21)
| Dosya | Anahtar | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_tab_build | İnşa sekmesi | `a wooden carpenter's hammer` |
| ui_icon_tab_items | Eşya sekmesi | `a small wooden chair seen from the side` |
| ui_icon_tab_inventory | Envanter sekmesi | `a closed wooden treasure chest with iron bands` |
| ui_icon_top_level_basement | Kat: Bodrum | `a wooden trapdoor in the floor, half open` |
| ui_icon_top_level_upper | Kat: Üst kat | `a short wooden ladder` |
| ui_icon_top_walls_up | Duvar: Yukarı | `a tall timber-framed wall panel with white plaster` |
| ui_icon_top_walls_cut | Duvar: Kesit | `a timber-framed wall panel only half height` |
| ui_icon_top_walls_down | Duvar: Aşağı | `a single row of grey foundation stones` |
| ui_icon_top_roof | Çatı göster | `a small triangular golden straw thatched roof` |
| ui_icon_top_eyedropper | Damlalık | `a glass pipette with a red rubber bulb` |
| ui_icon_top_sledgehammer | Balyoz | `a big iron sledgehammer with a long wooden handle` |
| ui_icon_top_rooms | Odalar vurgusu | `a rolled-up paper blueprint with a ribbon` |
| ui_icon_top_undo | Geri al | `a curved brown wooden arrow pointing left` |
| ui_icon_ui_comfort | Konfor | `a plump red pillow` |
| ui_icon_ui_filter | Filtre | `a small copper funnel` |
| ui_icon_ui_lock | Kilitli | `a closed iron padlock` |
| ui_icon_tool_room | Room | `a tiny square house floor plan with brown walls` |
| ui_icon_tool_wall | Wall | `a short brick and plaster wall piece` |
| ui_icon_tool_erase_edge | EraseEdge | `a broken wall piece with cracks` |
| ui_icon_tool_push_wall | PushWall | `a wall piece with a big white arrow beside it` |
| ui_icon_tool_move_room | MoveRoom | `a small wooden house with a white arrow` |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk.
- Göz kontrolü x3, koyu ve keten zeminde: nesne tanınıyor mu, insan/hayvan/yaratık değil mi (cat_animals hariç). Okunmayan, iki denemeden sonra da okunmuyorsa teslim edilmez, raporda listelenir.
- `art/results/399a-yanyana.png`: yeni ikonlar + 392-394'te teslim edilmiş 36 ikon tek sayfada (dil birliği kontrolü).
- `style.py check --family arayuz` (UYARI raporda).

## Notlar
- Ad kuralı `ui_icon_<grup>_<anahtar>`; kod `Assets/Scripts/Game/BuildMode.cs` anahtar → dosya eşlemesini yapar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
