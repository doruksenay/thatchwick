---
id: "781"
title: "Küçük çiftlik hayvanları ölçek seti: kedi 2, tavuk (3), kaz (3), ördek, tavşan, güvercin (8 yön)"
status: running
claimed_at: 2026-11-21T09:21Z
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet ("(b) hepsi birlikte"); tester raporu 66
priority: 40
family: hayvan
style_gate: false
cost_est: 20
cost_max: 34
depends_on: ["780"]
max_attempts: 1
pixellab: true
supersedes:
  - Assets/Art/Iso/_aday/iso_animal_cat_2_cand_*.png
  - Assets/Art/Iso/_aday/iso_animal_chicken_{3,4,5}_cand_*.png
  - Assets/Art/Iso/_aday/iso_animal_goose_cand_*.png
  - Assets/Art/Iso/_aday/iso_animal_goose_{1,3}_cand_*.png
  - Assets/Art/Iso/_aday/iso_animal_rabbit_cand_*.png
  - Assets/Art/Iso/_aday/iso_animal_pigeon_cand_*.png
  - Assets/Art/Iso/_aday/iso_animal_sheep_1_cand_*.png   # 100'de reddedildi, ölçek dışı (25 px); oyun artık sheep_2 kullanır
outputs:
  - Assets/Art/Iso/_aday/iso_animal_{cat_2,chicken_3,chicken_4,chicken_5,goose,goose_1,goose_3,duck,rabbit,pigeon}_v2_{se,sw,nw,ne,s,w,n,e}.png
  - art/results/781-*.md
created: 2026-11-21
---
# 781 Küçük çiftlik hayvanları ölçek seti

## Amaç
Ölçek tablosunun dışında kalan küçük çiftlik hayvanları (tester raporu 66; `python tools/art/scale_check.py` ölçümü): kedi 2 28 px, tavuklar 24-30 px, kazlar 28-30 px, tavşan 28 px, güvercin 30 px. Ördeğin görseli hiç yok (oyunda küp çiziliyor). Hepsi 780'in küçük gövde yöntemiyle yeniden çizilir.

## Tarif
**780 tarifi** (küçük gövde yöntemi, kırpma kuralı, dosya adı, boy ölçümü ve 40'lık yineleme kuralı). Farklar:

| # | Gövde | Hedef boy | Maske (en x boy) | Yol | Açıklama metni (+ 780 ortak ekleri) | Stil referansı (eski SE) |
|---|---|---|---|---|---|---|
| 1 | `cat_2` | 16-24 | 22 x 20 | quadruped, `cat` | `small farm cat seen straight from the front facing the viewer, black coat, standing on all four legs` | `iso_animal_cat_2_cand_se` |
| 2 | `chicken_3` | 15-22 | 18 x 18 | B' kuş | eski SE ile aynı tavuk: renk ve ibik eski görselden (açıklamayı ona göre yaz: `medieval farm hen seen straight from the front facing the viewer, ...`) | `iso_animal_chicken_3_cand_se` |
| 3 | `chicken_4` | 15-22 | 18 x 18 | B' kuş | aynı kural | `iso_animal_chicken_4_cand_se` |
| 4 | `chicken_5` | 15-22 | 18 x 19 | B' kuş | aynı kural (horoz ise kuyruk ve ibik korunur) | `iso_animal_chicken_5_cand_se` |
| 5 | `goose` | 17-26 | 18 x 22 | B' kuş | aynı kural (kaz: uzun boyun, turuncu gaga) | `iso_animal_goose_cand_se` |
| 6 | `goose_1` | 17-26 | 18 x 22 | B' kuş | aynı kural | `iso_animal_goose_1_cand_se` |
| 7 | `goose_3` | 17-26 | 18 x 22 | B' kuş | aynı kural | `iso_animal_goose_3_cand_se` |
| 8 | `duck` | 17-26 | 18 x 19 | B' kuş | `medieval farm duck seen straight from the front facing the viewer, brown speckled mallard-type hen, flat orange bill, short legs` | `iso_animal_goose_1_cand_se` (palet) |
| 9 | `rabbit` | 11-17 | 16 x 15 | quadruped, `dog` | eski SE ile aynı tavşan, oturur değil dört ayak üstünde (`... standing on all four legs, ears up`) | `iso_animal_rabbit_cand_se` |
| 10 | `pigeon` | 9-16 | 14 x 13 | B' kuş | eski SE ile aynı güvercin (`grey medieval dovecote pigeon seen straight from the front ...`) | `iso_animal_pigeon_cand_se` |

- **Yol B' (kuşlar, 106):** 1. adım 780'deki gibi `create_map_object` + maske; döndürme `body_type="humanoid"`.
- Maske tabanı alt kenardan 2 px yukarıda, yatayda orta. Maske boyları hedefin ortasını verir; çıktı hedefin altına düşerse de teslim edilmez, raporda yazılır.
- Tavuk, kaz ve güvercinde eski görselin rengi ve deseni korunur (tür içi çeşitlilik bu üç varyanttan gelir); yeni tasarım yapılmaz.
- Maliyet: gövde başına 2 gen (tek kare + v3), 10 gövde ≈ 20; 40'lık yineleme gerekirse +1.

## Doğrulama
780 doğrulaması. `scale_check` sınıfları: kedi, tavuk, kaz ve ördek, tavşan, küçük kuş. Yan yana sayfası (`art/results/781-yanyana.png`): 84 px yetişkin, 780'in yeni kedisi, bu işin 10 gövdesi (SE), karo konturu.

## Notlar
- Kod hazır: `IsoArt.Animals` tavuk, kaz, ördek, tavşan, güvercini eşler; `_v2` dosyası gelince o çizilir, gelmezse eski görsel (ördek için küp).
- `sheep_1` yeniden çizilmez: 100'de reddedilmişti, oyun artık kabul edilen `sheep_2`'yi (38 px, tabloda) kullanıyor; eski dosya bu işin arşiv listesinde.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
