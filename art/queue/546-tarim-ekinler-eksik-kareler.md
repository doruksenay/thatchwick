---
id: "546"
title: "Yeni ekinler 7/7: kendir, çivit otu, kökboya, sarı boya otu ve havucun olgun karesi (13)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 20
family: bitki
style_gate: false
cost_est: 13
cost_max: 26
depends_on: ["374"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_carrot_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_hemp_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_hemp_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_hemp_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_woad_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_woad_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_woad_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_madder_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_madder_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_madder_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_weld_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_weld_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_weld_3_se.png
created: 2026-11-20
---
# 546 Yeni ekinler 7/7: kendir, çivit otu, kökboya, sarı boya otu ve havucun olgun karesi (13)

## Amaç
Project belgesi 58 §4 bulgu 2-3: oyunda olup evre görseli hiç olmayan dört ekin ve havucun eksik 3. evresi. Üç evre: filiz, gelişmiş bitki, **olgun** (olgun görsel yalnızca ekin biçilmeye hazırken çizilir; bu yüzden 3. evre üründen açıkça okunmalı: başak, meyve, çiçek ya da kök başı görünür olsun).

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_hemp` (kendir) | `iso_crop_hemp_1_se.png` | `hemp seedlings with serrated leaves` |
| `item.crop_hemp` (kendir) | `iso_crop_hemp_2_se.png` | `tall thin hemp stalks with palmate leaves` |
| `item.crop_hemp` (kendir) | `iso_crop_hemp_3_se.png` | `very tall dense hemp stalks with palmate leaves and seed tops` |
| `item.crop_woad` (çivit otu) | `iso_crop_woad_1_se.png` | `rosette of blue-green woad leaves` |
| `item.crop_woad` (çivit otu) | `iso_crop_woad_2_se.png` | `larger woad rosette with long blue-green leaves` |
| `item.crop_woad` (çivit otu) | `iso_crop_woad_3_se.png` | `woad with a tall stalk of small yellow flowers above blue-green leaves` |
| `item.crop_madder` (kökboya) | `iso_crop_madder_1_se.png` | `madder shoots with whorled leaves` |
| `item.crop_madder` (kökboya) | `iso_crop_madder_2_se.png` | `sprawling madder with whorls of rough leaves` |
| `item.crop_madder` (kökboya) | `iso_crop_madder_3_se.png` | `sprawling madder with small pale flowers and a red root showing at the soil` |
| `item.crop_weld` (sarı boya otu) | `iso_crop_weld_1_se.png` | `flat rosette of narrow weld leaves` |
| `item.crop_weld` (sarı boya otu) | `iso_crop_weld_2_se.png` | `weld with a rising leafy spike` |
| `item.crop_weld` (sarı boya otu) | `iso_crop_weld_3_se.png` | `weld with a tall slender spike of tiny pale yellow flowers` |
| `item.crop_carrot` (havuç) | `iso_crop_carrot_3_se.png` | `ripe carrot plant: feathery green top and the orange-purple shoulder of the root showing at the soil` |

- Maliyet 13 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
