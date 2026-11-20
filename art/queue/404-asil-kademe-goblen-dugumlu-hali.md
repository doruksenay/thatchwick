---
id: "404"
title: "Asil ve Tüccar kademe: goblenler, sancak, düğümlü halılar, kanopili yataklar, taht, oymalı sandık (10)"
status: accepted
claimed_at: 2026-11-18T19:17Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 27 §5.4, §7 (18.11.2026)
priority: 20
family: esya
style_gate: false
cost_est: 16
cost_max: 24
depends_on: ["301", "302", "306", "337"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_tapestry_hunt_3w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_tapestry_garden_3w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_dynasty_banner_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_rug_knotted_3x3_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_kufic_4x3_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_crest_3x3_se.png
  - Assets/Art/Iso/_aday/iso_item_canopy_bed_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_canopy_bed_curtained_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_throne_chair_se.png
  - Assets/Art/Iso/_aday/iso_item_carved_chest_2x1_se.png
created: 2026-11-18
---
# 404 Asil ve Tüccar kademe eşyaları

## Amaç
Ev dekorasyonu D3b (Project belgesi `27-ev-dekorasyonu` §5.4, §7): "Göz alıcı oda" yalnızca Tüccar ve Asil eşyalarıyla açılır; Asil set bugün inceydi. Kodda ve `items.json`'da var, şimdilik yer tutucu.

## Tarif
Duvar eşyaları **306 ile aynı** (`sw_ne`; 3 kenarlık goblen 380'deki geniş eşya kılavuzu). Halılar **337 ile aynı** (düz, gölgesiz, en çok 2 px kalınlık; `finishGroup: cloth`, renk motorda kök boya varyantıyla değişebilir). Mobilya 302 ölçeği. Teslim `cp -n`, `.meta` yok.
- Takım renkleri 301/306'daki gibi: **Tüccar** boyalı ahşap, pirinç, kalay; **Asil** ceviz, dövme demir, koyu kırmızı ve altın sarısı kumaş.
- Halılar dönem notu (27 §5.2): düğümlü yün halı Selçuklu Anadolu'sundan; kûfi kenar yazı değil, **yazıyı andıran geometrik kenar bordürü** (okunur harf yok). Armalı halıda arma tek renkli kalkan biçimi, hanedan rengi motorda.

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.tapestry_hunt` (Asil) | `iso_wall_item_tapestry_hunt_3w_sw_ne.png` | 3 kenar | `long woven wall tapestry of a deer hunt in a green forest, deep red border` |
| 2 | `item.tapestry_garden` (Asil) | `iso_wall_item_tapestry_garden_3w_sw_ne.png` | 3 kenar | `long woven wall tapestry of an enclosed flower garden with birds, deep blue border` |
| 3 | `item.dynasty_banner` (Asil) | `iso_wall_item_dynasty_banner_sw_ne.png` | 1 kenar, üst bant | `narrow hanging cloth banner with a plain shield emblem, fringed bottom` |
| 4 | `item.rug_knotted` (Tüccar) | `iso_item_rug_knotted_3x3_se.png` | 3x3 | `square knotted wool carpet with geometric octagon medallions in red and indigo` |
| 5 | `item.rug_kufic` (Asil) | `iso_item_rug_kufic_4x3_se.png` | 4x3 | `large knotted carpet with a wide angular script-like geometric border and star field` |
| 6 | `item.rug_crest` (Asil) | `iso_item_rug_crest_3x3_se.png` | 3x3 | `square knotted carpet with a single heraldic shield in the centre and a plain border` |
| 7 | `item.canopy_bed` (Tüccar) | `iso_item_canopy_bed_2x2_se.png` | 2x2 uzun | `wooden four-post canopy bed with a cloth tester on top, no curtains` |
| 8 | `item.canopy_bed_curtained` (Asil) | `iso_item_canopy_bed_curtained_2x2_se.png` | 2x2 uzun | `carved walnut four-post bed with heavy red curtains drawn half open` |
| 9 | `item.throne_chair` (Asil) | `iso_item_throne_chair_se.png` | 1x1 | `tall carved walnut high-backed chair with armrests (head of household seat)` |
| 10 | `item.carved_chest` (Asil) | `iso_item_carved_chest_2x1_se.png` | 2x1 | `carved walnut chest with arcaded front panels and wrought iron hinges` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20).

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
