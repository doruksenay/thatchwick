---
id: "410"
title: "Kervan Odası tarzı (8 eşya) ve şenlik bayrak dizisi"
status: accepted
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 18.11.2026 ("hepsini"), Project belgesi 27 §6.1 ve 27e
priority: 30
family: esya
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["301", "302", "306", "337"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_brass_ewer_se.png
  - Assets/Art/Iso/_aday/iso_item_tile_bowl_se.png
  - Assets/Art/Iso/_aday/iso_item_lattice_screen_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_brass_lantern_se.png
  - Assets/Art/Iso/_aday/iso_item_cushioned_divan_3x1_se.png
  - Assets/Art/Iso/_aday/iso_item_caravan_rug_3x2_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_tile_panel_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_spice_chest_se.png
  - Assets/Art/Iso/_aday/iso_fx_festive_bunting_6w.png
created: 2026-11-18
---
# 410 Kervan Odası tarzı (8 eşya) ve şenlik bayrak dizisi

## Amaç
D4 tarzı **Kervan Odası** (gezgin tüccar ve doğu yolu): pirinç, çini, desenli kumaş, yastık. Eşyalar yalnızca pazar günü gezgin tüccarda satılır. Ek olarak şenlik hazırlığında meydana gerilen **bayrak dizisi** (bugün renkli elmaslarla çiziliyor).

## Tarif
Masa ve raf üstü **301 ile aynı** (32 px tuval, pivot nesnenin tabanı), duvar eşyaları **306 ile aynı** (`sw_ne`; 2 kenarlık geniş eşya 380'deki kılavuz), yer eşyası 302 ölçeği, halı **337 ile aynı** (düz, gölgesiz). Tek yönlü 2x1 ve 3x1 görsel `_se` adıyla, uzun ekseni "\" (390 gibi). Teslim `cp -n`, `.meta` yok.
- Renkler: pirinç sarısı, çini mavisi ve turkuaz, kökboya kırmızısı. Çinide **yazı yok**, geometrik ve bitkisel desen.
- Bayrak dizisi: iki ahşap direk arasında sarkan ip, üçgen kumaş bayraklar (kırmızı, sarı, mavi, yeşil); 6 karo genişliğinde tek görsel ya da tekrarlanan 1 bayrak + ip parçası.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.brass_ewer` (Tüccar) | `iso_item_brass_ewer_se.png` | yuva K | `tall brass ewer with a long curved spout` |
| 2 | `item.tile_bowl` (Tüccar) | `iso_item_tile_bowl_se.png` | yuva K | `glazed ceramic bowl with blue and turquoise geometric pattern` |
| 3 | `item.lattice_screen` (Tüccar) | `iso_item_lattice_screen_2x1_se.png` | 2x1 uzun | `carved wooden lattice folding screen` |
| 4 | `item.brass_lantern` (Tüccar) | `iso_item_brass_lantern_se.png` | tavan | `pierced brass hanging lantern on a chain, lit` |
| 5 | `item.cushioned_divan` (Tüccar) | `iso_item_cushioned_divan_3x1_se.png` | 3x1 | `long low divan with patterned cushions and bolsters` |
| 6 | `item.caravan_rug` (Tüccar) | `iso_item_caravan_rug_3x2_se.png` | 3x2 | `knotted carpet with red field and repeating star medallions` |
| 7 | `item.tile_panel` (Tüccar) | `iso_wall_item_tile_panel_sw_ne.png` | 1 kenar | `square panel of glazed tiles with blue floral pattern in a wooden frame` |
| 8 | `item.spice_chest` (Tüccar) | `iso_item_spice_chest_se.png` | 1x1 | `small brass-bound wooden chest with little drawers` |
| 9 | `(şenlik)` (—) | `iso_fx_festive_bunting_6w.png` | meydan, 6 karo | `string of triangular cloth pennants in red yellow blue green between two wooden poles` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20).

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
