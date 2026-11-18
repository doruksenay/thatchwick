---
id: "390"
title: Yörük Odası mobilyası ve halılar (11)
status: pending
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 13
cost_max: 20
depends_on: ["301", "302", "337"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_sedir_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_sedir_long_3x1_se.png
  - Assets/Art/Iso/_aday/iso_item_floor_cushion_se.png
  - Assets/Art/Iso/_aday/iso_item_sini_table_se.png
  - Assets/Art/Iso/_aday/iso_item_yukluk_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_floor_bed_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_sedir_bed_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_kilim_small_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_kilim_large_3x2_se.png
  - Assets/Art/Iso/_aday/iso_item_cicim_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_herb_rush_mat_2x2_se.png
created: 2026-11-18
---
# 390 Yörük Odası mobilyası ve halılar (11)

## Amaç
Ev dekorasyonu D2 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026): tarz ekseni (Çatkı Evi, Yörük Odası), doldurulan dekor, masa ve sedir örtüsü, pencere içi ve perde. Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- Halılar (8-11) **337 ile aynı** (düz, gölgesiz, en çok 2 px kalınlık). Kilim ve cicim `finishGroup: cloth`: desen çizilir, renk motorda kök boya varyantlarıyla değişebilir.
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Tarz ipuçları: **Çatkı Evi** meşe, demir kulp, saz, badana (Kuzey Avrupa köyü); **Yörük Odası** sedir, minder, kilim, boyalı ahşap, bakır (Anadolu). Kök boyalar: kökboya kırmızısı, çivit mavisi, cehri sarısı, doğal yün.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.sedir` (Zanaatkâr) | `iso_item_sedir_2x1_se.png` | 2x1 | — | `low wooden divan (sedir) along the wall with a flat cushion, empty top` |
| 2 | `item.sedir_long` (Zanaatkâr) | `iso_item_sedir_long_3x1_se.png` | 3x1 | — | `long low wooden divan (sedir) with a flat cushion, empty top` |
| 3 | `item.floor_cushion` (Köylü) | `iso_item_floor_cushion_se.png` | 1x1 | — | `square floor cushion (minder) with a woven cover` |
| 4 | `item.sini_table` (Köylü) | `iso_item_sini_table_se.png` | 1x1 | — | `low folding wooden stand with a round brass tray top (sini)` |
| 5 | `item.yukluk` (Zanaatkâr) | `iso_item_yukluk_2x1_se.png` | 2x1 uzun | — | `tall built-in wooden cupboard (yüklük) with carved doors for bedding` |
| 6 | `item.floor_bed` (Köylü) | `iso_item_floor_bed_2x1_se.png` | 2x1 | — | `floor mattress with a thick quilt and a pillow, no bed frame` |
| 7 | `item.sedir_bed` (Zanaatkâr) | `iso_item_sedir_bed_2x1_se.png` | 2x1 | — | `low wooden day-bed divan with a quilt and bolster cushions` |
| 8 | `item.kilim_small` (Zanaatkâr) | `iso_item_kilim_small_2x1_se.png` | 2x1 | — | `flat-woven kilim rug with red and indigo diamond motifs` |
| 9 | `item.kilim_large` (Zanaatkâr) | `iso_item_kilim_large_3x2_se.png` | 3x2 | — | `large flat-woven kilim rug with stepped geometric medallions` |
| 10 | `item.cicim` (Zanaatkâr) | `iso_item_cicim_2x2_se.png` | 2x2 | — | `square cicim rug with small raised woven motifs on a red ground` |
| 11 | `item.herb_rush_mat` (Köylü) | `iso_item_herb_rush_mat_2x2_se.png` | 2x2 | — | `square woven rush mat with sprigs of dried lavender and rosemary worked in` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
