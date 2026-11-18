---
id: "389"
title: Çatkı Evi mobilyası (9)
status: review
claimed_at: 2026-11-18T13:22Z
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["301", "302"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_settle_bench_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_chest_bench_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_trestle_table_3x1_se.png
  - Assets/Art/Iso/_aday/iso_item_aumbry_se.png
  - Assets/Art/Iso/_aday/iso_item_milking_stool_se.png
  - Assets/Art/Iso/_aday/iso_item_x_chair_se.png
  - Assets/Art/Iso/_aday/iso_item_walnut_table_3x1_se.png
  - Assets/Art/Iso/_aday/iso_item_clothes_press_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_bed_end_chest_2x1_se.png
created: 2026-11-18
---
# 389 Çatkı Evi mobilyası (9)

## Amaç
Ev dekorasyonu D2 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026): tarz ekseni (Çatkı Evi, Yörük Odası), doldurulan dekor, masa ve sedir örtüsü, pencere içi ve perde. Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Tarz ipuçları: **Çatkı Evi** meşe, demir kulp, saz, badana (Kuzey Avrupa köyü); **Yörük Odası** sedir, minder, kilim, boyalı ahşap, bakır (Anadolu). Kök boyalar: kökboya kırmızısı, çivit mavisi, cehri sarısı, doğal yün.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.settle_bench` (Zanaatkâr) | `iso_item_settle_bench_2x1_se.png` | 2x1 uzun | — | `high-backed oak settle bench with solid side panels` |
| 2 | `item.chest_bench` (Köylü) | `iso_item_chest_bench_2x1_se.png` | 2x1 | — | `plain pine chest with a hinged lid that doubles as a bench, low backrest` |
| 3 | `item.trestle_table` (Köylü) | `iso_item_trestle_table_3x1_se.png` | 3x1 | — | `long plank table on two trestle legs, rough pine` |
| 4 | `item.aumbry` (Zanaatkâr) | `iso_item_aumbry_se.png` | 1x1 uzun | — | `tall oak food cupboard with pierced ventilation holes in its door` |
| 5 | `item.milking_stool` (Köylü) | `iso_item_milking_stool_se.png` | 1x1 | — | `small three-legged wooden milking stool` |
| 6 | `item.x_chair` (Asil) | `iso_item_x_chair_se.png` | 1x1 | — | `folding X-frame chair of walnut with a leather seat` |
| 7 | `item.walnut_table` (Asil) | `iso_item_walnut_table_3x1_se.png` | 3x1 | — | `long carved walnut table with turned legs` |
| 8 | `item.clothes_press` (Tüccar) | `iso_item_clothes_press_2x1_se.png` | 2x1 uzun | — | `tall painted wooden clothes press with two doors and brass fittings` |
| 9 | `item.bed_end_chest` (Zanaatkâr) | `iso_item_bed_end_chest_2x1_se.png` | 2x1 | — | `low oak chest with iron straps, for the foot of a bed` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
