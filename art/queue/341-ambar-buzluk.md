---
id: "341"
title: Ambar, buzluk, buzhane
status: review
claimed_at: 2026-11-18T12:07Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 20
family: esya
style_gate: false
cost_est: 5
cost_max: 8
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_granary_3x2_se.png
  - Assets/Art/Iso/_aday/iso_item_ice_box_se.png
  - Assets/Art/Iso/_aday/iso_item_ice_house_2x2_se.png
created: 2026-11-17
---
# 341 Ambar ve soğuk depo

## Amaç
Yiyecek depoya otomatik yerleşir: en çabuk bozulan en soğuk yere. Ambar büyük ve havadardır (x0,85), buzluk ve buzhane buz varken çok soğuktur (x0,2). Buz kışın kuyudan ya da su yalağından kesilir. Ahşap olanlar ham, açık tonlu (`finishGroup: wood`).

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı).
- ambar (3x2) = raised wooden granary on stone stilts with a thatched roof and a small ladder (Karadeniz serenderi gibi); buzluk (1x1) = wooden ice chest with iron bands, lid slightly open, ice blocks and straw inside; buzhane (2x2) = small half-buried stone ice house with a thatched conical roof and a wooden door.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-18: yeniden `pending` (Claude, kurtarma): 2026-11-18T09:53Z sahiplenmesinden sonuç gelmedi.

## Sorular
- 2026-11-18 (bulut işçisi): `get_balance` plan olarak "Tier 2: Pixel Artisan", 4999/5000 generation (yenilenme 2026-12-18) döndürdü; `art/budget.md` Tier 1 / 2000 diyor. İşçi kuralı gereği plan Tier 1 değilse durur. Üretim yapılmadı (0 gen). Kullanıcı `art/budget.md` planını güncelleyince işi `pending` yapın.
- 2026-11-18 (tamir): `art/budget.md` planı Tier 2 olarak güncellendi, ajan kuralı "Tier 1 değilse dur" yerine "budget.md planıyla uyuşmuyorsa dur" oldu; iş yeniden `pending`.

- 2026-11-18 (bulut işçisi): ambar ve buzluk teslim (buzluk palet UYARI, ambar ayak izini ~%60 dolduruyor). Buzhane çimen zemin pişirdiği ve stil denetiminde KALDI olduğu için teslim edilmedi (`art/results/341-red/`); tek deneme hakkı bitti, yeniden üretim için karar gerek. Rapor: `art/results/341-ambar-buzluk.md`.
