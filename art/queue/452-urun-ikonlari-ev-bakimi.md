---
id: "452"
title: "Ürün ikonları: tüy, kabuk, çakıl (4)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 50
family: arayuz
style_gate: false
cost_est: 6
cost_max: 10
depends_on: ["420a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_down_feather.png
  - Assets/Art/Iso/_aday/icon_item_snail_shell.png
  - Assets/Art/Iso/_aday/icon_item_river_pebble.png
  - Assets/Art/Iso/_aday/icon_item_heron_feather.png
created: 2026-11-19
---
# 452 Ürün ikonları: tüy, kabuk, çakıl (4)

## Amaç
Belge 45: kuş tüyü yatak malzemesi, göl kıyısı koleksiyonu.

## Tarif
**420a ile aynı yöntem.**

- `item.down_feather`: `small pile of soft white goose and duck down feathers`
- `item.snail_shell`: `small striped freshwater snail shell`
- `item.river_pebble`: `three smooth coloured river pebbles`
- `item.heron_feather`: `long grey heron feather`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family arayuz <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- İkon eşlemesi ada göre kendiliğinden (icon_<id>).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
