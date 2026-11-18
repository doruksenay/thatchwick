---
id: "380"
title: Duvar dekoru: kancalı çıta, tabak rafı, uzun raf, kaşıklık, çelenk, nazarlık, tunç ayna
status: review
claimed_at: 2026-11-18T12:49Z
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_peg_rail_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_plate_rack_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_long_shelf_iron_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_spoon_rack_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_flower_wreath_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_nazar_charm_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_bronze_mirror_wall_sw_ne.png
created: 2026-11-18
---
# 380 Duvar dekoru: kancalı çıta, tabak rafı, uzun raf, kaşıklık, çelenk, nazarlık, tunç ayna

## Amaç
Ev dekorasyonu D1 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Yuva türü ve boyutu, duvar bandı ve tavan katmanı kodda; görsel yalnızca eşyanın kendisi.

## Tarif
Yöntem, ortak ayarlar, duvar kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **306 ile aynıdır** (`art/queue/306-duvar-esyasi-pilot.md` > Tarif). Hepsi `sw_ne`; öbür yüz aynayla.
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Geniş eşyalar (tabak rafı, uzun raf) iki kenar boyunca uzanır: kılavuz, 306'daki duvar yüzü paralelkenarının iki karo uzunluğudur. Pivot görselin merkezi (kod iki kenarın ortasına koyar).

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.peg_rail` (Köylü) | `iso_wall_item_peg_rail_sw_ne.png` | 1 kenar | 64x40 | `wooden peg rail: a plank with three turned wooden pegs, empty` |
| 2 | `item.plate_rack` (Zanaatkâr) | `iso_wall_item_plate_rack_2w_sw_ne.png` | 2 kenar | 96x56 | `wide wall plate rack with a front rail, empty slots for four upright plates` |
| 3 | `item.long_shelf_iron` (Tüccar) | `iso_wall_item_long_shelf_iron_2w_sw_ne.png` | 2 kenar | 96x48 | `long sturdy oak wall shelf on wrought iron brackets, empty top` |
| 4 | `item.spoon_rack` (Köylü) | `iso_wall_item_spoon_rack_sw_ne.png` | 1 kenar | 40x48 | `small carved wooden spoon rack holding wooden spoons` |
| 5 | `item.flower_wreath` (Köylü) | `iso_wall_item_flower_wreath_sw_ne.png` | 1 kenar | 40x40 | `small wreath of dried wildflowers and grasses hung on a nail` |
| 6 | `item.nazar_charm` (Köylü) | `iso_wall_item_nazar_charm_sw_ne.png` | 1 kenar | 24x40 | `small blue glass evil-eye bead charm hanging from a red cord` |
| 7 | `item.bronze_mirror_wall` (Zanaatkâr) | `iso_wall_item_bronze_mirror_wall_sw_ne.png` | 1 kenar | 40x48 | `round polished bronze hand mirror hung on the wall, warm metallic sheen, simple wooden frame` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
