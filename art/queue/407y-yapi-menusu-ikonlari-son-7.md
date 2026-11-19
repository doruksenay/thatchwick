---
id: "407y"
title: Yapı menüsü ikonları, son 7 (zayıf 5 + eksik 2)
status: accepted
claimed_at: 2026-11-19T03:24Z
approved_by: user
priority: 15
family: arayuz
style_gate: false
cost_est: 9
cost_max: 15
depends_on: ["399a", "399b"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_top_undo.png
  - Assets/Art/Iso/_aday/ui_icon_tool_erase_edge.png
  - Assets/Art/Iso/_aday/ui_icon_tool_move_room.png
  - Assets/Art/Iso/_aday/ui_icon_ui_filter.png
  - Assets/Art/Iso/_aday/ui_icon_top_eyedropper.png
  - Assets/Art/Iso/_aday/ui_icon_tool_gate.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_clay.png
created: 2026-11-18
---
# 407y Yapı menüsü ikonları, son 7

## Amaç
399a raporunda zayıf okunan 5 ikon (undo, erase_edge, move_room, filter, eyedropper) ve 399b'de iki denemede de üretilemeyen 2 ikon (tool_gate, tool_conduit_clay). Kullanıcı kararı (2026-11-18, sohbet): "yap bunları".

**Üzerine yazma izni:** zayıf 5 ikonun mevcut dosyaları bu iş için ezilebilir (kullanıcı kararı). Eskileri `art/results/407y-eski/` altına taşı. Ayrıca `Assets/Resources/UI/Icons/` altındaki aynı adlı kopyaları da yeni dosyayla değiştir (oyun oradan yükler; adlar aynı).

## Tarif
399a/399b yöntemi: `create_map_object` 32x32, saydam, `view: side`, `single color outline`, `basic shading`, `medium detail`; okunmazsa ikinci deneme `create_image_pixflux` 32x32 `no_background=true`. Teslim öncesi 16 renge indir (yarı saydam 0). Ortak ek: `, single game icon, one object centered, no frame, no background, no text`. Palet: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey`.

| Dosya | Açıklama (+ ortak ek) | Neden |
|---|---|---|
| ui_icon_top_undo | `a thick curved arrow made of wood, bending back to the left, bold simple shape` | ok belirsizdi |
| ui_icon_tool_erase_edge | `a wooden mallet hitting a cracked wall brick, pieces flying` | yıkım okunmuyordu |
| ui_icon_tool_move_room | `a small wooden house on a wheeled cart` | taşıma okunmuyordu |
| ui_icon_ui_filter | `a wooden sieve with a round mesh, seen at an angle` | huni belirsizdi; elek dönemle uyumlu |
| ui_icon_top_eyedropper | `a small paint brush dipped in red paint with one drop` | pipet dönem dışı ve küçüktü |
| ui_icon_tool_gate | `two wooden posts with a single swinging plank bar between them` | 399b önerisi |
| ui_icon_tool_conduit_clay | `a short straight orange clay tube, open at both ends, lying horizontally` | 399b önerisi |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk.
- Göz kontrolü x3 koyu ve keten zeminde; okunmayan teslim edilmez.
- `art/results/407y-yanyana.png`: yeni 7 + 399a/b'den 10 komşu ikon.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
