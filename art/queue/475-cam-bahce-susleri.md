---
id: "475"
title: "Cam bahçe süsleri: taş, kabuk, odun, mercan (10)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe: balık, taş, yosun, kabuk), Project belgesi 47
priority: 20
family: esya
style_gate: false
cost_est: 10
cost_max: 20
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/viv_mossy_stone.png
  - Assets/Art/Iso/_aday/viv_flat_stone.png
  - Assets/Art/Iso/_aday/viv_bark.png
  - Assets/Art/Iso/_aday/viv_driftwood.png
  - Assets/Art/Iso/_aday/viv_pot_shard.png
  - Assets/Art/Iso/_aday/viv_whelk.png
  - Assets/Art/Iso/_aday/viv_coral.png
  - Assets/Art/Iso/_aday/viv_river_pebble.png
  - Assets/Art/Iso/_aday/viv_mussel_shell.png
  - Assets/Art/Iso/_aday/viv_snail_shell.png
created: 2026-11-20
---
# 475 Cam bahçe süsleri: taş, kabuk, odun, mercan (10)

## Amaç
Belge 47 (kullanıcı 20.11): cam bahçenin içine konan süsler. Oyuncu tabanda sağa sola dizer; böcek ve balıklar bunların ardına saklanır, kertenkele yassı taşta güneşlenir, keşiş yengeci boş kabuğa girer.

## Tarif
`create_image_pixen` (24x16 ve 32x24), `no_background=true`, `view=side`, yan görünüm, ayaklar ya da taban alt kenarda (pivot alttan ortada). Canlılar **sağa bakar** (sola kod aynalar). Süsler önden ve hafif yukarıdan; taban düz, alt kenarda.
Aile `esya`: 470-473 raporundaki ders, `esya` paleti kahverengi ağırlıklı olduğu için yeşil ve renkli türler palet dışı kaldı; bu iş kendi ailesinin referanslarıyla denetlenir.

| # | items.json id | Dosya | Boyut | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.viv_mossy_stone` | `viv_mossy_stone.png` | 32x24 | `rounded grey stone with green moss on top` |
| 2 | `item.viv_flat_stone` | `viv_flat_stone.png` | 32x24 | `wide flat grey stone slab` |
| 3 | `item.viv_bark` | `viv_bark.png` | 32x24 | `curved piece of dark tree bark forming a small cave` |
| 4 | `item.viv_driftwood` | `viv_driftwood.png` | 32x24 | `pale weathered driftwood branch lying down` |
| 5 | `item.viv_pot_shard` | `viv_pot_shard.png` | 24x16 | `curved terracotta pot shard lying on its side, open like a cave` |
| 6 | `item.viv_whelk` | `viv_whelk.png` | 24x16 | `empty cream whelk shell` |
| 7 | `item.viv_coral` | `viv_coral.png` | 24x16 | `small branch of red Mediterranean coral` |
| 8 | `item.river_pebble` | `viv_river_pebble.png` | 24x16 | `handful of colourful smooth river pebbles` |
| 9 | `item.mussel_shell` | `viv_mussel_shell.png` | 24x16 | `open dark blue freshwater mussel shell` |
| 10 | `item.snail_shell` | `viv_snail_shell.png` | 24x16 | `empty brown spiral snail shell` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`, yan yana: `art/results/475-yanyana.png`. Dönem kuralları (CLAUDE.md, Project belgesi 20): yalnızca Avrupa ve Akdeniz türleri.

## Notlar
Kod görseli adla bulur (`IsoArt.Get("viv_<ad>")`), gelene kadar renkli şekil çizer; dosya adı değişmesin. 472-473'te teslim edilmeyenlerin yeniden denemesi de burada (ham hâlleri `art/results/47x-red/`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
