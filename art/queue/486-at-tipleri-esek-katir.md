---
id: "486"
title: "At tipleri ve eşek ailesi: 7 gövde"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: hayvan
style_gate: false
cost_est: 17
cost_max: 28
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_horse_light_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_horse_medium_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_horse_heavy_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_pony_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_donkey_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_mule_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_hinny_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-20
---
# 486 At tipleri, eşek, katır, bardo

## Amaç
Belge 48 §7: altı at tipi dört gövdeyi paylaşır; eşek yeni tür, katır ve bardo melez. Rengi genetik verir (al, doru, yağız, kula, kır ...). Binici pozu ve eyer mevcut at görselinin kalıbını izler (104).

## Tarif
Tarif **375 ile aynı** (106 tarifi, yol B, güney kontrolü, zemin kontrolü, `fit --family hayvan`, teslim kuralı, masaüstü kopyası). Farklar:

1. **Renk paletle gelir (belge 48 §11):** her tip **tek gövde** olarak, gri-kahve nötr tonlarda üretilir; oyundaki tüy rengi (kızıl, siyah, kum, tekir ...) gövdeye palet değişimiyle uygulanır. Açıklama ekine `, neutral grey-brown coat, plain coat without markings or spots` eklenir. Desen, beyaz leke ve maske ayrı katman olarak sonra gelir (gövdeler kabul edilince ayrı iş).
2. **Açıklama eki** (375 ile aynı): `, standing directly on transparent background, no ground disc, no floor shadow, no grey patch under the paws`.
3. Her satırdaki gövde biçimi kesin uyulacak ayrımdır: tipler yan yana konunca birbirinden ayırt edilmeli (boy, bacak uzunluğu, kulak, kuyruk, tüy).

| # | Gövde | Kullanan | Açıklama metni |
|---|---|---|---|
| 1 | `horse_light` | hafif at (rahvan, koşu atı) | `light slender riding horse, long legs, refined head, arched neck` |
| 2 | `horse_medium` | orta at (beygir, kırma at) | `ordinary medieval riding horse, medium build` |
| 3 | `horse_heavy` | ağır at (koşum, yük atı) | `heavy draft horse, massive body, thick legs with feathered hooves, broad chest` |
| 4 | `pony` | midilli | `small sturdy pony, short legs, thick mane, round body` |
| 5 | `donkey` | eşek | `grey donkey with long upright ears, short upright mane, dark dorsal stripe, small hooves, tufted tail` |
| 6 | `mule` | katır | `mule: horse-sized body with long donkey ears, short mane, slim legs, donkey-like tufted tail` |
| 7 | `hinny` | bardo | `hinny: smaller than a mule, horse-like head with medium ears, fuller mane and tail` |

- Tuval: atlar ve katır 56, midilli ve eşek 44, bardo 48. Maliyet ≈ 17. `cost_max` 28.

## Doğrulama
375 doğrulaması. Yan yana: 7 gövde + mevcut at (104). Katır uzun kulağıyla attan, bardo kısa kulak ve dolgun yelesiyle katırdan ayrılmalı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
