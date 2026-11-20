---
id: "381"
title: Duvar örtüleri: boyalı bez, aplike örtü, kilim örtü (2 karo)
status: accepted
claimed_at: 2026-11-18T12:50Z
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 6
cost_max: 10
depends_on: ["306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_painted_cloth_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_applique_hanging_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_kilim_hanging_2w_sw_ne.png
created: 2026-11-18
---
# 381 Duvar örtüleri: boyalı bez, aplike örtü, kilim örtü (2 karo)

## Amaç
Ev dekorasyonu D1 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Yuva türü ve boyutu, duvar bandı ve tavan katmanı kodda; görsel yalnızca eşyanın kendisi.

## Tarif
Yöntem, ortak ayarlar, duvar kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **306 ile aynıdır** (`art/queue/306-duvar-esyasi-pilot.md` > Tarif). Hepsi `sw_ne`; öbür yüz aynayla.
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Kumaşlar `finishGroup: cloth`: boya varyantları motorda (ileride palet takası). Desenler ortaçağ ve Anadolu düz dokuma esinli; yazı ve arma yok.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.painted_cloth` (Köylü) | `iso_wall_item_painted_cloth_2w_sw_ne.png` | 2 kenar | 96x64 | `painted linen wall cloth hung from a rod, simple painted scene of a stag in a forest, muted earth colors` |
| 2 | `item.applique_hanging` (Zanaatkâr) | `iso_wall_item_applique_hanging_2w_sw_ne.png` | 2 kenar | 96x64 | `appliqué wall hanging: cut wool cloth shapes stitched on a dark blue ground, medieval figures and a border` |
| 3 | `item.kilim_hanging` (Zanaatkâr) | `iso_wall_item_kilim_hanging_2w_sw_ne.png` | 2 kenar | 96x64 | `flat-woven kilim hung on the wall, geometric diamond pattern in madder red, woad blue and undyed wool` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
