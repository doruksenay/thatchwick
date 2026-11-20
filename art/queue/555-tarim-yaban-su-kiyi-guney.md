---
id: "555"
title: "Yaban bitkileri 3/3: ırmak, kıyı ve Güney Yamaçları (9)"
status: review
claimed_at: 2026-11-20T14:49Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 30
family: bitki
style_gate: false
cost_est: 9
cost_max: 18
depends_on: ["373"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_licorice_se.png
  - Assets/Art/Iso/_aday/iso_forage_marsh_mallow_se.png
  - Assets/Art/Iso/_aday/iso_forage_willow_bark_se.png
  - Assets/Art/Iso/_aday/iso_forage_water_chestnut_se.png
  - Assets/Art/Iso/_aday/iso_forage_samphire_se.png
  - Assets/Art/Iso/_aday/iso_forage_caper_se.png
  - Assets/Art/Iso/_aday/iso_forage_myrtle_se.png
  - Assets/Art/Iso/_aday/iso_forage_kenger_se.png
  - Assets/Art/Iso/_aday/iso_forage_zahter_se.png
created: 2026-11-20
---
# 555 Yaban bitkileri 3/3: ırmak, kıyı ve Güney Yamaçları (9)

## Amaç
Project belgesi 59 §6.2: **yalnız doğadan toplanan** bitkiler (ekilemez, pazarda satılmaz). Bölgeye ve mevsime göre belirir.

## Tarif
Yöntem **373 ile aynı**: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24; 373'teki açıklama eki aynen. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| meyan kökü | `iso_forage_licorice_se.png` | 22 | `licorice plant with feathery leaves and pale lilac flower spikes, a woody root showing at the base` |
| hatmi | `iso_forage_marsh_mallow_se.png` | 26 | `tall marsh mallow with velvety grey-green leaves and pale pink flowers` |
| söğüt kabuğu | `iso_forage_willow_bark_se.png` | 24 | `cluster of young willow shoots with one stem's bark peeled in a strip` |
| su kestanesi | `iso_forage_water_chestnut_se.png` | 6 | `floating rosette of diamond-shaped water chestnut leaves with a dark horned nut` |
| deniz börülcesi | `iso_forage_samphire_se.png` | 12 | `clump of fleshy jointed bright green samphire stems on wet sand` |
| kapari | `iso_forage_caper_se.png` | 14 | `trailing caper bush with round leaves, green buds and one white-purple flower on a rock` |
| mersin | `iso_forage_myrtle_se.png` | 26 | `dense myrtle shrub with small glossy leaves and blue-black berries` |
| kenger | `iso_forage_kenger_se.png` | 16 | `spiny thistle-like gundelia plant with a round prickly flower head` |
| zahter | `iso_forage_zahter_se.png` | 12 | `low woody wild thyme cushion with tiny grey-green leaves and pink flowers` |

- Maliyet 9 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod düğümü adla bulur (`iso_forage_<ad>_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
