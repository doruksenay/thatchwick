---
id: "498"
title: "Easter egg: Kılıçtan Saban masa üstü dekoru (1)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet ("ekle kodu"), Project belgesi 55 §4, kod PR #89
priority: 50
family: esya
style_gate: false
cost_est: 1
cost_max: 3
depends_on: ["383"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_sword_ploughshare_se.png
created: 2026-11-20
---
# 498 Easter egg: Kılıçtan Saban masa üstü dekoru

## Amaç
Bulunan paslı kılıç demirci ocağında saban demirine dövülür (Project belgesi 55 §4). Kodda (`item.sword_ploughshare`, PR #89) masa ve raf üstüne konan küçük bir dekor; envanter ikonu 495'te (`icon_item_sword_ploughshare`). Bu iş dünyadaki görünüşü.

## Tarif
Masa üstü **383 ile aynı yöntem** (yuva K).

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.sword_ploughshare` | `iso_item_sword_ploughshare_se.png` | yuva K | 32x24 | 16, 8 | `small iron ploughshare blade lying flat, a worn sword hilt shape still faintly visible at one end, dark forged iron` |

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`. Dönem: saban demiri ortaçağ nesnesi; keskin, tehditkâr bir silah gibi görünmemeli (artık alet).

## Notlar
- Kod görseli adla bulur (`items.json` `art.alongX`); dosya adı değişmesin.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
