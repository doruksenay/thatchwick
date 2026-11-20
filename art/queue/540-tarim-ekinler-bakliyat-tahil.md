---
id: "540"
title: "Yeni ekinler 1/7: mercimek, nohut, darı, pirinç, haşhaş (15)"
status: running
claimed_at: 2026-11-20T13:21Z
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
  - Assets/Art/Iso/_aday/iso_crop_lentil_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_lentil_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_lentil_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_chickpea_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_chickpea_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_chickpea_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_millet_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_millet_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_millet_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_rice_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_rice_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_rice_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_poppy_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_poppy_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_poppy_3_se.png
created: 2026-11-20
---
# 540 Yeni ekinler 1/7: mercimek, nohut, darı, pirinç, haşhaş (15)

## Amaç
Project belgesi 59 §5: ilk dalga tarla ekinleri. Üç evre: filiz, gelişmiş bitki, **olgun** (olgun görsel yalnızca ekin biçilmeye hazırken çizilir; bu yüzden 3. evre üründen açıkça okunmalı: başak, meyve, çiçek ya da kök başı görünür olsun).

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_lentil` (mercimek) | `iso_crop_lentil_1_se.png` | `tiny lentil seedlings with fine paired leaflets` |
| `item.crop_lentil` (mercimek) | `iso_crop_lentil_2_se.png` | `low bushy lentil plant with feathery leaves` |
| `item.crop_lentil` (mercimek) | `iso_crop_lentil_3_se.png` | `bushy lentil plant with many small flat tan pods` |
| `item.crop_chickpea` (nohut) | `iso_crop_chickpea_1_se.png` | `small chickpea seedlings with toothed leaflets` |
| `item.crop_chickpea` (nohut) | `iso_crop_chickpea_2_se.png` | `upright chickpea plant with grey-green feathery leaves` |
| `item.crop_chickpea` (nohut) | `iso_crop_chickpea_3_se.png` | `chickpea plant with puffy pale green pods among the leaves` |
| `item.crop_millet` (darı) | `iso_crop_millet_1_se.png` | `thin millet grass shoots` |
| `item.crop_millet` (darı) | `iso_crop_millet_2_se.png` | `knee-high millet with broad grass leaves` |
| `item.crop_millet` (darı) | `iso_crop_millet_3_se.png` | `ripe millet with drooping golden seed heads` |
| `item.crop_rice` (pirinç) | `iso_crop_rice_1_se.png` | `short bright green rice shoots standing in shallow water` |
| `item.crop_rice` (pirinç) | `iso_crop_rice_2_se.png` | `dense green rice tufts in shallow water` |
| `item.crop_rice` (pirinç) | `iso_crop_rice_3_se.png` | `ripe rice with bowed golden panicles in shallow water` |
| `item.crop_poppy` (haşhaş) | `iso_crop_poppy_1_se.png` | `small rosette of lobed grey-green poppy leaves` |
| `item.crop_poppy` (haşhaş) | `iso_crop_poppy_2_se.png` | `tall poppy stems with nodding closed buds` |
| `item.crop_poppy` (haşhaş) | `iso_crop_poppy_3_se.png` | `poppy with open pale lilac flowers and round green seed capsules` |

- Maliyet 15 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
