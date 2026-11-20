---
id: "544"
title: "Yeni ekinler 5/7: roka, karnabahar, kuşkonmaz, enginar, gül, aynısefa (18)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 20
family: bitki
style_gate: false
cost_est: 18
cost_max: 36
depends_on: ["374"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_rocket_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_rocket_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_rocket_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_cauliflower_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_cauliflower_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_cauliflower_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_asparagus_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_asparagus_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_asparagus_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_artichoke_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_artichoke_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_artichoke_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_rose_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_rose_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_rose_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_calendula_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_calendula_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_calendula_3_se.png
created: 2026-11-20
---
# 544 Yeni ekinler 5/7: roka, karnabahar, kuşkonmaz, enginar, gül, aynısefa (18)

## Amaç
Project belgesi 59 §5: yaprak, kalıcı yatak ve çiçek ekinleri (gül ve aynısefa bakım hattının ham maddesi). Üç evre: filiz, gelişmiş bitki, **olgun** (olgun görsel yalnızca ekin biçilmeye hazırken çizilir; bu yüzden 3. evre üründen açıkça okunmalı: başak, meyve, çiçek ya da kök başı görünür olsun).

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_rocket` (roka) | `iso_crop_rocket_1_se.png` | `tiny rocket sprouts` |
| `item.crop_rocket` (roka) | `iso_crop_rocket_2_se.png` | `small rosette of lobed rocket leaves` |
| `item.crop_rocket` (roka) | `iso_crop_rocket_3_se.png` | `full rocket rosette with long lobed dark green leaves` |
| `item.crop_cauliflower` (karnabahar) | `iso_crop_cauliflower_1_se.png` | `cauliflower seedling with blue-green leaves` |
| `item.crop_cauliflower` (karnabahar) | `iso_crop_cauliflower_2_se.png` | `leafy cauliflower plant, no head yet` |
| `item.crop_cauliflower` (karnabahar) | `iso_crop_cauliflower_3_se.png` | `cauliflower with a white curd head framed by large blue-green leaves` |
| `item.crop_asparagus` (kuşkonmaz) | `iso_crop_asparagus_1_se.png` | `bare asparagus bed with one thin spear tip` |
| `item.crop_asparagus` (kuşkonmaz) | `iso_crop_asparagus_2_se.png` | `asparagus bed with feathery fern-like growth` |
| `item.crop_asparagus` (kuşkonmaz) | `iso_crop_asparagus_3_se.png` | `asparagus bed with several thick green spears rising from the soil` |
| `item.crop_artichoke` (enginar) | `iso_crop_artichoke_1_se.png` | `small rosette of silvery artichoke leaves` |
| `item.crop_artichoke` (enginar) | `iso_crop_artichoke_2_se.png` | `large rosette of long silvery jagged artichoke leaves` |
| `item.crop_artichoke` (enginar) | `iso_crop_artichoke_3_se.png` | `artichoke plant with two green globe buds on thick stems above silvery leaves` |
| `item.crop_rose` (gül) | `iso_crop_rose_1_se.png` | `small thorny rose cutting with a few leaves` |
| `item.crop_rose` (gül) | `iso_crop_rose_2_se.png` | `knee-high rose bush with dark leaves and closed buds` |
| `item.crop_rose` (gül) | `iso_crop_rose_3_se.png` | `rose bush covered in open pink damask roses` |
| `item.crop_calendula` (aynısefa) | `iso_crop_calendula_1_se.png` | `small calendula seedlings` |
| `item.crop_calendula` (aynısefa) | `iso_crop_calendula_2_se.png` | `leafy calendula plants with closed buds` |
| `item.crop_calendula` (aynısefa) | `iso_crop_calendula_3_se.png` | `calendula with bright orange daisy-like flowers` |

- Maliyet 18 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
