---
id: "336b"
title: Çatı dokuları yeniden (saz, şindıl, kiremit, arduvaz; düz eğim ve sırt)
status: review
claimed_at: 2026-11-19T12:16Z
approved_by: user        # kullanıcı onayı sohbette 2026-11-19 ("önerdiklerini yap": 336 için yeniden deneme)
family: yapi
style_gate: false
cost_est: 12
cost_max: 20
depends_on: ["325"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_roof_thatch_a.png
  - Assets/Art/Iso/iso_roof_thatch_ridge.png
  - Assets/Art/Iso/iso_roof_shingle_a.png
  - Assets/Art/Iso/iso_roof_shingle_ridge.png
  - Assets/Art/Iso/iso_roof_tile_a.png
  - Assets/Art/Iso/iso_roof_tile_ridge.png
  - Assets/Art/Iso/iso_roof_slate_a.png
  - Assets/Art/Iso/iso_roof_slate_ridge.png
created: 2026-11-19
---
# 336b Çatı dokuları yeniden

## Amaç
336 hiçbir karo teslim edemedi (`art/results/336-cati-dokulari.md`). Üç sorun vardı:
- **Biçim:** saz, şindıl ve arduvaz eğim karoları düz doku değil, piramit (kırma çatı) gibi geldi. Döşenince her karo ayrı bir tepe gibi görünüyor.
- **Maske:** karolar maske dışına taşıyor, içinde delik bırakıyordu (31-109 opak piksel dışarıda, 1-49 boş piksel içeride).
- **Renk:** kiremit fazla doygun (x1,69), arduvaz fazla soluk (x0,38).

Saz ve kiremidin rengi ve dokusu beğenildi.

## Tarif
İşçinin 336 önerisi uygulanır:
1. **Araç:** `create_tiles_pro` yerine **`create_isometric_tile`, tek tek** (8 çağrı). Eğim karosu ile sırt karosu ayrı çağrılarda üretilir.
2. **Eğim karoları** (`_a`): açıklamaya `, flat roof surface texture seen orthographically from above, no peak, no pyramid, no ridge, uniform repeating pattern edge to edge, seamless` eklenir.
   - saz: `thick golden-brown thatch with straw bundles in parallel rows`
   - şindıl: `overlapping dark brown wooden shingles in staggered rows`
   - kiremit: `curved terracotta clay roof tiles in rows, muted earthy red`
   - arduvaz: `blue-grey slate tiles in staggered rows, slightly varied tones`
3. **Sırt karoları** (`_ridge`): aynı doku, tepe boyunca elmasın ortasından geçen bir sırt örtüsüyle. Örtüler: saz demeti, ahşap kapak, mahya kiremidi, düz taş kapak. Sırt **elmasın içinde kalır**, üstüne taşmaz. Açıklamaya `, the ridge cap lies flat inside the diamond, nothing rises above the tile` eklenir.
4. **Stil:** `style_images` = kabul edilmiş 325 duvar karosu ve kabul edilmiş zemin karoları. Mümkünse `style_options.color_palette` açık; kiremit ve arduvaz ailenin doygunluğuna yaklaşsın. Teslimden önce `style.py fit --family yapi` (ücretsiz) uygulanır, ardından `check`.
5. **Maske:** üretimden sonra 64x32 elmas maskesiyle kırpılır. Maske içinde en çok 5 boş piksel varsa en yakın opak komşuyla doldurulur (376b'deki delik onarımı); daha fazlası teslim edilmez.
6. **Yedek yöntem:** dört eğim karosundan ikiden fazlası yine piramit gelirse dur, raporla. Sonraki adım kullanıcının kararı (`create_building_kit`, `layout=materials`).

- Maliyet 8 çağrı ve olası `inpaint` payı: `cost_est` 12, `cost_max` 20. Tek deneme.

## Doğrulama
- `validate_iso.py --kind ground` ve `style.py check --family yapi`.
- 6x6 döşeme testi ve dikiş parlaklık oranı (hedef 0,95-1,05).
- Sırt testi: bir sıra eğim + bir sıra sırt + bir sıra eğim. Sırt hattı kesintisiz mi.
- Yan yana sayfası: 325 duvarıyla birlikte, çatı ve duvar aynı ailede mi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
