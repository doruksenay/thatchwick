---
id: "393"
title: Yapı menüsü ikonları: inşa araçları
status: accepted
claimed_at: 2026-11-18T13:58Z
approved_by: user
priority: 15
family: arayuz
style_gate: false
cost_est: 36
cost_max: 46
depends_on: ["210"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_tool_room.png
  - Assets/Art/Iso/_aday/ui_icon_tool_wall.png
  - Assets/Art/Iso/_aday/ui_icon_tool_halfwall.png
  - Assets/Art/Iso/_aday/ui_icon_tool_door.png
  - Assets/Art/Iso/_aday/ui_icon_tool_window.png
  - Assets/Art/Iso/_aday/ui_icon_tool_erase_edge.png
  - Assets/Art/Iso/_aday/ui_icon_tool_push_wall.png
  - Assets/Art/Iso/_aday/ui_icon_tool_move_room.png
  - Assets/Art/Iso/_aday/ui_icon_tool_copy_room.png
  - Assets/Art/Iso/_aday/ui_icon_tool_floor.png
  - Assets/Art/Iso/_aday/ui_icon_tool_floor_room.png
  - Assets/Art/Iso/_aday/ui_icon_tool_finish.png
  - Assets/Art/Iso/_aday/ui_icon_tool_finish_room.png
  - Assets/Art/Iso/_aday/ui_icon_tool_roof.png
  - Assets/Art/Iso/_aday/ui_icon_tool_grass.png
  - Assets/Art/Iso/_aday/ui_icon_tool_fence.png
  - Assets/Art/Iso/_aday/ui_icon_tool_gate.png
  - Assets/Art/Iso/_aday/ui_icon_tool_railing.png
  - Assets/Art/Iso/_aday/ui_icon_tool_pond.png
  - Assets/Art/Iso/_aday/ui_icon_tool_road_path.png
  - Assets/Art/Iso/_aday/ui_icon_tool_road_gravel.png
  - Assets/Art/Iso/_aday/ui_icon_tool_road_paved.png
  - Assets/Art/Iso/_aday/ui_icon_tool_road_erase.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_ditch.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_stone.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_flume.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_wood.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_clay.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_lead.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_erase.png
  - Assets/Art/Iso/_aday/ui_icon_tool_level_build.png
  - Assets/Art/Iso/_aday/ui_icon_tool_level_remove.png
  - Assets/Art/Iso/_aday/ui_icon_tool_plot.png
  - Assets/Art/Iso/_aday/ui_icon_tool_zone_storage.png
  - Assets/Art/Iso/_aday/ui_icon_tool_zone_pasture.png
  - Assets/Art/Iso/_aday/ui_icon_tool_zone_erase.png
created: 2026-11-18
---
# 393 Yapı menüsü ikonları: inşa araçları

## Amaç
İnşa sekmesindeki her aracın ikonu (`BuildMode.Tool` enum'u ve su yolu türleri `ConduitType`).

Kullanıcı kararı (2026-11-18, sohbet): "tek seferde yap, PixelLab kuyruğuna at". Bağlam: proje belgesi 33-menu-gorsel-dili-ve-asset-listesi; kod `Assets/Scripts/Game/BuildMode.cs`.

## Tarif
- **Yöntem (315 ve 212 dersleri):** sayfa üretimi kesilemedi ve 16x16 tek ikon okunmadı. Bu yüzden **her ikon ayrı çağrı**: `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, `seed=0`. İkon başına 1 gen.
- **Dil birliği (315 sorun 6):** hepsi **çerçevesiz**, tek nesne, 32x32 tuvalde 24-26 px, ortalı. Renk ≤ 12, 1 px koyu mürekkep (#2F2926) kontur, ışık sol-üstten.
- Palet: `"linen cream #E9DDC7, burgundy #7A2E3B, wheat gold #C89B3C, leaf green #5E7F45, hearth orange #D97A2B, ink #2F2926, wood browns, iron grey"` (marka paleti, proje belgesi 17).
- Stil referansı: varsa `Assets/Art/Iso/_aday/ui_icon_misc_coin.png` ve `ui_icon_act_craft.png` (315) `style_image` olarak; araç desteklemiyorsa yalnızca açıklamaya ek.
- Ortak açıklama eki: `, single pixel art UI icon, one object centered, no frame, no background, flat simple shading, clean dark outline, light from top-left, no text, no letters, no numbers`.
- Teslim: `Assets/Art/Iso/_aday/<ad>.png`, `cp -n`, `.meta` yazma. Var olan dosya varsa üzerine yazma, raporda yaz.

### İkonlar (36 çağrı)
| Dosya | Koddaki anahtar | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_tool_room | Room | `square room outline with walls on all sides` |
| ui_icon_tool_wall | Wall | `straight wattle and daub wall segment` |
| ui_icon_tool_halfwall | HalfWall | `low half-height stone wall` |
| ui_icon_tool_door | Door | `plank wooden door with iron hinges` |
| ui_icon_tool_window | Window | `small window with wooden shutters` |
| ui_icon_tool_erase_edge | EraseEdge | `wall segment with a red cross over it` |
| ui_icon_tool_push_wall | PushWall | `wall segment with two arrows pushing it` |
| ui_icon_tool_move_room | MoveRoom | `small room outline with a four-way arrow` |
| ui_icon_tool_copy_room | CopyRoom | `two overlapping room outlines` |
| ui_icon_tool_floor | Floor | `single wooden floor tile` |
| ui_icon_tool_floor_room | FloorRoom | `room outline filled with wooden floor` |
| ui_icon_tool_finish | Finish | `wide paint brush with lime wash` |
| ui_icon_tool_finish_room | FinishRoom | `paint brush over a small room outline` |
| ui_icon_tool_roof | Roof | `thatched roof with a straw bundle` |
| ui_icon_tool_grass | FloorErase | `tuft of green grass` |
| ui_icon_tool_fence | Fence | `wooden rail fence section` |
| ui_icon_tool_gate | Gate | `small wooden garden gate` |
| ui_icon_tool_railing | Railing | `wooden balustrade railing` |
| ui_icon_tool_pond | Pond | `small round pond with water` |
| ui_icon_tool_road_path | RoadPath | `dirt footpath` |
| ui_icon_tool_road_gravel | RoadGravel | `gravel road with small pebbles` |
| ui_icon_tool_road_paved | RoadPaved | `cobblestone paving` |
| ui_icon_tool_road_erase | RoadErase | `cobblestones with a red cross` |
| ui_icon_tool_conduit_ditch | Conduit Ditch | `earth irrigation ditch with water` |
| ui_icon_tool_conduit_stone | Conduit StoneChannel | `stone lined water channel` |
| ui_icon_tool_conduit_flume | Conduit Flume | `raised wooden water flume` |
| ui_icon_tool_conduit_wood | Conduit WoodPipe | `hollow log water pipe` |
| ui_icon_tool_conduit_clay | Conduit ClayPipe | `terracotta clay pipe segment` |
| ui_icon_tool_conduit_lead | Conduit LeadPipe | `grey lead pipe segment` |
| ui_icon_tool_conduit_erase | Conduit None | `pipe segment with a red cross` |
| ui_icon_tool_level_build | LevelBuild | `wooden floor slab with an up arrow` |
| ui_icon_tool_level_remove | LevelRemove | `wooden floor slab with a down arrow and crack` |
| ui_icon_tool_plot | Plot | `land deed scroll with a wax seal` |
| ui_icon_tool_zone_storage | ZoneStorage | `stacked crates and sacks` |
| ui_icon_tool_zone_pasture | ZonePasture | `grass field with a small cow bell` |
| ui_icon_tool_zone_erase | ZoneErase | `crates with a red cross` |

## Doğrulama
- Her ikon 32x32, yarı saydam 0, renk sayısı (≤ 12 hedef; aşanları raporda ⚠).
- Toplu önizleme x3, koyu (#333333) ve keten (#E9DDC7) zemin üstünde: `art/results/393-yanyana.png`. Aynı sayfadaki ikonlar birbirinden şekliyle ayırt ediliyor mu (renk körü dostu)?
- `style.py check --family arayuz` (UYARI raporda).
- Okunmayan ikon için yeniden deneme yok; raporda listele.

## Notlar
- Ad kuralı `ui_icon_<grup>_<anahtar>` (315 ile aynı). Kod tarafı anahtar → dosya eşlemesini yapar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
