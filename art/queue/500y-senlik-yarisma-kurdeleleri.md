---
id: "500y"
title: "Şenlik ürün yarışması kurdeleleri (2 hatıra eşyası)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerini uygula"), Project belgesi 57
priority: 30
family: esya
style_gate: false
cost_est: 3
cost_max: 6
depends_on: ["403"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_show_ribbon_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_show_rosette_sw_ne.png
created: 2026-11-20
---
# 500y Şenlik ürün yarışması kurdeleleri

## Amaç
Şenlik ürün yarışmaları (Project belgesi 57): her şenlikteki tadımda birinci olan hane **tadım kurdelesi**, Hasat Şenliği sergisinde birinci olan hane **sergi rozeti** kazanır. İkisi de hatıra (`keepsake`): satılmaz, rafa ya da duvara konur, üstünde yarışma ve yıl yazar. `items.json`'da var (`art.pending: true`), görsel gelene kadar yer tutucu.

## Tarif
**403 ile aynı** (hatıralar): masa ve raf üstü eşya 301 ölçeğinde küçük tuval (`_se`), duvar eşyası 306 ile aynı (`sw_ne`, 1 kenar, 40x40). Teslim `cp -n`, `.meta` yok. `harvest_ribbon` (403) ile karışmasın: o buğday demetli kırmızı rozet; bunlar şenlik renkli (çivit ve buğday sarısı).

| # | items.json id | Dosya (`_aday/`) | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.show_ribbon` | `iso_item_show_ribbon_se.png` | yuva K | `small indigo blue cloth ribbon with two tails and a round cream linen center stitched with a tiny gold star, lying flat` |
| 2 | `item.show_rosette` | `iso_wall_item_show_rosette_sw_ne.png` | 1 kenar | `large pleated rosette of indigo and wheat-gold cloth with three long ribbon tails, pinned on a small wooden board` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem: kumaş rozet ve kurdele ortaçağ panayırında bilinir; yazı yok, madalya yok.

## Notlar
- `items.json`'da `art.alongX` adları hazır; kabul edilince `pending` kaldırılır (kod oturumunun işi). Pivotlar `Assets/Editor/IsoPivots.txt`'e.
- Kimlik harfli (`500y`): sayı kimlikleri başka oturumlarla çakıştı (497).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
