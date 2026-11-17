---
id: "337"
title: Halılar (6) ve kiler kapağı
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (Sims düzeyinde yapı modu, 2026-11-17); "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 8
cost_max: 12
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_rug_rush_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_wool_small_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_runner_3x1_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_hide_1x2_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_wool_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_rug_woven_large_3x2_se.png
  - Assets/Art/Iso/_aday/iso_item_cellar_hatch_2x1_se.png
created: 2026-11-17
---
# 337 Halılar ve kiler kapağı

## Amaç
Yeni halı katmanı (`placement: Rug`): yere serilir, yolu kapatmaz, üstüne mobilya konur. Halılar `finishGroup: cloth` (hasır kilim ve post hariç): renkler motorda boyanacağı için halılar **ham, açık tonlu** üretilir. Kiler kapağı (`item.cellar_hatch`) evin dışından bodruma inen ahşap kapaktır (1x2, merdiven -1).

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı). Halılar düz, yere yapışık, neredeyse yüksekliksiz (en çok 2 px kalınlık), gölgesiz.
- hasır kilim (1x1) = woven rush mat; küçük yün halı (2x1) = small undyed wool rug with simple border; yolluk (3x1) = long narrow runner; post (1x2) = sheepskin hide; yün halı (2x2) = undyed wool rug with border pattern; desenli dokuma (3x2) = large woven rug with geometric medieval pattern; kiler kapağı (1x2) = slanted wooden cellar door doors set in a stone frame in the ground.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
