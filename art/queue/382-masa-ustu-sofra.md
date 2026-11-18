---
id: "382"
title: Masa ve raf üstü: sofra takımı ve kaplar (10)
status: running
claimed_at: 2026-11-18T12:50Z
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
  - Assets/Art/Iso/_aday/iso_item_wooden_plate_se.png
  - Assets/Art/Iso/_aday/iso_item_clay_plate_se.png
  - Assets/Art/Iso/_aday/iso_item_tin_plate_se.png
  - Assets/Art/Iso/_aday/iso_item_serving_tray_se.png
  - Assets/Art/Iso/_aday/iso_item_wooden_bowl_se.png
  - Assets/Art/Iso/_aday/iso_item_ladle_se.png
  - Assets/Art/Iso/_aday/iso_item_mortar_pestle_se.png
  - Assets/Art/Iso/_aday/iso_item_oil_jug_se.png
  - Assets/Art/Iso/_aday/iso_item_copper_pot_se.png
  - Assets/Art/Iso/_aday/iso_item_storage_pot_small_se.png
created: 2026-11-18
---
# 382 Masa ve raf üstü: sofra takımı ve kaplar (10)

## Amaç
Ev dekorasyonu D1 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Yuva türü ve boyutu, duvar bandı ve tavan katmanı kodda; görsel yalnızca eşyanın kendisi.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Küçük süsler 16-32 px tuvalde; bir çağrıda 6-8 eşyalık sayfa üretilebilir (27 §13). Masa üstü ölçeği: 300'deki tahta masanın üst yüzü.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.wooden_plate` (Köylü) | `iso_item_wooden_plate_se.png` | yuva K | 24x16 | `turned wooden plate` |
| 2 | `item.clay_plate` (Köylü) | `iso_item_clay_plate_se.png` | yuva K | 24x16 | `glazed earthenware plate, ochre` |
| 3 | `item.tin_plate` (Tüccar) | `iso_item_tin_plate_se.png` | yuva K | 24x16 | `tinned copper plate with a rim, bright` |
| 4 | `item.serving_tray` (Zanaatkâr) | `iso_item_serving_tray_se.png` | yuva O | 32x20 | `round brass serving tray (sini)` |
| 5 | `item.wooden_bowl` (Köylü) | `iso_item_wooden_bowl_se.png` | yuva K | 24x20 | `small turned wooden bowl` |
| 6 | `item.ladle` (Köylü) | `iso_item_ladle_se.png` | kanca K | 16x32 | `wooden ladle hanging by its handle` |
| 7 | `item.mortar_pestle` (Köylü) | `iso_item_mortar_pestle_se.png` | yuva O | 24x28 | `stone mortar with a wooden pestle` |
| 8 | `item.oil_jug` (Köylü) | `iso_item_oil_jug_se.png` | yuva O | 24x32 | `earthenware oil jug with a handle` |
| 9 | `item.copper_pot` (Tüccar) | `iso_item_copper_pot_se.png` | yuva O | 32x28 | `polished copper cooking pot with a handle` |
| 10 | `item.storage_pot_small` (Köylü) | `iso_item_storage_pot_small_se.png` | yuva B | 32x40 | `small earthenware storage jar with a wooden lid` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
