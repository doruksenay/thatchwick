---
id: "400"
title: "Cam eşyalar, aynalar, kutu yatak ve cam ürün ikonları (8 eşya + 7 ikon)"
status: review
claimed_at: 2026-11-18T18:40Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 27 §18 (18.11.2026)
priority: 20
family: esya
style_gate: false
cost_est: 15
cost_max: 22
depends_on: ["301", "306", "316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_glass_bottle_se.png
  - Assets/Art/Iso/_aday/iso_item_glass_cup_se.png
  - Assets/Art/Iso/_aday/iso_item_glass_jar_se.png
  - Assets/Art/Iso/_aday/iso_item_glass_goblet_se.png
  - Assets/Art/Iso/_aday/iso_item_hourglass_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_convex_mirror_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_large_mirror_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_box_bed_2x1_se.png
  - Assets/Art/Icons/icon_ash.png
  - Assets/Art/Icons/icon_fine_sand.png
  - Assets/Art/Icons/icon_glass.png
  - Assets/Art/Icons/icon_clear_glass.png
  - Assets/Art/Icons/icon_leaded_glass.png
  - Assets/Art/Icons/icon_mirror_sphere.png
  - Assets/Art/Icons/icon_mirror_glass.png
created: 2026-11-18
---
# 400 Cam eşyalar, aynalar, kutu yatak ve cam ürün ikonları

## Amaç
Ev dekorasyonu D3, cam zinciri (Project belgesi `27-ev-dekorasyonu` §18). Eşyalar yalnızca yapılır (`craftonly`), kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Masa üstü eşyalar **301 ile aynı** (küçük tuval, 382'deki gibi masa üstü ölçeği); duvar aynaları **306 ile aynı** (`sw_ne`, öbür yüz aynayla; büyük ayna iki kenar boyunca, 380'deki geniş eşya kılavuzu); kutu yatak 2x1 uzun mobilya (302 ölçeği); ikonlar **316 ile aynı** (32x32, tek nesne).
- Takım renkleri 301/306'daki gibi (Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir). Orman camı **yeşilimsi-sarımsı**, kabarcıklı; berrak cam (kadeh) daha açık ve parlak.

### Eşyalar
| # | items.json id (takım) | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.glass_bottle` (Zanaatkâr) | `iso_item_glass_bottle_se.png` | yuva K | `small greenish forest-glass bottle with a long neck and a cork` |
| 2 | `item.glass_cup` (Zanaatkâr) | `iso_item_glass_cup_se.png` | yuva K | `small greenish forest-glass beaker with applied glass prunts` |
| 3 | `item.glass_jar` (Zanaatkâr) | `iso_item_glass_jar_se.png` | yuva K | `squat greenish glass jar with a cloth cover tied with string, dark jam inside` |
| 4 | `item.glass_goblet` (Tüccar) | `iso_item_glass_goblet_se.png` | yuva K | `clear glass goblet on a slender stem, faint golden tint` |
| 5 | `item.hourglass` (Tüccar) | `iso_item_hourglass_se.png` | yuva K | `medieval hourglass: two glass bulbs with pale sand in a turned wooden frame` |
| 6 | `item.convex_mirror` (Tüccar) | `iso_wall_item_convex_mirror_sw_ne.png` | 1 kenar | `small round convex glass mirror in a carved painted wooden frame` |
| 7 | `item.large_mirror` (Asil) | `iso_wall_item_large_mirror_2w_sw_ne.png` | 2 kenar | `large framed mirror of several convex glass roundels set in a carved walnut frame` |
| 8 | `item.box_bed` (Zanaatkâr, Çatkı) | `iso_item_box_bed_2x1_se.png` | 2x1 uzun | `enclosed wooden box bed (cupboard bed) with sliding doors half open, quilt inside` |

### İkonlar
`icon_ash.png` (grey wood ash in a small heap), `icon_fine_sand.png` (fine pale sand heap), `icon_glass.png` (lump of greenish raw glass), `icon_clear_glass.png` (lump of clear glass), `icon_leaded_glass.png` (small diamond panes in lead came), `icon_mirror_sphere.png` (glass sphere with a dark lead lining), `icon_mirror_glass.png` (small convex round mirror piece).

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem: orman camı ve kurşun astarlı dışbükey ayna ortaçağda bilinir (27 §18.1); düz büyük cam ayna yok, büyük ayna küçük yuvarlak ayna parçalarından oluşur.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
