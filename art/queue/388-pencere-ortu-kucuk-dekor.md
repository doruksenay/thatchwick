---
id: "388"
title: Pencere içi, perdeler, örtüler ve küçük Yörük/Çatkı süsleri (13)
status: accepted
claimed_at: 2026-11-18T13:22Z
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 13
cost_max: 20
depends_on: ["306", "330", "301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_window_sill_board_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_linen_curtain_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_wool_curtain_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_kilim_curtain_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_harvest_wreath_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_embroidery_panel_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_tablecloth_linen_se.png
  - Assets/Art/Iso/_aday/iso_item_tablecloth_embroidered_se.png
  - Assets/Art/Iso/_aday/iso_item_table_carpet_se.png
  - Assets/Art/Iso/_aday/iso_item_sedir_cover_kilim_se.png
  - Assets/Art/Iso/_aday/iso_item_heybe_se.png
  - Assets/Art/Iso/_aday/iso_item_big_cushion_se.png
  - Assets/Art/Iso/_aday/iso_item_copper_ewer_se.png
created: 2026-11-18
---
# 388 Pencere içi, perdeler, örtüler ve küçük Yörük/Çatkı süsleri (13)

## Amaç
Ev dekorasyonu D2 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026): tarz ekseni (Çatkı Evi, Yörük Odası), doldurulan dekor, masa ve sedir örtüsü, pencere içi ve perde. Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp.

## Tarif
Duvar ve pencere eşyaları (1-6) **306 ile aynı** (`sw_ne`, öbür yüz aynayla); pencere eşyaları 330'daki pencere açıklığının ölçüsüne göre çizilir (alt bant pencere içi, orta bant perde). Diğerleri **301 ile aynı**.
- Örtüler (7-10) düz, gölgesiz, en çok 2 px kalınlık (337 halı yöntemi); kod eşyanın üstündeki her karoya serer, motor ölçekler.
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Tarz ipuçları: **Çatkı Evi** meşe, demir kulp, saz, badana (Kuzey Avrupa köyü); **Yörük Odası** sedir, minder, kilim, boyalı ahşap, bakır (Anadolu). Kök boyalar: kökboya kırmızısı, çivit mavisi, cehri sarısı, doğal yün.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.window_sill_board` (Köylü) | `iso_wall_item_window_sill_board_sw_ne.png` | pencere alt | 48x24 | `narrow plain wooden sill board fixed under a window, empty top` |
| 2 | `item.linen_curtain` (Köylü) | `iso_wall_item_linen_curtain_sw_ne.png` | pencere orta | 48x56 | `plain undyed linen curtain on a wooden rod, drawn to one side of a small window` |
| 3 | `item.wool_curtain` (Zanaatkâr) | `iso_wall_item_wool_curtain_sw_ne.png` | pencere orta | 48x56 | `heavy woven wool curtain on an iron rod, drawn to both sides` |
| 4 | `item.kilim_curtain` (Zanaatkâr) | `iso_wall_item_kilim_curtain_sw_ne.png` | pencere orta | 48x56 | `flat-woven kilim curtain with red and indigo geometric bands` |
| 5 | `item.harvest_wreath` (Köylü) | `iso_wall_item_harvest_wreath_sw_ne.png` | 1 kenar | 40x40 | `wreath of wheat ears and barley tied with red cord` |
| 6 | `item.embroidery_panel` (Zanaatkâr) | `iso_wall_item_embroidery_panel_sw_ne.png` | 1 kenar | 40x48 | `small framed cross-stitch embroidery panel with red and indigo floral motifs` |
| 7 | `item.tablecloth_linen` (Köylü) | `iso_item_tablecloth_linen_se.png` | örtü 2x1 | — | `flat undyed linen tablecloth seen from above, slight folds` |
| 8 | `item.tablecloth_embroidered` (Zanaatkâr) | `iso_item_tablecloth_embroidered_se.png` | örtü 2x1 | — | `flat linen tablecloth with a red embroidered border, seen from above` |
| 9 | `item.table_carpet` (Tüccar) | `iso_item_table_carpet_se.png` | örtü 2x1 | — | `small knotted Anatolian carpet laid over a table, geometric medallions, fringed edges` |
| 10 | `item.sedir_cover_kilim` (Zanaatkâr) | `iso_item_sedir_cover_kilim_se.png` | örtü 2x1 | — | `flat kilim cover with red and yellow geometric bands, seen from above` |
| 11 | `item.heybe` (Köylü) | `iso_item_heybe_se.png` | kanca O | 24x32 | `woven saddlebag (heybe) with kilim pattern hanging by its strap` |
| 12 | `item.big_cushion` (Köylü) | `iso_item_big_cushion_se.png` | minder O | 32x24 | `large plump floor cushion with a kilim-woven cover` |
| 13 | `item.copper_ewer` (Zanaatkâr) | `iso_item_copper_ewer_se.png` | yuva K | 20x32 | `hammered copper ewer with a long curved spout` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
