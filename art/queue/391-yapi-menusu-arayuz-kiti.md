---
id: "391"
title: Yapı menüsü arayüz kiti (9-slice panel, düğme, sekme, hücre, rozet)
status: accepted
claimed_at: 2026-11-18T13:57Z
approved_by: user
priority: 15
family: arayuz
style_gate: false
cost_est: 30
cost_max: 40
depends_on: ["210"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_kit_panel_sheet.png
  - Assets/Art/Iso/_aday/ui_kit_button_sheet.png
  - Assets/Art/Iso/_aday/ui_kit_misc_sheet.png
  - Assets/Art/Iso/_aday/ui_kit_<parca>.png (kesilmiş parçalar, aşağıdaki liste)
created: 2026-11-18
---
# 391 Yapı menüsü arayüz kiti

## Amaç
Yapı modu paneli (`Assets/Scripts/Game/BuildMode.cs`) bugün Unity'nin varsayılan gri IMGUI görünümünde. Rakip kıyası ve karar: proje belgesi 33-menu-gorsel-dili-ve-asset-listesi. Bu iş, panelin bir GUISkin'e dönüşmesi için gereken **9-slice parçaları** üretir. Aynı kit sonra diğer IMGUI panellerinde (Pazar, Kiler, Aile, Köy) de kullanılacak.

Kullanıcı kararı (2026-11-18, sohbet): "tek seferde yap, PixelLab kuyruğuna at".

## Tarif
- **Yön:** 210'un `yon:` satırı; yoksa varsayılan `parsomen`. Raporda yazılır.
- Araç: `create_ui_asset`, üç çağrı, her biri 256x256, `no_background=true`, `max_colors=32`, `seed=0`.
- `style_image_base64` = `Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png` (210 çıktısı). Gerekirse en yakın komşuyla yarıya indir, raporda yaz.
- `color_palette` = `"linen cream #E9DDC7, burgundy #7A2E3B, wheat gold #C89B3C, ink #2F2926, leaf green #5E7F45"` (marka paleti, proje belgesi 17).
- Ortak açıklama eki: `, pixel art game UI kit, separate blank pieces with transparent gaps between them, flat simple shading, clean 1px dark ink outline, light from top-left, no dithering, no noise, no text, no letters, no numbers, every piece blank`.
- `pieces` koordinatları 512x512 sanal tuvalde (aşağıda). `elements` verme.

### Çağrı 1: panel sayfası (`ui_kit_panel_sheet`)
Açıklama: `parchment and linen cloth panel pieces with burgundy stitched border: one large blank panel, one blank inset area, one active tab and one inactive tab, three square item cells: normal, selected with a wheat gold frame, locked and darker`
```json
[
 {"id":"panel","kind":"rounded_rect","x":16,"y":16,"w":224,"h":224,"radius":16,"label":"main panel"},
 {"id":"inset","kind":"rounded_rect","x":272,"y":16,"w":224,"h":112,"radius":8,"label":"inset area"},
 {"id":"tab_on","kind":"rounded_rect","x":272,"y":160,"w":104,"h":56,"radius":8,"label":"active tab"},
 {"id":"tab_off","kind":"rounded_rect","x":392,"y":160,"w":104,"h":56,"radius":8,"label":"inactive tab"},
 {"id":"cell","kind":"rounded_rect","x":16,"y":288,"w":128,"h":128,"radius":8,"label":"item cell"},
 {"id":"cell_sel","kind":"rounded_rect","x":176,"y":288,"w":128,"h":128,"radius":8,"label":"selected item cell"},
 {"id":"cell_lock","kind":"rounded_rect","x":336,"y":288,"w":128,"h":128,"radius":8,"label":"locked item cell"}
]
```
### Çağrı 2: düğme sayfası (`ui_kit_button_sheet`)
Açıklama: `five wide blank buttons of burgundy cloth with wheat gold stitched edge, in states: normal, hovered and lighter, pressed and darker sunk, selected with a bright gold frame, disabled grey and faded`
```json
[
 {"id":"btn","kind":"rounded_rect","x":32,"y":24,"w":448,"h":72,"radius":12,"label":"button normal"},
 {"id":"btn_hover","kind":"rounded_rect","x":32,"y":120,"w":448,"h":72,"radius":12,"label":"button hovered"},
 {"id":"btn_press","kind":"rounded_rect","x":32,"y":216,"w":448,"h":72,"radius":12,"label":"button pressed"},
 {"id":"btn_sel","kind":"rounded_rect","x":32,"y":312,"w":448,"h":72,"radius":12,"label":"button selected"},
 {"id":"btn_off","kind":"rounded_rect","x":32,"y":408,"w":448,"h":72,"radius":12,"label":"button disabled"}
]
```
### Çağrı 3: çeşitli sayfa (`ui_kit_misc_sheet`)
Açıklama: `small UI pieces in the same material: a blank text input field, a blank tooltip box, a vertical scrollbar track and a scrollbar thumb, a small blank round price badge with a coin edge, a small blank ribbon badge, a small padlock badge, a small blank square close button`
```json
[
 {"id":"field","kind":"rounded_rect","x":16,"y":16,"w":320,"h":56,"radius":6,"label":"text field"},
 {"id":"tooltip","kind":"rounded_rect","x":16,"y":96,"w":320,"h":96,"radius":8,"label":"tooltip box"},
 {"id":"scroll_track","kind":"rounded_rect","x":376,"y":16,"w":32,"h":288,"radius":12,"label":"scrollbar track"},
 {"id":"scroll_thumb","kind":"rounded_rect","x":440,"y":16,"w":32,"h":96,"radius":12,"label":"scrollbar thumb"},
 {"id":"badge_price","kind":"ellipse","x":16,"y":232,"w":72,"h":72,"label":"price badge"},
 {"id":"badge_new","kind":"rounded_rect","x":112,"y":240,"w":120,"h":56,"radius":6,"label":"ribbon badge"},
 {"id":"badge_lock","kind":"rounded_rect","x":256,"y":232,"w":72,"h":72,"radius":8,"label":"padlock badge"},
 {"id":"close","kind":"rounded_rect","x":16,"y":336,"w":72,"h":72,"radius":8,"label":"close button"}
]
```
### Kesim ve teslim
- Her sayfayı saydam boşluklarından parçalara kes (kırpma, yeniden boyama yok): `ui_kit_{panel,inset,tab_on,tab_off,cell,cell_sel,cell_lock,btn,btn_hover,btn_press,btn_sel,btn_off,field,tooltip,scroll_track,scroll_thumb,badge_price,badge_new,badge_lock,close}.png`.
- Her 9-slice parçası (panel, inset, tab, cell, btn, field, tooltip, scroll) için rapora önerilen kenarlık (`border` L/R/T/B piksel) yaz: köşe süsü ve kenar dikişini kapsayan en küçük değer.
- `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.

## Doğrulama
- Sayfalar 256x256, saydam zemin, yarı saydam piksel sayısı (0 beklenir).
- 20 parça çıktı mı; her parçada yazı ya da harf benzeri şekil yok mu.
- 9-slice testi: her panel ve düğme parçasını önerilen kenarlıkla 3 kat genişletip `art/results/391-9slice-x2.png` önizlemesi üret; ortası ve kenarları tekrar ederken dikiş kopmuyor mu.
- `style.py check --family arayuz` (UYARI raporda).
- Yan yana: `art/results/391-yanyana.png`.

## Notlar
- Unity tarafı (planlama/kod oturumu): parçalar `Assets/Art/UI/` altına taşınıp GUISkin'e bağlanacak; IMGUI `GUIStyle.border` = rapordaki kenarlık.
- Bilinen bir oyunun arayüz motifi kopyalanmaz; açıklamada oyun adı geçmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
