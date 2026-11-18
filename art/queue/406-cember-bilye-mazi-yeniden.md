---
id: "406"
title: Çember, bilye, mazı yeniden (401'in zayıf üç parçası)
status: pending
approved_by: user        # kullanıcı 18.11.2026: "bunları baştan yapsın"
priority: 20
family: esya
style_gate: false
cost_est: 3
cost_max: 6
depends_on: ["401"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_hoop_se_v3.png
  - Assets/Art/Iso/_aday/iso_item_marbles_se_v3.png
  - Assets/Art/Iso/_aday/icon_oak_gall_v2.png
created: 2026-11-18
---
# 406 Çember, bilye, mazı yeniden

## Amaç
401'den kabul edilmeyen üç parça (sonuç: `art/results/401-...md`). Yöntem 401 ile aynı: `create_map_object` 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline`; her açıklamanın sonuna `, simple still-life object, no face, no eyes, no creature, no person, plain background`. Eski dosyaların üzerine yazılmaz.

## Kullanıcı geri bildirimi ve tarif
| # | Dosya (`_aday/`) | Yer | Sorun (401) | Açıklama |
|---|---|---|---|---|
| 1 | `iso_item_hoop_se_v3.png` | zemin 1x1 (dış) | çember ince, dağınık | `one thick solid wooden hoop, a clean perfect circle made of a single bent wooden band 2 pixels thick, standing upright, one short straight stick leaning against it, tidy and simple` |
| 2 | `iso_item_marbles_se_v3.png` | yuva K | kese soluk, bilyeler kesenin dışında saçık | `small open drawstring pouch of warm brown leather, filled to the top with round clay marbles visible inside the opening, two marbles resting right beside the pouch` |
| 3 | `icon_oak_gall_v2.png` | ikon | şeftali gibi, fazla pembe ve açık | `three small hard round oak galls, dull woody brown with darker brown speckles and tiny bumps, attached to a thin oak twig, matte, not fruit, not peach` |

Renk: bilye kesesi sıcak orta kahve (palete yakın, soluk değil); mazı mat ağaç kahvesi, pembe/turuncu yok. `style.py check` uyarısı varsa `style.py fit` uygula, şekil bozulursa ham hâli red klasörüne.

## Doğrulama
`python tools/art/validate_iso.py <oyuncaklar>`, `python tools/art/style.py check --family esya <hepsi>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
