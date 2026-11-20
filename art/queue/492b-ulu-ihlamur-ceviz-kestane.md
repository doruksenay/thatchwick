---
id: "492b"
title: "Ulu ıhlamur, ulu ceviz, ulu kestane; dört mevsim"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026, Project belgesi 52
priority: 10
family: bitki
style_gate: false
cost_est: 12
cost_max: 24
depends_on: ["492a2"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_greattree_linden_{spring,summer,autumn,winter}_se.png
  - Assets/Art/Iso/_aday/iso_greattree_walnut_{spring,summer,autumn,winter}_se.png
  - Assets/Art/Iso/_aday/iso_greattree_chestnut_{spring,summer,autumn,winter}_se.png
created: 2026-11-20
---
# 492b Ulu ıhlamur, ceviz, kestane

## Amaç
Belge 52 §2: ırmak kıyısında ulu ıhlamur (Pitchford türü, 17. yüzyıl ağaç evinin ağacı), köy yolunda ulu ceviz, ormanda ulu kestane.

## Tarif
**492a ile aynı yöntem ve ölçü** (2x2, 160x224, pivot (80, 36), 190-215 px; kullanıcı kararı 2026-11-20 21:16: seçenek (b) "Stil eşlemeyle 160x224 ... bu olsun"; mevsimler aynı gövdeyle). Stil referansı 492a'nın yaz görseli.

| Tür | Yaz açıklaması | Mevsim farkı |
|---|---|---|
| `linden` | `ancient huge large-leaved linden (lime) tree, tall straight thick trunk with many burrs, dense rounded crown of heart-shaped leaves` | yaz: soluk sarı çiçek salkımları; güz: sarı; kış: çıplak, ince sık dallar |
| `walnut` | `ancient huge walnut tree, broad grey furrowed trunk, wide airy crown of long compound leaves` | güz: sararan yaprak ve yeşil ceviz kabukları; kış: çıplak kalın dallar |
| `chestnut` | `ancient huge sweet chestnut tree, massive spiralling twisted bark, broad crown of long serrated leaves` | yaz: krem rengi püsküllü çiçek; güz: dikenli yeşil kabuklar ve kahve yaprak; kış: çıplak |

- Maliyet ≈ 12 gen (3 tür × 4 mevsim).

- 2026-11-21 (tester raporu 66): pilot 492a başarısız; bağımlılık 492a2. Mevsimler 492a2 gibi `edit_image_pro_flash` ile yaz görselinden, açıklamada ağaç evi/platform yok. Ölçek: `tools/art/scale_check.py` (2x2: 185-215 px, çınar: 165-190 px).

## Doğrulama
492a doğrulaması. Ek: dört ulu ağaç (492a dahil) yan yana: taç biçimi ve rengiyle ayırt ediliyor mu.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
