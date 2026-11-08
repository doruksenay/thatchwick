---
id: "006"
title: Söndürülmüş ocak taşı
status: pending
approved_by: pending
cost_est: 1
cost_max: 2
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_hearth_stone_unlit_se.png
created: 2026-11-08
---
# 006 Söndürülmüş ocak taşı

## Amaç
Tur 3'teki ocak (şimdiki adı `iso_item_hearth_stone_lit_se.png`) "ateş yok" denmesine rağmen sabit çizili alevle geldi (62 renk) ve zaten yanık sürüm olarak adlandırıldı. Isı mekaniğinde ocağın sönük durumu gerekir; sönük sürüm ve ayrı ateş efekti ayrı olmalı.

`approved_by: pending`: kullanıcı kararı bekliyor. İstenirse `approved_by: user` yap. Var olan dosya silinmez ya da üzerine yazılmaz. Ad, mevcut `_lit_se` ile eşleşir.

## Tarif
- Araç: `create_map_object`, stil eşleme + 1x1 maske (tur 3 yöntemi).
- Kılavuz ve maske: `python tools/art/footprint_guide.py --w 80 --h 56 --n 1 --axis x --height 14 --out <geçici>/hearth`. Footprint x 8–71, y 16–47; pivot (40, 24).
- Açıklama: taş halka ocak, **kül ve kömür, ateş yok, parıltı yok**, düz az tonlu. Referans: `iso_item_hearth_stone_lit_se.png` (taş kısmı).

## Doğrulama
`validate_iso.py --footprint 8,16,64,32 --max-colors 40`, maske dışı opak 0, yan yüz ışık sırası (yan yüzler 2-3 px olduğu için sayı belirsizse söyle), alev/parıltı pikseli yok.

## Notlar
Ateş efekti (animasyon) sonraki tur.
