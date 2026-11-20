---
id: "542"
title: "Yeni ekinler 3/7: börülce, karabuğday, susam, aspir, hardal (15)"
status: pending
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
  - Assets/Art/Iso/_aday/iso_crop_cowpea_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_cowpea_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_cowpea_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_buckwheat_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_buckwheat_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_buckwheat_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_sesame_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_sesame_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_sesame_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_safflower_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_safflower_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_safflower_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_mustard_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_mustard_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_mustard_3_se.png
created: 2026-11-20
---
# 542 Yeni ekinler 3/7: börülce, karabuğday, susam, aspir, hardal (15)

## Amaç
Project belgesi 59 §5: yağ, boya ve ara ekinler. Üç evre: filiz, gelişmiş bitki, **olgun** (olgun görsel yalnızca ekin biçilmeye hazırken çizilir; bu yüzden 3. evre üründen açıkça okunmalı: başak, meyve, çiçek ya da kök başı görünür olsun).

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_cowpea` (börülce) | `iso_crop_cowpea_1_se.png` | `cowpea seedlings with trifoliate leaves beside a thin stake` |
| `item.crop_cowpea` (börülce) | `iso_crop_cowpea_2_se.png` | `cowpea vine climbing a stake with pale flowers` |
| `item.crop_cowpea` (börülce) | `iso_crop_cowpea_3_se.png` | `cowpea vine on a stake with long slender hanging green pods` |
| `item.crop_buckwheat` (karabuğday) | `iso_crop_buckwheat_1_se.png` | `buckwheat seedlings with heart-shaped leaves` |
| `item.crop_buckwheat` (karabuğday) | `iso_crop_buckwheat_2_se.png` | `buckwheat with reddish stems and heart-shaped leaves` |
| `item.crop_buckwheat` (karabuğday) | `iso_crop_buckwheat_3_se.png` | `buckwheat with clusters of small white flowers and dark triangular seeds` |
| `item.crop_sesame` (susam) | `iso_crop_sesame_1_se.png` | `small sesame seedlings` |
| `item.crop_sesame` (susam) | `iso_crop_sesame_2_se.png` | `upright sesame stems with narrow leaves and pale bell flowers` |
| `item.crop_sesame` (susam) | `iso_crop_sesame_3_se.png` | `tall sesame stems lined with upright green seed capsules` |
| `item.crop_safflower` (aspir) | `iso_crop_safflower_1_se.png` | `spiny safflower seedling rosette` |
| `item.crop_safflower` (aspir) | `iso_crop_safflower_2_se.png` | `safflower plant with spiny leaves and closed thistle-like buds` |
| `item.crop_safflower` (aspir) | `iso_crop_safflower_3_se.png` | `safflower with bright orange-yellow thistle-like flower heads` |
| `item.crop_mustard` (hardal) | `iso_crop_mustard_1_se.png` | `mustard seedlings with rough lobed leaves` |
| `item.crop_mustard` (hardal) | `iso_crop_mustard_2_se.png` | `leafy mustard plant with a rising flower stalk` |
| `item.crop_mustard` (hardal) | `iso_crop_mustard_3_se.png` | `mustard with small bright yellow flowers and thin upright seed pods` |

- Maliyet 15 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
