---
id: "405"
title: "Mevsim süsleri (taze ve solmuş) ve hayvan bölmesi süsleri (10)"
status: running
claimed_at: 2026-11-18T19:17Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 27 §7.11, §7.12, §9 (18.11.2026)
priority: 20
family: esya
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["301", "306", "338"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_spring_branch_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_spring_garland_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_harvest_heap_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_winter_greenery_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_door_wreath_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_name_plaque_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_horseshoe_display_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_saddle_peg_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_straw_ornament_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_coop_curtain_sw_ne.png
created: 2026-11-18
---
# 405 Mevsim süsleri ve hayvan bölmesi süsleri

## Amaç
Ev dekorasyonu D3b. **Mevsim süsleri** gerçek malzemeden yapılır (mürver çiçeği, söğüt dalı, başak, kozalak) ve mevsimi geçince solar: güzelliği eksiye döner, oyuncu kaldırıp yenisini asar. **Hayvan bölmesi süsleri** ahırdaki hayvan konforunun "süs" payını doldurur (`AnimalCare.ComfortOf`, en çok 15). Kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Masa ve raf üstü **301 ile aynı**, duvar eşyaları **306 ile aynı** (`sw_ne`), yer süsü 1x1 (302 ölçeği), ahır eşyaları 338'deki renkler (kaba çam, ip, saman). Teslim `cp -n`, `.meta` yok.
- **Solmuş hâl ayrı görsel istemez:** motor solmuş süsü kahverengiye boyar. İleride istenirse `_wilted` son ekli ikinci kare eklenebilir (kod şimdilik aramıyor).

### Eşyalar
| # | items.json id | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.spring_branch` (bahar) | `iso_item_spring_branch_se.png` | yuva O | `clay jug holding a branch of white elderflower blossom` |
| 2 | `item.spring_garland` (bahar, yaz) | `iso_wall_item_spring_garland_sw_ne.png` | 1 kenar | `garland of white elderflowers woven on willow withies, hanging in a loop` |
| 3 | `item.harvest_heap` (sonbahar) | `iso_item_harvest_heap_se.png` | 1x1 | `small decorative heap of wheat sheaves tied with straw rope` |
| 4 | `item.winter_greenery` (kış) | `iso_wall_item_winter_greenery_sw_ne.png` | 1 kenar, üst | `bundle of pine and juniper sprigs with pinecones hung under a beam` |
| 5 | `item.door_wreath` (sonbahar, kış) | `iso_wall_item_door_wreath_sw_ne.png` | 1 kenar | `round straw wreath with pinecones and a red woollen bow` |
| 6 | `item.name_plaque` (ahır) | `iso_wall_item_name_plaque_sw_ne.png` | 1 kenar | `small rough plank sign hung on a nail, blank carved surface` |
| 7 | `item.horseshoe_display` (ahır) | `iso_wall_item_horseshoe_display_sw_ne.png` | 1 kenar | `wooden board with three iron horseshoes nailed in a row` |
| 8 | `item.saddle_peg` (ahır) | `iso_wall_item_saddle_peg_sw_ne.png` | 1 kenar | `stout wooden peg sticking out of the wall with a coiled rope bridle` |
| 9 | `item.straw_ornament` (ahır) | `iso_wall_item_straw_ornament_sw_ne.png` | 1 kenar, üst | `small plaited straw corn-dolly ornament hanging from a string` |
| 10 | `item.coop_curtain` (kümes) | `iso_wall_item_coop_curtain_sw_ne.png` | 1 kenar | `short linen curtain hung on a rod in front of nesting boxes` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem: mürver, söğüt, çam ve ardıç Eski Dünya bitkileri; Yeni Dünya bitkisi yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
