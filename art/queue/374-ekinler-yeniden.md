---
id: "374"
title: Ekin evreleri yeniden (pırasa 1 ve 3, bezelye 2, şalgam 2)
status: pending
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 ("yap bunu": yeniden deneme işleri)
family: bitki
style_gate: false
cost_est: 4
cost_max: 8
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_leek_{1,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_pea_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_turnip_2_se.png
created: 2026-11-18
---
# 374 Ekin evreleri yeniden

## Amaç
310'un 15 evresinden 11'i 371'de kurtarılıyor. Bu dördü biçim yüzünden yeniden üretilir:
- pırasa 1: iki denemede de zemin karosu;
- pırasa 3: mavi agav gibi, pırasa değil;
- bezelye 2: toprak kare taban üstünde;
- şalgam 2: altıgen kümeler, tanınmıyor.

## Tarif
Yöntem **310 ile aynı**: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, evre yükseklikleri 10/22/32, bezelye çitli 40. Farklar 372'deki gibi:
- Referanslar bitki ailesinin yeni referansları.
- Açıklama eki: `, single crop plant on one tile seen from high top-down isometric view, muted desaturated natural greens, simple shading, light from top-left, no ground tile, no soil square, no tray, no shadow, transparent background`
- Üretimden sonra `style.py fit --family bitki`, ardından `check`.
- İkinci deneme yalnızca biçim için.

| Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|
| `iso_crop_leek_1_se.png` | 10 | `three thin young leek shoots like tall grass blades` |
| `iso_crop_leek_3_se.png` | 32 | `one mature leek: thick straight white stalk with long flat dark green leaves fanning upward` |
| `iso_crop_pea_2_se.png` | 30 | `young pea vine climbing a single thin wooden stick, small round leaves and curly tendrils` |
| `iso_crop_turnip_2_se.png` | 22 | `turnip plant with a rosette of broad green leaves and the pale purple-white top of the root just visible` |

- Maliyet 4 × 1 gen. İkinci deneme payı 4: `cost_max` 8.

## Doğrulama
- 310 doğrulaması.
- Evre dizisi sayfası: her ekin 1-2-3 yan yana, 371'den kurtarılan evrelerle birlikte. Evreler aynı bitkinin büyümesi gibi okunuyor mu.

## Notlar
Kod tarafı (ekinin büyüme yüzdesine göre evre görseli) bu iş ve 371 review'a geçince yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
