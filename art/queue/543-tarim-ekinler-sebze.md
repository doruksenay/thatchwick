---
id: "543"
title: "Yeni ekinler 4/7: patlıcan, bamya, pazı, kereviz, maydanoz (15)"
status: review
claimed_at: 2026-11-20T13:30Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 20
family: bitki
style_gate: false
cost_est: 15
cost_max: 30
depends_on: ["374"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_eggplant_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_eggplant_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_eggplant_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_okra_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_okra_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_okra_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_chard_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_chard_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_chard_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_celery_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_celery_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_celery_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_parsley_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_parsley_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_parsley_3_se.png
created: 2026-11-20
---
# 543 Yeni ekinler 4/7: patlıcan, bamya, pazı, kereviz, maydanoz (15)

## Amaç
Project belgesi 59 §5: sebzeler (patlıcan ve bamya Güney Yamaçları). Üç evre: filiz, gelişmiş bitki, **olgun** (olgun görsel yalnızca ekin biçilmeye hazırken çizilir; bu yüzden 3. evre üründen açıkça okunmalı: başak, meyve, çiçek ya da kök başı görünür olsun).

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_eggplant` (patlıcan) | `iso_crop_eggplant_1_se.png` | `eggplant seedlings with soft grey-green leaves` |
| `item.crop_eggplant` (patlıcan) | `iso_crop_eggplant_2_se.png` | `bushy eggplant with broad leaves and purple star flowers` |
| `item.crop_eggplant` (patlıcan) | `iso_crop_eggplant_3_se.png` | `eggplant bush with several glossy dark purple elongated fruits` |
| `item.crop_okra` (bamya) | `iso_crop_okra_1_se.png` | `okra seedlings with lobed leaves` |
| `item.crop_okra` (bamya) | `iso_crop_okra_2_se.png` | `upright okra stalk with large lobed leaves and a pale yellow flower` |
| `item.crop_okra` (bamya) | `iso_crop_okra_3_se.png` | `okra stalk with upright ridged green pods` |
| `item.crop_chard` (pazı) | `iso_crop_chard_1_se.png` | `small chard leaves with pale stalks` |
| `item.crop_chard` (pazı) | `iso_crop_chard_2_se.png` | `chard clump with crinkled dark green leaves` |
| `item.crop_chard` (pazı) | `iso_crop_chard_3_se.png` | `full chard clump with large crinkled leaves and thick white stalks` |
| `item.crop_celery` (kereviz) | `iso_crop_celery_1_se.png` | `celery seedlings with small cut leaves` |
| `item.crop_celery` (kereviz) | `iso_crop_celery_2_se.png` | `leafy celery clump` |
| `item.crop_celery` (kereviz) | `iso_crop_celery_3_se.png` | `celery with a pale swollen root crown showing at the soil and tall leafy stalks` |
| `item.crop_parsley` (maydanoz) | `iso_crop_parsley_1_se.png` | `tiny parsley sprouts` |
| `item.crop_parsley` (maydanoz) | `iso_crop_parsley_2_se.png` | `low parsley tuft with curled leaves` |
| `item.crop_parsley` (maydanoz) | `iso_crop_parsley_3_se.png` | `dense bright green parsley mound` |

- Maliyet 15 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
