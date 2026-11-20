---
id: "478a"
title: "Beceri tamamlama: yeni ekinler ve yer mantarı düğümü (10)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("geliştirmeyi tamamla"), Project belgesi 45 bölüm 9
priority: 40
family: bitki
style_gate: false
cost_est: 10
cost_max: 22
depends_on: ["374", "373"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_herb_bed_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_herb_bed_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_herb_bed_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_saffron_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_saffron_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_saffron_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_vetch_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_vetch_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_vetch_3_se.png
  - Assets/Art/Iso/_aday/iso_forage_truffle_se.png
created: 2026-11-20
---
# 478a Beceri tamamlama: yeni ekinler ve yer mantarı düğümü (10)

## Amaç
Belge 45: Bahçıvanlık 2 mutfak otu yatağı, Bahçıvanlık 4 safran çiğdemi, Toprak bakımı 7 yeşil gübre (fiğ), Toplayıcılık 8 yer mantarı.

## Tarif
Ekinler: **374 ile aynı yöntem** (üç evre, 80x72, pivot 40,24, `se`). Yer mantarı düğümü: **373 ile aynı yöntem**.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.crop_herb_bed` | `iso_crop_herb_bed_1_se.png` | 80x72 | `young kitchen herb bed, tiny dill and fenugreek sprouts in tilled soil` |
| 2 | `item.crop_herb_bed` | `iso_crop_herb_bed_2_se.png` | 80x72 | `growing kitchen herb bed, feathery dill, fenugreek and small lavender` |
| 3 | `item.crop_herb_bed` | `iso_crop_herb_bed_3_se.png` | 80x72 | `ripe kitchen herb bed, tall feathery dill, fenugreek and purple lavender` |
| 4 | `item.crop_saffron` | `iso_crop_saffron_1_se.png` | 80x72 | `saffron crocus shoots, thin grass-like leaves in tilled soil` |
| 5 | `item.crop_saffron` | `iso_crop_saffron_2_se.png` | 80x72 | `saffron crocus clump with closed pale purple buds` |
| 6 | `item.crop_saffron` | `iso_crop_saffron_3_se.png` | 80x72 | `saffron crocus in bloom, open lilac flowers with red stigmas` |
| 7 | `item.crop_vetch` | `iso_crop_vetch_1_se.png` | 80x72 | `young vetch seedlings, small paired leaves in tilled soil` |
| 8 | `item.crop_vetch` | `iso_crop_vetch_2_se.png` | 80x72 | `climbing vetch with tendrils, low green mat` |
| 9 | `item.crop_vetch` | `iso_crop_vetch_3_se.png` | 80x72 | `lush vetch green manure with small purple flowers` |
| 10 | `item.forage_truffle` | `iso_forage_truffle_se.png` | 1x1 | `oak roots with disturbed soil and a black truffle showing` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- Kod ekin evrelerini adla bulur (`IsoArt.Crops`: iso_crop_herb_bed, iso_crop_saffron, iso_crop_vetch; `_1/_2/_3_se`); gelene kadar renkli şekil çizer.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
