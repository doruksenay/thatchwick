---
id: "468b"
title: "Karakter yaratma ekranı ikonları (12)"
status: pending
approved_by: user
family: arayuz
style_gate: false
cost_est: 14
cost_max: 26
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_cc_<anahtar>.png (12 adet)
  - art/results/468b-yaratma-ekrani-ikonlari.md
created: 2026-11-20
---
# 468b Karakter yaratma ekranı ikonları

## Amaç
Belge 43 §5.3: yeni yaratma ekranının sekme ve düğme ikonları. Var olanlar yeniden kullanılır: kilit `ui_icon_ui_lock`, geri al `ui_icon_top_undo`.

## Tarif
407y yöntemi aynen:
- `create_map_object` 32x32, saydam, `view: side`, `single color outline`, `basic shading`, `medium detail`.
- Okunmazsa ikinci deneme `create_image_pixflux` 32x32 `no_background=true`.
- Teslim öncesi 16 renge indir.
- Ortak ek: `, single game icon, one object centered, no frame, no background, no text`.
- Palet: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey`.

| Dosya | Açıklama (+ ortak ek) | Kullanım |
|---|---|---|
| ui_icon_cc_person | `a small carved wooden figurine of a villager` | Kişi sekmesi |
| ui_icon_cc_face | `a simple face mask of pale linen with two eye holes` | Yüz sekmesi |
| ui_icon_cc_hair | `a bone hair comb with a lock of brown hair` | Saç sekmesi |
| ui_icon_cc_clothes | `a folded linen tunic with a red trim` | Kıyafet sekmesi |
| ui_icon_cc_adorn | `a small iron earring and a bead necklace` | Süs sekmesi |
| ui_icon_cc_voice | `a small wooden bell with sound lines` | Ses sekmesi |
| ui_icon_cc_family | `two clasped hands, one larger one smaller` | Aile / hane şeridi |
| ui_icon_cc_dice | `a single carved bone die showing five pips` | Zar |
| ui_icon_cc_redo | `a thick curved wooden arrow bending to the right` | İleri al (top_undo'nun eşi) |
| ui_icon_cc_portrait | `a small wooden oval frame with a painted face` | Portre ⇄ Dünya |
| ui_icon_cc_elder | `a gnarled wooden walking stick` | Yaşlı göster |
| ui_icon_cc_night | `a crescent moon over a small candle` | Gece ışığı |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk.
- Göz kontrolü: x3 büyütmede koyu ve keten zeminde okunuyor.
- `art/results/468b-yanyana.png`: 12 ikon + `ui_icon_ui_lock`, `ui_icon_top_undo`.
- Kabul edilenler `Assets/Resources/UI/Icons/` altına kopyalanır (oyun oradan yükler).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
