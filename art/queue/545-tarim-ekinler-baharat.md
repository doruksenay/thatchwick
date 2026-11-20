---
id: "545"
title: "Yeni ekinler 6/7: kimyon, kişniş, anason, çörek otu (12)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 20
family: bitki
style_gate: false
cost_est: 12
cost_max: 24
depends_on: ["374"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_cumin_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_cumin_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_cumin_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_coriander_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_coriander_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_coriander_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_anise_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_anise_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_anise_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_nigella_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_nigella_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_nigella_3_se.png
created: 2026-11-20
---
# 545 Yeni ekinler 6/7: kimyon, kişniş, anason, çörek otu (12)

## Amaç
Project belgeleri 59 §5 ve 58a §2: tohum baharatlar (ek baharat yuvası). Üç evre: filiz, gelişmiş bitki, **olgun** (olgun görsel yalnızca ekin biçilmeye hazırken çizilir; bu yüzden 3. evre üründen açıkça okunmalı: başak, meyve, çiçek ya da kök başı görünür olsun).

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_cumin` (kimyon) | `iso_crop_cumin_1_se.png` | `thread-thin cumin seedlings` |
| `item.crop_cumin` (kimyon) | `iso_crop_cumin_2_se.png` | `low feathery cumin plant` |
| `item.crop_cumin` (kimyon) | `iso_crop_cumin_3_se.png` | `cumin with small white-pink flower umbels and slender ridged seeds` |
| `item.crop_coriander` (kişniş) | `iso_crop_coriander_1_se.png` | `small coriander leaves like flat parsley` |
| `item.crop_coriander` (kişniş) | `iso_crop_coriander_2_se.png` | `coriander with lacy upper leaves and a flower stalk` |
| `item.crop_coriander` (kişniş) | `iso_crop_coriander_3_se.png` | `coriander with white flower umbels and round green seeds` |
| `item.crop_anise` (anason) | `iso_crop_anise_1_se.png` | `small anise seedlings with rounded leaves` |
| `item.crop_anise` (anason) | `iso_crop_anise_2_se.png` | `anise with feathery upper leaves` |
| `item.crop_anise` (anason) | `iso_crop_anise_3_se.png` | `anise with white flower umbels and grey-green seeds` |
| `item.crop_nigella` (çörek otu) | `iso_crop_nigella_1_se.png` | `fine thread-leaved nigella seedlings` |
| `item.crop_nigella` (çörek otu) | `iso_crop_nigella_2_se.png` | `nigella with feathery leaves and pale blue flowers` |
| `item.crop_nigella` (çörek otu) | `iso_crop_nigella_3_se.png` | `nigella with inflated striped seed capsules` |

- Maliyet 12 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
