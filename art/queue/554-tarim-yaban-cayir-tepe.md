---
id: "554"
title: "Yaban bitkileri 2/3: çayır ve tepe (9)"
status: running
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
  - Assets/Art/Iso/_aday/iso_forage_sorrel_se.png
  - Assets/Art/Iso/_aday/iso_forage_mallow_se.png
  - Assets/Art/Iso/_aday/iso_forage_dandelion_se.png
  - Assets/Art/Iso/_aday/iso_forage_knotweed_se.png
  - Assets/Art/Iso/_aday/iso_forage_wild_asparagus_se.png
  - Assets/Art/Iso/_aday/iso_forage_sumac_se.png
  - Assets/Art/Iso/_aday/iso_forage_juniper_se.png
  - Assets/Art/Iso/_aday/iso_forage_salep_orchid_se.png
  - Assets/Art/Iso/_aday/iso_forage_st_johns_wort_se.png
created: 2026-11-20
---
# 554 Yaban bitkileri 2/3: çayır ve tepe (9)

## Amaç
Project belgesi 59 §6.2: **yalnız doğadan toplanan** bitkiler (ekilemez, pazarda satılmaz). Bölgeye ve mevsime göre belirir.

## Tarif
Yöntem **373 ile aynı**: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24; 373'teki açıklama eki aynen. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| kuzukulağı | `iso_forage_sorrel_se.png` | 12 | `clump of arrow-shaped sorrel leaves with a reddish seed stalk` |
| ebegümeci | `iso_forage_mallow_se.png` | 16 | `mallow plant with round scalloped leaves and small pink flowers` |
| karahindiba | `iso_forage_dandelion_se.png` | 10 | `dandelion rosette with toothed leaves, one yellow flower and one seed head` |
| madımak | `iso_forage_knotweed_se.png` | 8 | `low mat of knotgrass with tiny leaves along wiry stems` |
| yabani kuşkonmaz | `iso_forage_wild_asparagus_se.png` | 20 | `thin wild asparagus spears rising from a prickly feathery bush` |
| sumak | `iso_forage_sumac_se.png` | 32 | `sumac shrub with long compound leaves turning red and upright dark red fruit cones` |
| ardıç | `iso_forage_juniper_se.png` | 26 | `low spiky juniper bush with blue-black berries` |
| salep orkidesi | `iso_forage_salep_orchid_se.png` | 16 | `single wild orchid with a spike of purple flowers and spotted leaves` |
| sarı kantaron | `iso_forage_st_johns_wort_se.png` | 18 | `upright St John's wort with small leaves and clusters of yellow star flowers` |

- Maliyet 9 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod düğümü adla bulur (`iso_forage_<ad>_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
