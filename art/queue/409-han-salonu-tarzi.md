---
id: "409"
title: "Han Salonu tarzı (8 eşya)"
status: review
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 18.11.2026 ("hepsini"), Project belgesi 27 §6.1 ve 27e
priority: 30
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301", "302", "306", "337"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_han_table_3x1_se.png
  - Assets/Art/Iso/_aday/iso_item_barrel_table_se.png
  - Assets/Art/Iso/_aday/iso_item_han_bench_2x1_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_tankard_rack_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_tankard_se.png
  - Assets/Art/Iso/_aday/iso_item_pewter_jug_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_notice_board_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_singer_stand_se.png
created: 2026-11-18
---
# 409 Han Salonu tarzı (8 eşya)

## Amaç
D4 tarzı **Han Salonu** (köy hanı havası): koyu meşe, fıçı, uzun masa, demir. **Alkol gösterilmez:** maşrapa boş, sürahi kapalı (+18 paketi dışında; hafıza kararı). Kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Masa ve raf üstü **301 ile aynı** (32 px tuval, pivot nesnenin tabanı), duvar eşyaları **306 ile aynı** (`sw_ne`; 2 kenarlık geniş eşya 380'deki kılavuz), yer eşyası 302 ölçeği, halı **337 ile aynı** (düz, gölgesiz). Tek yönlü 2x1 ve 3x1 görsel `_se` adıyla, uzun ekseni "\" (390 gibi). Teslim `cp -n`, `.meta` yok.
- Renkler: koyu meşe, is lekeli ahşap, dövme demir, kalay grisi.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.han_table` (Zanaatkâr) | `iso_item_han_table_3x1_se.png` | 3x1 | `long heavy dark oak trestle table, empty top` |
| 2 | `item.barrel_table` (Köylü) | `iso_item_barrel_table_se.png` | 1x1 | `upright wooden barrel used as a small table, empty top` |
| 3 | `item.han_bench` (Zanaatkâr) | `iso_item_han_bench_2x1_se.png` | 2x1 | `sturdy dark oak bench` |
| 4 | `item.tankard_rack` (Zanaatkâr) | `iso_wall_item_tankard_rack_2w_sw_ne.png` | 2 kenar | `wall rack with pegs for tankards, empty` |
| 5 | `item.tankard` (Köylü) | `iso_item_tankard_se.png` | yuva K | `empty wooden tankard with a handle` |
| 6 | `item.pewter_jug` (Tüccar) | `iso_item_pewter_jug_se.png` | yuva K | `lidded pewter jug` |
| 7 | `item.notice_board` (Köylü) | `iso_wall_item_notice_board_sw_ne.png` | 1 kenar | `wooden notice board with pinned parchment notes` |
| 8 | `item.singer_stand` (Zanaatkâr) | `iso_item_singer_stand_se.png` | 1x1 | `small raised wooden platform with a stool for a singer` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20).

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
