---
id: "494"
title: "Ürün ikonları: ağaç ev iskeletleri (5) ve ceviz"
status: review
claimed_at: 2026-11-20T11:11Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026, Project belgesi 52
priority: 40
family: arayuz
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["420a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_th_kit_{nest,lookout,fort,hut,kiosk}.png
  - Assets/Art/Iso/_aday/icon_item_walnut.png
created: 2026-11-20
---
# 494 Ağaç ev iskeleti ve ceviz ikonları

## Amaç
Belge 52: marangoz tezgâhında yapılan ağaç ev iskeletleri (ulu ağacın dibinde "Ağaç ev kur") ve ulu cevizden toplanan ceviz. Kod ikonu ada göre bulur (`IsoArt.ItemIcon`: `icon_item_<id>`).

## Tarif
**420a ile aynı yöntem.**

- `item.th_kit_nest` (Dal Yuvası iskeleti): `a small bundle of rough planks tied with rope, a rolled rope ladder on top`
- `item.th_kit_lookout` (Gözcü Kulesi iskeleti): `a bundle of long wooden poles and planks tied with rope, a few shingles`
- `item.th_kit_fort` (Tahta Kale iskeleti): `a stack of planks with notched tops and a small folded cloth pennant`
- `item.th_kit_hut` (Saz Damlı Kulübe iskeleti): `a stack of planks with a tied bundle of golden straw thatch`
- `item.th_kit_kiosk` (Ağaç Köşkü iskeleti): `a stack of carved timber beams with a few red clay roof tiles and an iron bracket`
- `item.walnut` (Ceviz): `three brown walnuts, one cracked open showing the kernel`

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
