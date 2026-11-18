---
id: "403"
title: "Hatıralar, soy ağacı işlemesi, çeyiz sandığı (7 eşya)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 27 §7.7, §8.2, §8.4 (18.11.2026)
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301", "306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_festival_cup_se.png
  - Assets/Art/Iso/_aday/iso_item_harvest_ribbon_se.png
  - Assets/Art/Iso/_aday/iso_item_baby_first_shoes_se.png
  - Assets/Art/Iso/_aday/iso_item_elder_tin_cup_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_first_fish_scale_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_family_tree_embroidery_3w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_dowry_chest_2x1_se.png
created: 2026-11-18
---
# 403 Hatıralar, soy ağacı işlemesi, çeyiz sandığı

## Amaç
Ev dekorasyonu D3b (Project belgesi `27-ev-dekorasyonu` §8.4): ilerleme dekora dönüşür. Hatıralar satın alınmaz, kazanılır (şenlik, birinci sınıf hasat, ilk balık, doğum, yaşlının ölümü). Soy ağacı işlemesi nesille dallanır; çeyiz sandığı evlenen kızla gider. Kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Masa ve raf üstü hatıralar **301 ile aynı** (küçük tuval, 382'deki masa üstü ölçeği); duvar eşyaları **306 ile aynı** (`sw_ne`; 3 kenarlık geniş eşya 380'deki geniş eşya kılavuzu); çeyiz sandığı 2x1 mobilya (302 ölçeği). Teslim `cp -n`, `.meta` yok.
- Soy ağacı işlemesi **marka diliyle aynı** (kanaviçe, Project belgesi `17`): keten zemin, kökboya kırmızısı ve çivit mavisiyle işlenmiş ağaç; dallarda küçük düğüm noktaları (kişiler). Oyunda nesil büyüdükçe güzellik artar; görsel tek kare yeterli (ileride nesil sayısına göre 2-3 kare düşünülebilir).
- Çeyiz sandığı **Yörük Odası** tarzı: boyalı ahşap, kırmızı-yeşil çiçek motifleri, demir köşebent.

### Eşyalar
| # | items.json id | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.festival_cup` | `iso_item_festival_cup_se.png` | yuva K | `small pewter festival cup with two handles and a ribbon tied round the stem` |
| 2 | `item.harvest_ribbon` | `iso_item_harvest_ribbon_se.png` | yuva K | `red rosette ribbon with a small sheaf of wheat tied to it, lying flat` |
| 3 | `item.baby_first_shoes` | `iso_item_baby_first_shoes_se.png` | yuva K | `pair of tiny soft leather baby shoes tied together with string` |
| 4 | `item.elder_tin_cup` | `iso_item_elder_tin_cup_se.png` | yuva K | `old dented tin drinking cup, worn and polished by use` |
| 5 | `item.first_fish_scale` | `iso_wall_item_first_fish_scale_sw_ne.png` | 1 kenar | `small wooden plaque with a dried fish mounted on it` |
| 6 | `item.family_tree_embroidery` | `iso_wall_item_family_tree_embroidery_3w_sw_ne.png` | 3 kenar | `long linen cross-stitch embroidery of a branching family tree in red and indigo thread` |
| 7 | `item.dowry_chest` | `iso_item_dowry_chest_2x1_se.png` | 2x1 | `painted wooden dowry chest with red and green floral motifs and iron corner straps, lid closed` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem: kalay ve kurşun-kalay alaşımı kupa, deri bebek ayakkabısı ve kanaviçe ortaçağda bilinir.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
