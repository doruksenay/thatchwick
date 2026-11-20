---
id: "491"
title: "Midye, inci, kırmız, çuha, kürk, gübre, çekirge, sülük ikonları (9)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: esya
style_gate: false
cost_est: 9
cost_max: 14
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_mussel.png
  - Assets/Art/Iso/_aday/icon_pearl.png
  - Assets/Art/Iso/_aday/icon_kermes.png
  - Assets/Art/Iso/_aday/icon_dye_crimson.png
  - Assets/Art/Iso/_aday/icon_scarlet_cloth.png
  - Assets/Art/Iso/_aday/icon_rabbit_fur.png
  - Assets/Art/Iso/_aday/icon_pigeon_dung.png
  - Assets/Art/Iso/_aday/icon_locust.png
  - Assets/Art/Iso/_aday/icon_leech.png
created: 2026-11-20
---
# 491 Yeni ürün ikonları

## Amaç
Belge 48 §8.3 ve §9.3: tavşan kürkü, güvercin gübresi, kırmız boyası zinciri (kırmız → kırmız boyası → iskarlat çuha), midyeden inci, çekirge yemi, sülük.

## Tarif
483 ile aynı (32x32 ikon).

| # | items.json id | Ad | Açıklama metni |
|---|---|---|---|
| 1 | `item.mussel` | Tatlı su midyesi | `dark blue-black freshwater mussel shell, closed, item icon` |
| 2 | `item.pearl` | Tatlı su incisi | `single lustrous white pearl on an open mussel shell, item icon` |
| 3 | `item.kermes` | Kırmız | `small pile of dried round dark red kermes insect grains, item icon` |
| 4 | `item.dye_crimson` | Kırmız boyası | `small clay pot of deep crimson red dye, item icon` |
| 5 | `item.scarlet_cloth` | İskarlat çuha | `folded bolt of rich scarlet red woollen broadcloth, item icon` |
| 6 | `item.rabbit_fur` | Tavşan kürkü | `soft grey-brown rabbit pelt, item icon` |
| 7 | `item.pigeon_dung` | Güvercin gübresi | `small sack of whitish pigeon droppings fertilizer, item icon` |
| 8 | `item.locust` | Çekirge | `single brown-green locust, item icon` |
| 9 | `item.leech` | Sülük | `dark olive leech in a small clay jar of water, item icon` |

## Doğrulama
483 doğrulaması; kırmız ve çuha kırmızısı palet dışı çıkarsa `--family hayvan` ile yeniden denetle.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
