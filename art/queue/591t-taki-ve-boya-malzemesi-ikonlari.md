---
id: "591t"
title: "Takı, takı malzemesi ve boya malzemesi ikonları (20)"
status: review
claimed_at: 2026-11-20T15:48Z
approved_by: user        # otomatik akış (CLAUDE.md, 2026-11-17); kullanıcı 20.11.2026 sohbet: "assetleri de sıraya ekle"
priority: 35
family: arayuz
style_gate: false
cost_est: 20
cost_max: 40
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_jw_earring.png
  - Assets/Art/Iso/_aday/icon_item_jw_nose_ring.png
  - Assets/Art/Iso/_aday/icon_item_jw_cord_necklace.png
  - Assets/Art/Iso/_aday/icon_item_jw_glass_beads.png
  - Assets/Art/Iso/_aday/icon_item_jw_brooch.png
  - Assets/Art/Iso/_aday/icon_item_jw_ring.png
  - Assets/Art/Iso/_aday/icon_item_jw_seal_ring.png
  - Assets/Art/Iso/_aday/icon_item_jw_bracelet.png
  - Assets/Art/Iso/_aday/icon_item_jw_evil_eye.png
  - Assets/Art/Iso/_aday/icon_item_jw_ring_gold.png
  - Assets/Art/Iso/_aday/icon_item_jw_ring_gem.png
  - Assets/Art/Iso/_aday/icon_item_jw_earring_gold.png
  - Assets/Art/Iso/_aday/icon_item_jw_earring_gem.png
  - Assets/Art/Iso/_aday/icon_item_jw_necklace_gem.png
  - Assets/Art/Iso/_aday/icon_item_jw_evil_eye_gem.png
  - Assets/Art/Iso/_aday/icon_item_gold_dust.png
  - Assets/Art/Iso/_aday/icon_item_gemstone.png
  - Assets/Art/Iso/_aday/icon_item_weld_berry.png
  - Assets/Art/Iso/_aday/icon_item_walnut_hull.png
  - Assets/Art/Iso/_aday/icon_item_lichen.png
created: 2026-11-20
---
# 591t Takı, takı malzemesi ve boya malzemesi ikonları (20)

## Amaç
Project belgesi 62 §2: altın tozu ve değerli taşla yapılan altı yeni takı eklendi, takılar Aile panelinde takılıp çıkarılıyor. Eski dokuz takının, iki malzemenin (altın tozu, değerli taş) ve üç boya malzemesinin (cehri tanesi, ceviz kabuğu, taş likeni) ikonu yok; listelerde yalnızca yazı görünüyor.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person, no hand`.
- Aile kuralı: **bakır** takılar sıcak turuncu-kahve, **gümüş** soğuk açık gri, **altın** parlak sarı; aynı türün bakır ve altın hâli aynı biçimde, yalnızca metal rengi ve taş farkı (yüzük ile altın yüzük yan yana aynı yüzük gibi okunmalı). Taş tek ve iri, 2-3 piksel parlama.
- Dönem kuralı (CLAUDE.md, Project belgesi 20): fasetli modern pırlanta kesimi yok; taşlar yuvarlak kabaşon. Zincir, kopça ve iğne elle dövme.

## Tarif
| Dosya (`_aday/`) | Eşya | Açıklama |
|---|---|---|
| `icon_item_jw_earring.png` | Küpe | `a pair of small plain copper hoop earrings` |
| `icon_item_jw_nose_ring.png` | Hızma | `one tiny thin copper nose ring, an open hoop with a small bead` |
| `icon_item_jw_cord_necklace.png` | İp kolye | `necklace of a dark flax cord with one carved bone pendant` |
| `icon_item_jw_glass_beads.png` | Cam boncuk dizisi | `loop of round blue and green glass beads on a string` |
| `icon_item_jw_brooch.png` | Broş | `round copper cloak brooch, an open ring with a pin across it` |
| `icon_item_jw_ring.png` | Yüzük | `plain copper finger ring` |
| `icon_item_jw_seal_ring.png` | Mühür yüzüğü | `silver signet ring with a flat round engraved face` |
| `icon_item_jw_bracelet.png` | Bilezik | `open copper wrist bangle of twisted wire` |
| `icon_item_jw_evil_eye.png` | Nazar boncuğu | `flat round blue glass bead with white and dark blue concentric rings, hanging on a short flax cord` |
| `icon_item_jw_ring_gold.png` | Altın yüzük | `plain bright gold finger ring` |
| `icon_item_jw_ring_gem.png` | Taşlı altın yüzük | `gold finger ring set with one round red cabochon gemstone` |
| `icon_item_jw_earring_gold.png` | Altın küpe | `a pair of small plain gold hoop earrings` |
| `icon_item_jw_earring_gem.png` | Taşlı altın küpe | `a pair of gold drop earrings, each with one small round green cabochon gemstone` |
| `icon_item_jw_necklace_gem.png` | Taşlı gerdanlık | `short gold chain necklace with three round cabochon gemstones, a red one in the middle and two blue ones` |
| `icon_item_jw_evil_eye_gem.png` | Taşlı nazarlık | `flat round blue glass bead with concentric rings and a small red gemstone set in its centre, hanging on a flax cord` |
| `icon_item_gold_dust.png` | Altın tozu | `small open leather pouch with glittering gold dust inside` |
| `icon_item_gemstone.png` | Değerli taş | `one rough uncut deep red gemstone` |
| `icon_item_weld_berry.png` | Cehri tanesi | `small handful of yellow-green buckthorn berries with one leaf` |
| `icon_item_walnut_hull.png` | Ceviz kabuğu | `three split green and dark brown walnut husks` |
| `icon_item_lichen.png` | Taş likeni | `clump of pale grey-green lichen scraped from a rock` |

## Doğrulama
- 32x32, yarı saydam 0, renk ≤ 16. `python tools/art/style.py check --family arayuz <dosya> ...`.
- Toplu önizleme x4, parşömen (#EADBB8) ve koyu (#333333) zeminde: `art/results/591t-ikonlar-x4.png`, `art/results/591t-ikonlar-x4-koyu.png`.
- Göz kontrolü: yüzük, mühür yüzüğü, altın yüzük ve taşlı yüzük dördü yan yana ayırt ediliyor; nazar boncuğu ile taşlı nazarlık ortadaki taştan ayrılıyor; küpe çifti tek bir halka gibi okunmuyor.

## Notlar
- Kod ikonu adla bulur (`icon_item_<ad>`, `IsoArt.ItemIcon`); kabulden sonra kod değişikliği gerekmez.
- Karakterin üstünde görünen takı katmanı bu işte yok (84 pikselde 1-2 piksel kalır); istenirse giysi toplu parti planına (509) eklenir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
