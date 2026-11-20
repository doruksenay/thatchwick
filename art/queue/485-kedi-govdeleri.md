---
id: "485"
title: "Kedi gövdeleri: 3 boy × kısa/uzun tüy (6)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: hayvan
style_gate: false
cost_est: 13
cost_max: 22
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_cat_medium_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_medium_long_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_large_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_large_long_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_slim_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_slim_long_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-20
---
# 485 Kedi gövdeleri

## Amaç
Belge 48 §5: altı kedi tipi (ambar, orman, kıyı, Van, Ankara, kitaplık) üç gövdeyi paylaşır; uzun tüy genle gelir (`L` lokusu), bu yüzden her gövdenin uzun tüylü eşi var. Tekir, kızıl, bağa ve beyaz leke palet ve katmanla gelir.

## Tarif
Tarif **375 ile aynı** (106 tarifi, yol B, güney kontrolü, zemin kontrolü, `fit --family hayvan`, teslim kuralı, masaüstü kopyası). Farklar:

1. **Renk paletle gelir (belge 48 §11):** her tip **tek gövde** olarak, gri-kahve nötr tonlarda üretilir; oyundaki tüy rengi (kızıl, siyah, kum, tekir ...) gövdeye palet değişimiyle uygulanır. Açıklama ekine `, neutral grey-brown coat, plain coat without markings or spots` eklenir. Desen, beyaz leke ve maske ayrı katman olarak sonra gelir (gövdeler kabul edilince ayrı iş).
2. **Açıklama eki** (375 ile aynı): `, standing directly on transparent background, no ground disc, no floor shadow, no grey patch under the paws`.
3. Her satırdaki gövde biçimi kesin uyulacak ayrımdır: tipler yan yana konunca birbirinden ayırt edilmeli (boy, bacak uzunluğu, kulak, kuyruk, tüy).

| # | Gövde | Kullanan tip | Açıklama metni |
|---|---|---|---|
| 1 | `cat_medium` | orta gövde (ambar, kitaplık) | `ordinary medium domestic cat, short coat, upright tail` |
| 2 | `cat_medium_long` | orta gövde, uzun tüy | `ordinary medium domestic cat with long fluffy coat and bushy tail` |
| 3 | `cat_large` | iri gövde (orman, Van) | `large sturdy cat, broad chest, thick legs, short coat` |
| 4 | `cat_large_long` | iri gövde, uzun tüy | `large sturdy forest cat with thick long coat, ear tufts, very bushy tail` |
| 5 | `cat_slim` | ince gövde (kıyı, Ankara) | `slim elegant cat, long legs, narrow head, short coat` |
| 6 | `cat_slim_long` | ince gövde, uzun tüy | `slim elegant cat with long silky coat and plumed tail` |

- Tuval 32 (kedi). Maliyet ≈ 13. `cost_max` 22.

## Doğrulama
375 doğrulaması. Yan yana: 6 gövde + kabul edilen `cat_1`. Kısa ve uzun tüylü eşler aynı duruşta olmalı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
