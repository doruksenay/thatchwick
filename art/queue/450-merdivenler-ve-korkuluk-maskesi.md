---
id: "450"
title: "Merdivenler ve korkuluk maskesi (4 + 4 maske)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 16
cost_max: 24
depends_on: ["302"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_stairs_up_wood_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_up_wood_1x2_se_accent.png
  - Assets/Art/Iso/_aday/iso_item_stairs_down_wood_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_down_wood_1x2_se_accent.png
  - Assets/Art/Iso/_aday/iso_item_stairs_up_stone_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_up_stone_1x2_se_accent.png
  - Assets/Art/Iso/_aday/iso_item_stairs_down_stone_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_down_stone_1x2_se_accent.png
created: 2026-11-19
---
# 450 Merdivenler ve korkuluk maskesi (4 + 4 maske)

## Amaç
Belge 45 §10 (kullanıcı: "aynı merdivenin siyahı, korkulukları farklı bir kırmızı"). Merdivenin gövdesi kaplama rengini, korkuluğu ikinci rengi alır.

## Tarif
302 ölçeği, 1x2 tek yön `_se` (uzun eksen "\", 390 gibi). Korkuluk açık gri (#C8C8C8) çizilir ki maske ayrılsın.
Her görselden sonra maske: `python tools/art/accent_mask.py <dosya> --key "#C8C8C8" --tol 40` → `<ad>_accent.png` (betik saydam zemin üstüne açık gri yazar). Maske 12 pikselden azsa anahtar rengi görselden seçilir ve yeniden denenir.

| # | items.json id | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.stairs_up` | `iso_item_stairs_up_wood_1x2_se.png` | 1x2 | `wooden staircase going up with a wooden handrail on one side, handrail painted pale grey #C8C8C8` |
| 2 | `item.stairs_down` | `iso_item_stairs_down_wood_1x2_se.png` | 1x2 | `wooden stairs going down into a cellar opening with a handrail painted pale grey #C8C8C8` |
| 3 | `item.stairs_up_stone` | `iso_item_stairs_up_stone_1x2_se.png` | 1x2 | `stone staircase going up with a thin iron handrail painted pale grey #C8C8C8` |
| 4 | `item.stairs_down_stone` | `iso_item_stairs_down_stone_1x2_se.png` | 1x2 | `stone steps going down into a cellar with an iron handrail painted pale grey #C8C8C8` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- IsoArt eşlemesi kod oturumunda eklendi; `_accent` dosyası varsa oyun ikinci rengi onunla çizer.
- Teslim `cp -n`, `.meta` yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
