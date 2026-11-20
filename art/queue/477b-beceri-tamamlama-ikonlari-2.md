---
id: "477b"
title: "Ürün ikonları: beceri tamamlama 2 (18)"
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
  - Assets/Art/Iso/_aday/icon_item_silk_thread.png
  - Assets/Art/Iso/_aday/icon_item_silk.png
  - Assets/Art/Iso/_aday/icon_item_bait.png
  - Assets/Art/Iso/_aday/icon_item_fishing_net.png
  - Assets/Art/Iso/_aday/icon_item_fish_oil.png
  - Assets/Art/Iso/_aday/icon_item_legend_fish.png
  - Assets/Art/Iso/_aday/icon_item_cart_wheel.png
  - Assets/Art/Iso/_aday/icon_item_hinge.png
  - Assets/Art/Iso/_aday/icon_item_tin_brooch.png
  - Assets/Art/Iso/_aday/icon_item_tunic.png
  - Assets/Art/Iso/_aday/icon_item_linen_shirt.png
  - Assets/Art/Iso/_aday/icon_item_quilt.png
  - Assets/Art/Iso/_aday/icon_item_dowry_linen.png
  - Assets/Art/Iso/_aday/icon_item_tulum.png
  - Assets/Art/Iso/_aday/icon_item_tulum_cheese.png
  - Assets/Art/Iso/_aday/icon_item_boots.png
  - Assets/Art/Iso/_aday/icon_item_leather_vest.png
  - Assets/Art/Iso/_aday/icon_item_harness.png
created: 2026-11-20
---
# 477b Ürün ikonları: beceri tamamlama 2/4 (18)

## Amaç
Belge 45 bölüm 9: beceri seviyeleriyle açılan yeni tarif ve kaynakların envanter, pazar ve seviye kartı ikonları. Kod ikonu ada göre bulur (`IsoArt.ItemIcon`: `icon_item_<id>`); gelene kadar boşluk çizer.

## Tarif
**420a ile aynı yöntem.**

- `item.silk_thread` (İpek ipliği): `a skein of glossy cream silk thread`
- `item.silk` (İpek kumaş): `a folded bolt of shimmering cream silk cloth`
- `item.bait` (Balık yemi): `a small clay dish of dough fishing bait balls`
- `item.fishing_net` (Büyük ağ): `a folded brown knotted fishing net with cork floats`
- `item.fish_oil` (Balık yağı): `a small clay oil flask with amber fish oil`
- `item.legend_fish` (Göllerin efsane balığı): `a huge golden-scaled pike lying on a wooden board`
- `item.cart_wheel` (Araba tekerleği): `a spoked wooden cart wheel with an iron rim`
- `item.hinge` (Menteşe): `two black forged iron strap hinges`
- `item.tin_brooch` (Cam taşlı kalay broş): `a round tin brooch set with green glass stones`
- `item.tunic` (Yün cepken): `a folded brown wool jacket`
- `item.linen_shirt` (Keten gömlek): `a folded off-white linen shirt`
- `item.quilt` (Yorgan): `a folded thick quilt with stitched diamond pattern`
- `item.dowry_linen` (İşlemeli çeyiz örtüsü): `a folded white linen cloth with red cross-stitch embroidery`
- `item.tulum` (Tulum (keçi derisi)): `a goatskin bag tied at the neck`
- `item.tulum_cheese` (Tulum peyniri): `a slice of crumbly white cheese beside a small goatskin`
- `item.boots` (Çizme): `a pair of brown leather boots`
- `item.leather_vest` (Deri yelek): `a brown leather vest with laces`
- `item.harness` (Koşum takımı): `a coiled leather horse harness with iron rings`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family arayuz <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- İkon eşlemesi ada göre kendiliğinden (icon_item_<id>).
- Kaynak listesi `tools/content/beceri45_tamamla.py` çıktısıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
