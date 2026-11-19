---
id: "430"
title: "Yaban haritaları: maden ağzı ve saz kümesi (2)"
status: running
claimed_at: 2026-11-19T12:28Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 "diğer mapleri de üretmeye başla"
priority: 20
family: esya
style_gate: false
cost_est: 6
cost_max: 10
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_mine_mouth_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_forage_reed_se.png
created: 2026-11-19
---
# 430 Yaban haritaları: maden ağzı ve saz kümesi (2)

## Amaç
Yeni yaban haritaları (`Assets/Scripts/Core/World/RegionsWild.cs`, Project belgesi 44):
- **Maden ağzı** (Tepe ve taş ocağı, yerel 28,34): aile ocağına (kat -1) iniş. Şimdilik yerinde yalnızca taş zemin ve fener var; ocak M paneliyle açılıyor. Görsel gelince eşya tanımı (`item.mine_mouth`, 2x2, taşınamaz) ve ocak panelini açan dokunma bağlanacak.
- **Saz kümesi** (Sazlık): çatı sazı toplama düğümü (`item.forage_reed`). Eşya tanımı görselle birlikte eklenecek.

## Tarif
Yöntem 301 ile aynı; 2x2 kılavuz `--n 2 --axis both` (tuval 144x112). Dönem: ortaçağ köy madeni, ahşap destekli; elektrik ya da ray yok.
- maden ağzı: `small medieval hillside mine entrance: dark opening in a grey rock face braced with rough timber posts and a lintel, a few loose stones and a wooden bucket at the mouth`
- saz kümesi: `clump of tall common reeds with brown seed heads growing from wet mud at a marsh edge` (toplama düğümü; 311/322 toplama düğümleri ölçeğinde)

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Maden ağzının iç karanlığı tam siyah değil, koyu kahve-gri olmalı (palet).

## Notlar
Çıktılar `_aday` altında; kabul edilince `Assets/Art/Iso/` altına alınır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
