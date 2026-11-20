---
id: "484"
title: "Köpek tipleri: 7 gövde (çoban, tazı, zağar, iz, mastıf, su, kucak)"
status: running
claimed_at: 2026-11-20T10:03Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: hayvan
style_gate: false
cost_est: 16
cost_max: 26
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_dog_shepherd_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_sighthound_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_scenthound_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_tracker_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_mastiff_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_waterdog_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_lapdog_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-20
---
# 484 Köpek tipleri

## Amaç
Belge 48 §4: köpekler dönemde ırkla değil işle tanımlanırdı. Genetik sistemi (kod: `GeneticsCatalog`) her tip için bir gövde kullanır (`BreedTypeDef.Body`: `dog_<tip>`). Adlı kırmalar (kırma tazı, alan, kurt kıran, göl izcisi) ebeveyn tipinin gövdesini kullanır; ayrı görsel gerekmez.

## Tarif
Tarif **375 ile aynı** (106 tarifi, yol B, güney kontrolü, zemin kontrolü, `fit --family hayvan`, teslim kuralı, masaüstü kopyası). Farklar:

1. **Renk paletle gelir (belge 48 §11):** her tip **tek gövde** olarak, gri-kahve nötr tonlarda üretilir; oyundaki tüy rengi (kızıl, siyah, kum, tekir ...) gövdeye palet değişimiyle uygulanır. Açıklama ekine `, neutral grey-brown coat, plain coat without markings or spots` eklenir. Desen, beyaz leke ve maske ayrı katman olarak sonra gelir (gövdeler kabul edilince ayrı iş).
2. **Açıklama eki** (375 ile aynı): `, standing directly on transparent background, no ground disc, no floor shadow, no grey patch under the paws`.
3. Her satırdaki gövde biçimi kesin uyulacak ayrımdır: tipler yan yana konunca birbirinden ayırt edilmeli (boy, bacak uzunluğu, kulak, kuyruk, tüy).

| # | Tip | Ad | Gövde (açıklama metni) | Dosya |
|---|---|---|---|---|
| 1 | `dog.shepherd` | iri çoban köpeği | `large heavy livestock guardian dog, thick neck, deep chest, medium-long dense coat, tail curled low` | `iso_animal_dog_shepherd_cand_*` |
| 2 | `dog.sighthound` | tazı | `tall slender greyhound-like sighthound, very long thin legs, narrow head, deep chest, tucked belly, short coat, long thin tail` | `iso_animal_dog_sighthound_cand_*` |
| 3 | `dog.scenthound` | zağar (sürü av köpeği) | `medium hound, long drooping ears, sturdy legs, short coat, tail raised like a sickle` | `iso_animal_dog_scenthound_cand_*` |
| 4 | `dog.tracker` | iz köpeği (lymer) | `heavy-headed scent hound, very long drooping ears, loose skin around muzzle, calm stance, short coat` | `iso_animal_dog_tracker_cand_*` |
| 5 | `dog.mastiff` | mastıf | `massive mastiff guard dog, broad head with short muzzle, heavy bones, short coat` | `iso_animal_dog_mastiff_cand_*` |
| 6 | `dog.waterdog` | su köpeği | `medium curly-coated water dog, dense curly coat, webbed paws, tail low` | `iso_animal_dog_waterdog_cand_*` |
| 7 | `dog.lapdog` | kucak köpeği | `small lapdog, silky long coat, feathered ears, small delicate legs` | `iso_animal_dog_lapdog_cand_*` |

- Tuval: çoban ve mastıf 42, tazı 42, zağar, iz ve su köpeği 38, kucak köpeği 32.
- Maliyet: 7 aday × 2 gen + azaltma ≈ 16. `cost_max` 26.

## Doğrulama
375 doğrulaması. Yan yana sayfası: 7 tip + kabul edilen koyun 1 (boy ölçüsü). Tazı en uzun bacaklı, mastıf en ağır, kucak köpeği en küçük olmalı.

## Notlar
Kod gövdeyi `iso_animal_dog_<tip>` adıyla arar; gelene kadar renkli yer tutucu ve eski köpek görseli kullanılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
