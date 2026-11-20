---
id: "725"
title: "Taklit oyuncakları (4)"
status: review
claimed_at: 2026-11-20T15:22Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 30
family: esya
style_gate: false
cost_est: 4
cost_max: 8
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_toy_pot_set_se.png
  - Assets/Art/Iso/iso_item_toy_hammer_se.png
  - Assets/Art/Iso/iso_item_toy_hoe_se.png
  - Assets/Art/Iso/iso_item_toy_sheep_se.png
created: 2026-11-20
---
# 725 Taklit oyuncakları (4)

## Amaç
Çocuk taklidi (Project belgesi 60b §5.6): çocuk, işini izlediği yetişkini oyuncağıyla taklit eder. Dört küçük oyuncak; masa ve rafa konur (bez bebek `iso_item_rag_doll` ölçüsünde).

## Tarif
Eşya dalgasının yöntemi (301/302): `create_map_object` stil eşleme, `tools/art/footprint_guide.py` kılavuzu ve maskesi, aile `esya`. Masa üstü eşyalar masa üstü dekor ölçüsünde (meyve sepeti `iso_item_fruit_basket`), duvar eşyası duvar eşyası yönteminde.

| Dosya | Ayak izi | Açıklama |
|---|---|---|
| `iso_item_toy_pot_set_se.png` | masa üstü | `tiny wooden toy cooking pot with a lid and a little spoon` |
| `iso_item_toy_hammer_se.png` | masa üstü | `small wooden toy mallet with a round head` |
| `iso_item_toy_hoe_se.png` | masa üstü | `small child-size wooden hoe lying flat` |
| `iso_item_toy_sheep_se.png` | masa üstü | `small stuffed rag sheep with stubby legs, stitched cloth, no face detail` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Işık kaynağı kuralı geçerli.

## Notlar
Eşya kimlikleri kodda bu adlarla açıldı (item.toy_pot_set, item.toy_hammer, item.toy_hoe, item.toy_sheep); pivotlar `IsoPivots.txt`'e kod oturumunda yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
