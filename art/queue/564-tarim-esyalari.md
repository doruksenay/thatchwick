---
id: "564"
title: "Tarım eşyaları: tohum sandıkları, imbik, fidelik, saban ve diğerleri (12)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 30
family: esya
style_gate: false
cost_est: 12
cost_max: 24
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_seed_chest_1_se.png
  - Assets/Art/Iso/_aday/iso_item_seed_chest_2_se.png
  - Assets/Art/Iso/_aday/iso_item_seed_chest_3_se.png
  - Assets/Art/Iso/_aday/iso_item_alembic_se.png
  - Assets/Art/Iso/_aday/iso_item_seedling_tray_se.png
  - Assets/Art/Iso/_aday/iso_item_hotbed_se.png
  - Assets/Art/Iso/_aday/iso_item_plough_se.png
  - Assets/Art/Iso/_aday/iso_item_owl_box_se.png
  - Assets/Art/Iso/_aday/iso_item_crop_stake_se.png
  - Assets/Art/Iso/_aday/iso_item_harvest_doll_wall_se.png
  - Assets/Art/Iso/_aday/iso_item_scarecrow_dressed_se.png
  - Assets/Art/Iso/_aday/iso_item_village_seed_store_se.png
created: 2026-11-20
---
# 564 Tarım eşyaları: tohum sandıkları, imbik, fidelik, saban ve diğerleri (12)

## Amaç
Project belgeleri 59 §4.5, §7 ve 59a §7: tohum sandığı (üç kademe), imbik (gül suyu), fidelik ve sıcak yastık, saban, baykuş yuvası, ekin sırığı, hasat bebeği, giydirilmiş korkuluk, köy tohum ambarı.

## Tarif
Eşya dalgasının yöntemi (301/302): `create_map_object` stil eşleme, `tools/art/footprint_guide.py` kılavuzu ve maskesi, aile `esya`. Duvar eşyası için duvar eşyası yöntemi (pivot ve yükseklik mevcut duvar dekorlarıyla aynı).

| Dosya | Ayak izi | Açıklama |
|---|---|---|
| `iso_item_seed_chest_1_se.png` | 1x1 | `simple low wooden seed chest with small drawers and cloth labels` |
| `iso_item_seed_chest_2_se.png` | 1x1 | `taller wooden seed cabinet with many small labelled drawers and iron pulls` |
| `iso_item_seed_chest_3_se.png` | 2x1 | `wide carved seed cabinet with dozens of small labelled drawers and a shelf of clay seed jars` |
| `iso_item_alembic_se.png` | 1x1 | `small copper alembic still on a low brick hearth with a curved spout and a clay receiving flask` |
| `iso_item_seedling_tray_se.png` | 1x1 | `shallow wooden tray of soil with rows of tiny green seedlings` |
| `iso_item_hotbed_se.png` | 1x1 | `low wooden frame filled with dark steaming manure topped with soil and young seedlings` |
| `iso_item_plough_se.png` | 2x1 | `medieval wooden ard plough with an iron share and a draft yoke lying on the ground` |
| `iso_item_owl_box_se.png` | 1x1 | `wooden owl nesting box on a tall post` |
| `iso_item_crop_stake_se.png` | 1x1 | `three thin wooden stakes tied at the top like a small tripod for climbing plants` |
| `iso_item_harvest_doll_wall_se.png` | duvar | `woven straw harvest doll with a red ribbon hanging on a wall peg` |
| `iso_item_scarecrow_dressed_se.png` | 1x1 | `scarecrow on a wooden cross wearing a patched tunic, a straw hat and a scarf` |
| `iso_item_village_seed_store_se.png` | 2x2 | `small stone-and-timber village seed granary on staddle stones with a thatched roof and a ladder` |

- Maliyet 12 × 1 gen; ikinci deneme payıyla `cost_max` 24.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Tohum sandığı kademeleri mobilya takımlarıyla uyumlu olsun: 1 Köylü, 2 Zanaatkâr, 3 Tüccar.
- Köy tohum ambarı köy meydanında durur (köy projesi).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
