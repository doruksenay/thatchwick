---
id: "384"
title: Masa ve raf üstü: oyunlar, oymalar, kutu, bez bebek, el aynası (10)
status: accepted
claimed_at: 2026-11-18T12:55Z
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_wood_carving_se.png
  - Assets/Art/Iso/_aday/iso_item_carved_bird_se.png
  - Assets/Art/Iso/_aday/iso_item_lockbox_se.png
  - Assets/Art/Iso/_aday/iso_item_tally_stick_se.png
  - Assets/Art/Iso/_aday/iso_item_dice_knucklebones_se.png
  - Assets/Art/Iso/_aday/iso_item_chess_set_se.png
  - Assets/Art/Iso/_aday/iso_item_backgammon_se.png
  - Assets/Art/Iso/_aday/iso_item_rag_doll_se.png
  - Assets/Art/Iso/_aday/iso_item_wooden_whistle_se.png
  - Assets/Art/Iso/_aday/iso_item_copper_hand_mirror_se.png
created: 2026-11-18
---
# 384 Masa ve raf üstü: oyunlar, oymalar, kutu, bez bebek, el aynası (10)

## Amaç
Ev dekorasyonu D1 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Yuva türü ve boyutu, duvar bandı ve tavan katmanı kodda; görsel yalnızca eşyanın kendisi.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Ayna (el aynası) metal: cilalı bakır, cam değil. Kararmış hâli motorda boyanır.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.wood_carving` (Zanaatkâr) | `iso_item_wood_carving_se.png` | yuva K | 16x28 | `small carved wooden figure of a horse` |
| 2 | `item.carved_bird` (Köylü) | `iso_item_carved_bird_se.png` | yuva K | 16x20 | `small carved wooden bird` |
| 3 | `item.lockbox` (Tüccar) | `iso_item_lockbox_se.png` | yuva K | 24x20 | `small iron-bound wooden lockbox with a lock` |
| 4 | `item.tally_stick` (Köylü) | `iso_item_tally_stick_se.png` | yuva K | 24x12 | `notched wooden tally stick` |
| 5 | `item.dice_knucklebones` (Köylü) | `iso_item_dice_knucklebones_se.png` | yuva K | 20x12 | `two bone dice and a few knucklebones` |
| 6 | `item.chess_set` (Tüccar) | `iso_item_chess_set_se.png` | yuva O | 32x24 | `wooden chessboard with carved pieces` |
| 7 | `item.backgammon` (Zanaatkâr) | `iso_item_backgammon_se.png` | yuva O | 32x20 | `open wooden backgammon (tables) board with round counters` |
| 8 | `item.rag_doll` (Köylü) | `iso_item_rag_doll_se.png` | yuva K | 16x24 | `simple cloth rag doll` |
| 9 | `item.wooden_whistle` (Köylü) | `iso_item_wooden_whistle_se.png` | yuva K | 16x12 | `small wooden whistle` |
| 10 | `item.copper_hand_mirror` (Köylü) | `iso_item_copper_hand_mirror_se.png` | yuva K | 16x24 | `round polished copper hand mirror on a small stand` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
