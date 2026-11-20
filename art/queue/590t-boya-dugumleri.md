---
id: "590t"
title: "Boya düğümleri: cehri çalısı, ceviz kabukları, likenli kaya (3)"
status: review
claimed_at: 2026-11-20T15:15Z
approved_by: user        # otomatik akış (CLAUDE.md, 2026-11-17); kullanıcı 20.11.2026 sohbet: "assetleri de sıraya ekle"
priority: 30
family: bitki
style_gate: false
cost_est: 3
cost_max: 6
depends_on: ["373"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_buckthorn_se.png
  - Assets/Art/Iso/_aday/iso_forage_walnut_hull_se.png
  - Assets/Art/Iso/_aday/iso_forage_lichen_se.png
created: 2026-11-20
---
# 590t Boya düğümleri (3)

## Amaç
Üç boya düğümü ormanda mevsiminde kendiliğinden çıkıyor (`ForageSystem`, `forageRegions: forest`) ama görselleri yok; yer tutucuyla çiziliyor. Ölü özellik taramasında (Project belgesi 62) bu yüzden "haritada yok" sanıldı. Ürünleri boya tariflerine girer: cehri tanesi (sarı), ceviz kabuğu (kahve), taş likeni (mor).

## Tarif
Yöntem **373 ile aynı**: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24; 373'teki açıklama eki aynen. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| Düğüm (`items.json`) | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| `item.forage_buckthorn` Cehri çalısı (sonbahar) | `iso_forage_buckthorn_se.png` | 26 | `wild buckthorn shrub with thorny dark branches, small oval green leaves and clusters of small yellow-green berries` |
| `item.forage_walnut_hull` Düşmüş ceviz kabukları (sonbahar) | `iso_forage_walnut_hull_se.png` | 8 | `scattered fallen walnut hulls on the forest floor, split green and dark brown husks, a few dry leaves` |
| `item.forage_lichen` Likenli kaya (dört mevsim) | `iso_forage_lichen_se.png` | 16 | `low grey forest rock covered with patches of pale grey-green and orange lichen` |

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Yan yana sayfası: 373'ün çalı ve mantar düğümleriyle. Ölçüt: üçü birbirinden ve `iso_terrain_boulder_small_se` kayasından (357) ayırt ediliyor; likenli kayada liken lekeleri uzaktan okunuyor.

## Notlar
- Kabulden sonra `items.json`'da üç düğüme `art` alanı eklenir (`{"alongX": "iso_forage_<ad>_se", "pivotX": 40.0, "pivotY": 24.0, "mirror": true}`); kod oturumunun işi.
- Ürün ikonları 591t'de.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
