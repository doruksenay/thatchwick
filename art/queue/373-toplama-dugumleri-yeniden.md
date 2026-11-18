---
id: "373"
title: Toplama düğümleri yeniden (böğürtlen, fındık, otlar, mürver, çilek, kestane, kuşburnu, ısırgan)
status: pending
priority: 10            # başlangıç haritasında görünenler önce (görsel analiz)
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 ("yap bunu": yeniden deneme işleri)
family: bitki
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_berry_bush_se.png
  - Assets/Art/Iso/_aday/iso_forage_hazel_se.png
  - Assets/Art/Iso/_aday/iso_forage_herbs_se.png
  - Assets/Art/Iso/_aday/iso_forage_elderflower_se.png
  - Assets/Art/Iso/_aday/iso_forage_strawberry_se.png
  - Assets/Art/Iso/_aday/iso_forage_chestnut_se.png
  - Assets/Art/Iso/_aday/iso_forage_rosehip_se.png
  - Assets/Art/Iso/_aday/iso_forage_nettle_se.png
created: 2026-11-18
---
# 373 Toplama düğümleri yeniden

## Amaç
Başlangıç haritasındaki 25 toplama düğümü (otlar, yabani sarımsak, ısırgan, mürver) hâlâ kahverengi küp. 311 ve 322'nin kurtarılamayan adayları burada yeniden üretilir. Sorunlar biçimdeydi:
- böğürtlen ve çilek toprak bloğu üstünde;
- otlar zemin karosu;
- fındık turuncu sonbahar ağacı;
- mürver çimli ağaç;
- kestane tanınmıyor;
- kuşburnu gül çiçeği;
- ısırgan çok koyu.

Yabani sarımsak, tere, kuru dal, kovan ve mantar 371'de kurtarılıyor.

## Tarif
Yöntem **311 ile aynı**: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24. Farklar 372'deki gibi:
- Referanslar ailenin yeni referansları; özellikle `iso_forage_pinecone_se`.
- Açıklama eki: `, small forage plant seen from high top-down isometric view, muted desaturated natural colours, simple shading, light from top-left, no ground tile, no soil block, no grass patch, no shadow, transparent background`
- Üretimden sonra `style.py fit --family bitki`, ardından `check`. Gerekirse 48 renk nicemleme.
- İkinci deneme yalnızca biçim için.

| Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|
| `iso_forage_berry_bush_se.png` | 26 | `wild blackberry bramble bush with arching thorny canes, dark green leaves and clusters of black and red berries` |
| `iso_forage_hazel_se.png` | 30 | `small hazel shrub with several thin upright stems, round green leaves and a few brown hazelnuts` |
| `iso_forage_herbs_se.png` | 16 | `small clump of wild herbs: thyme, sorrel and yarrow with tiny white and purple flowers` |
| `iso_forage_elderflower_se.png` | 30 | `elder shrub (not a tree) with several stems, dark green leaves and flat creamy white elderflower umbels` |
| `iso_forage_strawberry_se.png` | 10 | `low wild strawberry plant with trefoil leaves, tiny white flowers and a few small red strawberries` |
| `iso_forage_chestnut_se.png` | 10 | `a few fallen sweet chestnuts in open spiky green husks with shiny brown nuts on the ground` |
| `iso_forage_rosehip_se.png` | 26 | `dog rose bush in autumn with thorny stems, few leaves and many small oval red rose hips, no flowers` |
| `iso_forage_nettle_se.png` | 20 | `clump of stinging nettles with upright stems and serrated mid-green leaves, clearly readable leaf shapes` |

- Maliyet 8 × 1 gen. İkinci deneme payı 8: `cost_max` 16.

## Doğrulama
- 311 doğrulaması.
- Yan yana sayfası: 371'deki düğümlerle (sarımsak, tere, dal, kovan, mantar) ve kozalakla, köylü ölçeğinde.
- Düğüm köylüden küçük, ağaçtan çok küçük.
- Çimen üstünde 6x6 sayfa: düğümler fark ediliyor ama göz yormuyor.

## Notlar
- Dönem: hepsi Eski Dünya türü (böğürtlen, fındık, mürver, yaban çileği, kestane, köpek gülü, ısırgan).
- Kod tarafı (eşleme `IsoArt.Items`): iş review'a geçince.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
