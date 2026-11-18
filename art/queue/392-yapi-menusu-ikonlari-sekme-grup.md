---
id: "392"
title: Yapı menüsü ikonları: sekmeler, gruplar, üst şerit, durum
status: running
claimed_at: 2026-11-18T13:58Z
approved_by: user
priority: 15
family: arayuz
style_gate: false
cost_est: 28
cost_max: 36
depends_on: ["210"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_tab_build.png
  - Assets/Art/Iso/_aday/ui_icon_tab_items.png
  - Assets/Art/Iso/_aday/ui_icon_tab_inventory.png
  - Assets/Art/Iso/_aday/ui_icon_bgroup_wall.png
  - Assets/Art/Iso/_aday/ui_icon_bgroup_floor.png
  - Assets/Art/Iso/_aday/ui_icon_bgroup_garden.png
  - Assets/Art/Iso/_aday/ui_icon_bgroup_water.png
  - Assets/Art/Iso/_aday/ui_icon_bgroup_plot.png
  - Assets/Art/Iso/_aday/ui_icon_sgroup_furniture.png
  - Assets/Art/Iso/_aday/ui_icon_sgroup_household.png
  - Assets/Art/Iso/_aday/ui_icon_sgroup_decor.png
  - Assets/Art/Iso/_aday/ui_icon_sgroup_outdoor.png
  - Assets/Art/Iso/_aday/ui_icon_top_level_basement.png
  - Assets/Art/Iso/_aday/ui_icon_top_level_ground.png
  - Assets/Art/Iso/_aday/ui_icon_top_level_upper.png
  - Assets/Art/Iso/_aday/ui_icon_top_walls_up.png
  - Assets/Art/Iso/_aday/ui_icon_top_walls_cut.png
  - Assets/Art/Iso/_aday/ui_icon_top_walls_down.png
  - Assets/Art/Iso/_aday/ui_icon_top_roof.png
  - Assets/Art/Iso/_aday/ui_icon_top_eyedropper.png
  - Assets/Art/Iso/_aday/ui_icon_top_sledgehammer.png
  - Assets/Art/Iso/_aday/ui_icon_top_rooms.png
  - Assets/Art/Iso/_aday/ui_icon_top_undo.png
  - Assets/Art/Iso/_aday/ui_icon_ui_lock.png
  - Assets/Art/Iso/_aday/ui_icon_ui_new.png
  - Assets/Art/Iso/_aday/ui_icon_ui_comfort.png
  - Assets/Art/Iso/_aday/ui_icon_ui_search.png
  - Assets/Art/Iso/_aday/ui_icon_ui_filter.png
created: 2026-11-18
---
# 392 Yapı menüsü ikonları: sekmeler, gruplar, üst şerit, durum

## Amaç
Yapı modu panelinde yazılı düğmelerin yerine ikon: 3 ana sekme (İnşa/Eşya/Envanter), 5 inşa grubu, 4 eşya grubu, üst şerit (kat, duvar görünümü, çatı, damlalık, balyoz, odalar, geri al) ve durum rozetleri (kilit, yeni, konfor, arama, filtre).

Kullanıcı kararı (2026-11-18, sohbet): "tek seferde yap, PixelLab kuyruğuna at". Bağlam: proje belgesi 33-menu-gorsel-dili-ve-asset-listesi; kod `Assets/Scripts/Game/BuildMode.cs`.

## Tarif
- **Yöntem (315 ve 212 dersleri):** sayfa üretimi kesilemedi ve 16x16 tek ikon okunmadı. Bu yüzden **her ikon ayrı çağrı**: `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, `seed=0`. İkon başına 1 gen.
- **Dil birliği (315 sorun 6):** hepsi **çerçevesiz**, tek nesne, 32x32 tuvalde 24-26 px, ortalı. Renk ≤ 12, 1 px koyu mürekkep (#2F2926) kontur, ışık sol-üstten.
- Palet: `"linen cream #E9DDC7, burgundy #7A2E3B, wheat gold #C89B3C, leaf green #5E7F45, hearth orange #D97A2B, ink #2F2926, wood browns, iron grey"` (marka paleti, proje belgesi 17).
- Stil referansı: varsa `Assets/Art/Iso/_aday/ui_icon_misc_coin.png` ve `ui_icon_act_craft.png` (315) `style_image` olarak; araç desteklemiyorsa yalnızca açıklamaya ek.
- Ortak açıklama eki: `, single pixel art UI icon, one object centered, no frame, no background, flat simple shading, clean dark outline, light from top-left, no text, no letters, no numbers`.
- Teslim: `Assets/Art/Iso/_aday/<ad>.png`, `cp -n`, `.meta` yazma. Var olan dosya varsa üzerine yazma, raporda yaz.

### İkonlar (28 çağrı)
| Dosya | Koddaki anahtar | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_tab_build | İnşa sekmesi | `carpenter hammer crossed with a saw` |
| ui_icon_tab_items | Eşya sekmesi | `simple wooden chair` |
| ui_icon_tab_inventory | Envanter sekmesi | `small wooden chest with iron bands` |
| ui_icon_bgroup_wall | İnşa grubu Duvar | `short section of wattle and daub wall with a timber post` |
| ui_icon_bgroup_floor | İnşa grubu Zemin | `square of wooden floor planks seen from above` |
| ui_icon_bgroup_garden | İnşa grubu Bahçe | `short wooden picket fence with a tuft of grass` |
| ui_icon_bgroup_water | İnşa grubu Su | `wooden bucket full of blue water` |
| ui_icon_bgroup_plot | İnşa grubu Arsa | `wooden stake with a rope line marking a plot` |
| ui_icon_sgroup_furniture | Eşya grubu Mobilya | `simple wooden bed with a straw mattress` |
| ui_icon_sgroup_household | Eşya grubu Ev işi | `black iron cooking pot` |
| ui_icon_sgroup_decor | Eşya grubu Süs | `clay vase with a small flower` |
| ui_icon_sgroup_outdoor | Eşya grubu Dışarı | `small green tree beside a wooden barn` |
| ui_icon_top_level_basement | Kat: Bodrum | `stone stairs going down into darkness` |
| ui_icon_top_level_ground | Kat: Zemin | `small thatched cottage front` |
| ui_icon_top_level_upper | Kat: Üst kat | `wooden ladder going up` |
| ui_icon_top_walls_up | Duvar: Yukarı | `full wall section standing tall` |
| ui_icon_top_walls_cut | Duvar: Kesit | `wall section cut in half diagonally` |
| ui_icon_top_walls_down | Duvar: Aşağı | `wall section lowered to only a base row of stones` |
| ui_icon_top_roof | Çatı göster | `thatched roof` |
| ui_icon_top_eyedropper | Damlalık | `small glass dropper with a drop` |
| ui_icon_top_sledgehammer | Balyoz | `heavy wooden sledgehammer` |
| ui_icon_top_rooms | Odalar vurgusu | `floor plan of two rooms with colored floors` |
| ui_icon_top_undo | Geri al | `curved arrow pointing back to the left` |
| ui_icon_ui_lock | Kilitli | `small iron padlock` |
| ui_icon_ui_new | YENİ rozeti | `small gold star` |
| ui_icon_ui_comfort | Konfor | `soft cushion` |
| ui_icon_ui_search | Arama | `magnifying glass with wooden handle` |
| ui_icon_ui_filter | Filtre | `simple funnel` |

## Doğrulama
- Her ikon 32x32, yarı saydam 0, renk sayısı (≤ 12 hedef; aşanları raporda ⚠).
- Toplu önizleme x3, koyu (#333333) ve keten (#E9DDC7) zemin üstünde: `art/results/392-yanyana.png`. Aynı sayfadaki ikonlar birbirinden şekliyle ayırt ediliyor mu (renk körü dostu)?
- `style.py check --family arayuz` (UYARI raporda).
- Okunmayan ikon için yeniden deneme yok; raporda listele.

## Notlar
- Ad kuralı `ui_icon_<grup>_<anahtar>` (315 ile aynı). Kod tarafı anahtar → dosya eşlemesini yapar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
