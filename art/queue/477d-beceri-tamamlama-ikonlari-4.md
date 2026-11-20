---
id: "477d"
title: "Ürün ikonları: beceri tamamlama 4 (16)"
status: running
claimed_at: 2026-11-20T10:52Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("geliştirmeyi tamamla"), Project belgesi 45 bölüm 9
priority: 40
family: arayuz
style_gate: false
cost_est: 16
cost_max: 32
depends_on: ["420a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_yufka.png
  - Assets/Art/Iso/_aday/icon_item_bread_sourdough.png
  - Assets/Art/Iso/_aday/icon_item_wedding_bread.png
  - Assets/Art/Iso/_aday/icon_item_sourdough.png
  - Assets/Art/Iso/_aday/icon_item_pomegranate.png
  - Assets/Art/Iso/_aday/icon_item_pomegranate_sherbet.png
  - Assets/Art/Iso/_aday/icon_item_aged_vinegar.png
  - Assets/Art/Iso/_aday/icon_item_cough_syrup.png
  - Assets/Art/Iso/_aday/icon_item_insect_repellent.png
  - Assets/Art/Iso/_aday/icon_item_sleep_syrup.png
  - Assets/Art/Iso/_aday/icon_item_saffron_remedy.png
  - Assets/Art/Iso/_aday/icon_item_dynasty_chronicle.png
  - Assets/Art/Iso/_aday/icon_item_wheelbarrow.png
  - Assets/Art/Iso/_aday/icon_item_horse_cart.png
  - Assets/Art/Iso/_aday/icon_item_sled.png
  - Assets/Art/Iso/_aday/icon_item_covered_cart.png
created: 2026-11-20
---
# 477d Ürün ikonları: beceri tamamlama 4/4 (16)

## Amaç
Belge 45 bölüm 9: beceri seviyeleriyle açılan yeni tarif ve kaynakların envanter, pazar ve seviye kartı ikonları. Kod ikonu ada göre bulur (`IsoArt.ItemIcon`: `icon_item_<id>`); gelene kadar boşluk çizer.

## Tarif
**420a ile aynı yöntem.**

- `item.yufka` (Yufka): `a stack of thin round flatbread sheets`
- `item.bread_sourdough` (Ekşi mayalı ekmek): `a round crusty sourdough loaf scored on top`
- `item.wedding_bread` (Düğün ekmeği): `a braided round festive bread with seeds`
- `item.sourdough` (Ekşi maya): `a small clay crock of bubbly sourdough starter`
- `item.pomegranate` (Nar): `a red pomegranate split open showing seeds`
- `item.pomegranate_sherbet` (Nar şerbeti): `a clay cup of deep red pomegranate drink`
- `item.aged_vinegar` (Eski sirke): `a dark glass bottle of aged vinegar`
- `item.cough_syrup` (Öksürük şurubu): `a small clay bottle of amber syrup with a linden flower`
- `item.insect_repellent` (Böcek kovucu): `a small clay jar with herbs and resin`
- `item.sleep_syrup` (Uyku şurubu): `a small blue glass vial with a chamomile flower`
- `item.saffron_remedy` (Safranlı ilaç): `a small clay pot of golden saffron remedy`
- `item.dynasty_chronicle` (Hanedan kroniği): `a thick leather-bound book with brass corners and a family crest`
- `item.wheelbarrow` (El arabası): `a wooden wheelbarrow with one wheel`
- `item.horse_cart` (Hafif at arabası): `a light two-wheeled wooden horse cart`
- `item.sled` (Kış kızağı): `a wooden winter sled with iron runners`
- `item.covered_cart` (Üstü örtülü araba): `a wooden cart with a curved canvas cover`

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family arayuz <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ya da hayvanı yok, ortaçağ sonrası icat yok.

## Notlar
- İkon eşlemesi ada göre kendiliğinden (icon_item_<id>).
- Kaynak listesi `tools/content/beceri45_tamamla.py` çıktısıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
