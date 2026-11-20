---
id: "487"
title: "Yeni çiftlik hayvanları: kaz, ada tavşanı, güvercin"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: hayvan
style_gate: false
cost_est: 8
cost_max: 14
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_goose_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_rabbit_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_pigeon_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-20
---
# 487 Kaz, ada tavşanı, güvercin

## Amaç
Belge 48 §8.3: dönemin temel kümes hayvanı kaz, tavşan yastığında (yapay yuva tümseği) yaşayan ada tavşanı, güvercinlikte güvercin.

## Tarif
375 tarifi (yol B, güney kontrolü, `fit --family hayvan`, zemin kontrolü). Renk doğal (bu türlerde genetik yok).

| # | Tür | Açıklama metni | Tuval |
|---|---|---|---|
| 1 | `animal.goose` | `white-grey domestic farm goose, orange beak and feet, long neck` | 32 |
| 2 | `animal.rabbit` | `small grey-brown wild-type rabbit (coney), upright ears, sitting` | 32 |
| 3 | `animal.pigeon` | `grey rock pigeon with iridescent green-purple neck, standing` | 32 |

- Maliyet ≈ 8. `cost_max` 14.

## Doğrulama
375 doğrulaması; yan yana: tavuk ve ördek ile boy oranı (kaz ördekten iri, güvercin tavuktan küçük).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
