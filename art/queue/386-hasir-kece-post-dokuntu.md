---
id: "386"
title: Hasır, keçe ve post halılar, döküntü yığını (8)
status: pending
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 10
cost_max: 14
depends_on: ["337"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_rush_mat_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_rush_mat_large_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_rush_mat_hall_3x2_se.png
  - Assets/Art/Iso/_aday/iso_item_felt_rug_small_se.png
  - Assets/Art/Iso/_aday/iso_item_felt_rug_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_felt_rug_large_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_goat_hide_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_debris_se.png
created: 2026-11-18
---
# 386 Hasır, keçe ve post halılar, döküntü yığını (8)

## Amaç
Ev dekorasyonu D1 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Yuva türü ve boyutu, duvar bandı ve tavan katmanı kodda; görsel yalnızca eşyanın kendisi.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Yöntem 337 ile aynı (düz, gölgesiz, en çok 2 px kalınlık). Keçeler `finishGroup: cloth`: ham, açık tonlu (renk motorda). İleride 9 parça yöntemi (27 §13) bu desenlerden çıkarılabilir.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.rush_mat` (Köylü) | `iso_item_rush_mat_2x1_se.png` | 2x1 | — | `woven rush floor mat, natural straw color` |
| 2 | `item.rush_mat_large` (Köylü) | `iso_item_rush_mat_large_2x2_se.png` | 2x2 | — | `large square woven rush floor mat` |
| 3 | `item.rush_mat_hall` (Köylü) | `iso_item_rush_mat_hall_3x2_se.png` | 3x2 | — | `large woven rush hall mat with a plaited border` |
| 4 | `item.felt_rug_small` (Köylü) | `iso_item_felt_rug_small_se.png` | 1x1 | — | `small undyed felt rug with a simple stitched motif` |
| 5 | `item.felt_rug` (Köylü) | `iso_item_felt_rug_2x1_se.png` | 2x1 | — | `undyed felt rug with inlaid felt motifs` |
| 6 | `item.felt_rug_large` (Zanaatkâr) | `iso_item_felt_rug_large_2x2_se.png` | 2x2 | — | `large undyed felt rug with a ram's horn motif border` |
| 7 | `item.goat_hide` (Köylü) | `iso_item_goat_hide_1x2_se.png` | 1x2 | — | `brown goat hide rug` |
| 8 | `item.debris` (—) | `iso_item_debris_se.png` | 1x1 | — | `small pile of broken shelf planks, shards of pottery and fallen objects on the floor` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
