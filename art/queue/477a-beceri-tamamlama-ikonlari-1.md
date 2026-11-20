---
id: "477a"
title: "Ürün ikonları: beceri tamamlama 1 (18)"
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
  - Assets/Art/Iso/_aday/icon_item_dill.png
  - Assets/Art/Iso/_aday/icon_item_fenugreek.png
  - Assets/Art/Iso/_aday/icon_item_lavender.png
  - Assets/Art/Iso/_aday/icon_item_seed_herb_bed.png
  - Assets/Art/Iso/_aday/icon_item_saffron_corm.png
  - Assets/Art/Iso/_aday/icon_item_bone.png
  - Assets/Art/Iso/_aday/icon_item_bone_meal.png
  - Assets/Art/Iso/_aday/icon_item_coffee_spent.png
  - Assets/Art/Iso/_aday/icon_item_seed_vetch.png
  - Assets/Art/Iso/_aday/icon_item_fish_meal.png
  - Assets/Art/Iso/_aday/icon_item_thyme.png
  - Assets/Art/Iso/_aday/icon_item_truffle.png
  - Assets/Art/Iso/_aday/icon_item_tar.png
  - Assets/Art/Iso/_aday/icon_item_fine_lumber.png
  - Assets/Art/Iso/_aday/icon_item_folk_almanac.png
  - Assets/Art/Iso/_aday/icon_item_aged_cheese.png
  - Assets/Art/Iso/_aday/icon_item_mulberry_leaf.png
  - Assets/Art/Iso/_aday/icon_item_cocoon.png
created: 2026-11-20
---
# 477a Ürün ikonları: beceri tamamlama 1/4 (18)

## Amaç
Belge 45 bölüm 9: beceri seviyeleriyle açılan yeni tarif ve kaynakların envanter, pazar ve seviye kartı ikonları. Kod ikonu ada göre bulur (`IsoArt.ItemIcon`: `icon_item_<id>`); gelene kadar boşluk çizer.

## Tarif
**420a ile aynı yöntem.**

- `item.dill` (Dereotu): `a bunch of feathery green dill sprigs`
- `item.fenugreek` (Çemen): `a small cloth pouch of yellow-brown fenugreek seeds`
- `item.lavender` (Lavanta): `a tied bundle of purple lavender stems`
- `item.seed_herb_bed` (Mutfak otu tohumu): `a small linen seed pouch with a green herb leaf mark`
- `item.saffron_corm` (Safran soğanı): `three brown papery saffron crocus corms`
- `item.bone` (Kemik): `two plain white animal bones`
- `item.bone_meal` (Kemik unu): `a small sack of pale grey bone meal powder`
- `item.coffee_spent` (Kahve telvesi): `a small heap of dark brown spent coffee grounds on a wooden dish`
- `item.seed_vetch` (Fiğ tohumu): `a small linen pouch of round dark vetch seeds`
- `item.fish_meal` (Balık unu): `a small sack of greyish fish meal`
- `item.thyme` (Kekik): `a bundle of small-leaved wild thyme`
- `item.truffle` (Yer mantarı): `a knobbly black truffle with soil crumbs`
- `item.tar` (Katran): `a small clay pot of black pine tar with a wooden stick`
- `item.fine_lumber` (Kaliteli kereste): `two straight pale quarter-sawn oak boards`
- `item.folk_almanac` (Halk takvimi): `a small leather-bound almanac book with a sun and moon on the cover`
- `item.aged_cheese` (Olgun peynir): `a round aged cheese wheel with a dark rind, one wedge cut`
- `item.mulberry_leaf` (Dut yaprağı): `a small pile of broad green mulberry leaves`
- `item.cocoon` (İpek kozası): `three white silk cocoons`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family arayuz <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- İkon eşlemesi ada göre kendiliğinden (icon_item_<id>).
- Kaynak listesi `tools/content/beceri45_tamamla.py` çıktısıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
