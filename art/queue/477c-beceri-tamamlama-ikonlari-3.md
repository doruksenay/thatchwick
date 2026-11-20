---
id: "477c"
title: "Ürün ikonları: beceri tamamlama 3 (18)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("geliştirmeyi tamamla"), Project belgesi 45 bölüm 9
priority: 40
family: arayuz
style_gate: false
cost_est: 18
cost_max: 36
depends_on: ["420a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_race_saddle.png
  - Assets/Art/Iso/_aday/icon_item_glass_pane.png
  - Assets/Art/Iso/_aday/icon_item_glazed_tile.png
  - Assets/Art/Iso/_aday/icon_item_scented_candle.png
  - Assets/Art/Iso/_aday/icon_item_bay_leaf.png
  - Assets/Art/Iso/_aday/icon_item_bay_soap.png
  - Assets/Art/Iso/_aday/icon_item_mine_candle.png
  - Assets/Art/Iso/_aday/icon_item_festival_candle.png
  - Assets/Art/Iso/_aday/icon_item_sealing_wax.png
  - Assets/Art/Iso/_aday/icon_item_marble.png
  - Assets/Art/Iso/_aday/icon_item_master_lantern.png
  - Assets/Art/Iso/_aday/icon_item_hund_rail.png
  - Assets/Art/Iso/_aday/icon_item_windlass.png
  - Assets/Art/Iso/_aday/icon_item_patch_quilt.png
  - Assets/Art/Iso/_aday/icon_item_old_keepsake.png
  - Assets/Art/Iso/_aday/icon_item_herb_cheese.png
  - Assets/Art/Iso/_aday/icon_item_wedding_dish.png
  - Assets/Art/Iso/_aday/icon_item_bazlama.png
created: 2026-11-20
---
# 477c Ürün ikonları: beceri tamamlama 3/4 (18)

## Amaç
Belge 45 bölüm 9: beceri seviyeleriyle açılan yeni tarif ve kaynakların envanter, pazar ve seviye kartı ikonları. Kod ikonu ada göre bulur (`IsoArt.ItemIcon`: `icon_item_<id>`); gelene kadar boşluk çizer.

## Tarif
**420a ile aynı yöntem.**

- `item.race_saddle` (Yarış eyeri): `a light leather racing saddle with fur trim`
- `item.glass_pane` (Pencere camı): `two small greenish glass window panes`
- `item.glazed_tile` (Sırlı çini): `a square blue and white glazed ceramic tile`
- `item.scented_candle` (Lavantalı mum): `a beeswax candle with a sprig of lavender`
- `item.bay_leaf` (Defne yaprağı): `a small bunch of dark green bay leaves`
- `item.bay_soap` (Defne sabunu): `a green bar of laurel soap`
- `item.mine_candle` (Uzun yanan maden mumu): `a thick tallow candle in an iron holder with a spike`
- `item.festival_candle` (Şenlik mumu): `a tall red beeswax candle with ribbon`
- `item.sealing_wax` (Mühür mumu): `a red sealing wax stick and a wax seal`
- `item.marble` (Mermer): `a white veined marble block`
- `item.master_lantern` (Usta madencinin feneri): `an iron miner's lantern with glass panes`
- `item.hund_rail` (Hund rayı (maden arabası)): `a short wooden mine rail with a small wooden cart`
- `item.windlass` (Bocurgat ve kova dolabı): `a wooden well windlass with rope and bucket`
- `item.patch_quilt` (Yamalı yorgan): `a folded colourful patchwork quilt`
- `item.old_keepsake` (Eski hatıra): `a small worn wooden box with a faded ribbon`
- `item.herb_cheese` (Otlu peynir): `a round white cheese flecked with green herbs`
- `item.wedding_dish` (Düğün yemeği (keşkek)): `a large clay bowl of creamy wheat and meat stew`
- `item.bazlama` (Bazlama): `a round thick golden flatbread`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family arayuz <dosya> ...` (`mine_candle`, `festival_candle`, `scented_candle`, `master_lantern` için `--light`). Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- İkon eşlemesi ada göre kendiliğinden (icon_item_<id>).
- Kaynak listesi `tools/content/beceri45_tamamla.py` çıktısıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
