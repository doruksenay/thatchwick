---
id: "408"
title: "Kıyı Evi tarzı (8 eşya)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 18.11.2026 ("hepsini"), Project belgesi 27 §6.1 ve 27e
priority: 30
family: esya
style_gate: false
cost_est: 10
cost_max: 16
depends_on: ["301", "302", "306", "337"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_net_curtain_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_oar_rack_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_fish_basket_decor_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_dried_fish_string_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_shell_bowl_se.png
  - Assets/Art/Iso/_aday/iso_item_bleached_bench_2x1_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_green_bottle_shelf_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_rope_coil_se.png
created: 2026-11-18
---
# 408 Kıyı Evi tarzı (8 eşya)

## Amaç
D4 tarzı **Kıyı Evi** (balıkçı ve göl kumsalı): ağartılmış ahşap, ağ, ip, kum rengi; cam zincirinin yeşil orman camı şişeleri. Kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Masa ve raf üstü **301 ile aynı** (32 px tuval, pivot nesnenin tabanı), duvar eşyaları **306 ile aynı** (`sw_ne`; 2 kenarlık geniş eşya 380'deki kılavuz), yer eşyası 302 ölçeği, halı **337 ile aynı** (düz, gölgesiz). Tek yönlü 2x1 ve 3x1 görsel `_se` adıyla, uzun ekseni "\" (390 gibi). Teslim `cp -n`, `.meta` yok.
- Renkler: ağartılmış gri-bej ahşap, kum, solgun mavi-yeşil, kenevir ipi. Yeni Dünya bitkisi ya da deniz kabuğu süsü yok (göl midyesi kabuğu).

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.net_curtain` (Köylü) | `iso_wall_item_net_curtain_2w_sw_ne.png` | 2 kenar | `fishing net hung as a curtain with small cork floats, draped on the wall` |
| 2 | `item.oar_rack` (Köylü) | `iso_wall_item_oar_rack_sw_ne.png` | 1 kenar | `two crossed wooden oars on wall pegs` |
| 3 | `item.fish_basket_decor` (Köylü) | `iso_item_fish_basket_decor_se.png` | yuva O | `small woven willow fish creel basket` |
| 4 | `item.dried_fish_string` (Köylü) | `iso_wall_item_dried_fish_string_sw_ne.png` | 1 kenar, üst | `string of four dried fish hanging from a peg` |
| 5 | `item.shell_bowl` (Köylü) | `iso_item_shell_bowl_se.png` | yuva K | `small clay bowl filled with pale freshwater mussel shells` |
| 6 | `item.bleached_bench` (Köylü) | `iso_item_bleached_bench_2x1_se.png` | 2x1 | `long bench of sun-bleached grey driftwood planks` |
| 7 | `item.green_bottle_shelf` (Zanaatkâr) | `iso_wall_item_green_bottle_shelf_sw_ne.png` | 1 kenar | `narrow wooden wall shelf, empty, rope-hung` |
| 8 | `item.rope_coil` (Köylü) | `iso_item_rope_coil_se.png` | 1x1 | `large coil of thick hemp rope on the floor` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20).

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
