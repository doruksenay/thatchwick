---
id: "782"
title: "Yaban hayvanları ölçek seti: tilki, kurt, martı (8 yön)"
status: failed
claimed_at: 2026-11-21T00:57Z
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet ("(b) hepsi birlikte"); tester raporu 66
priority: 40
family: hayvan
style_gate: false
cost_est: 6
cost_max: 14
depends_on: ["780"]
max_attempts: 1
pixellab: true
supersedes:
  - Assets/Art/Iso/_aday/iso_wild_fox_cand_*.png
  - Assets/Art/Iso/_aday/iso_wild_fox_v2_cand_*.png
  - Assets/Art/Iso/_aday/iso_wild_wolf_cand_*.png
  - Assets/Art/Iso/_aday/iso_wild_gull_cand_*.png
outputs:
  - Assets/Art/Iso/_aday/iso_wild_{fox,wolf,gull}_v2_{se,sw,nw,ne,s,w,n,e}.png
  - art/results/782-*.md
created: 2026-11-21
---
# 782 Yaban hayvanları ölçek seti

## Amaç
Ölçüm (tester raporu 66): tilki 36 ve 32 px (hedef 19-26), kurt 39 px (hedef 28-36), martı 31 px (hedef 9-16). Karaca, ayı ve yaban domuzu tabloda; dokunulmaz.

## Tarif
**780 tarifi** (küçük gövde yöntemi, kırpma kuralı, boy ölçümü). Dosya adı: `Assets/Art/Iso/_aday/iso_wild_<tür>_v2_<yön>.png`.

| Gövde | Hedef | Tuval ve maske | Yol | Açıklama | Stil referansı |
|---|---|---|---|---|---|
| `fox` | 19-26 | 32 tuval, maske 26 x 22 | quadruped, `dog` | eski kızıl tilkiyle aynı (`red fox seen straight from the front facing the viewer, bushy tail, white chest, black legs, standing on all four legs`) | `iso_wild_fox_v2_cand_se` |
| `wolf` | 28-36 | 40 tuval, maske 30 x 32 (pixflux da olur; 32'den büyük) | quadruped, `dog` | eski kurtla aynı (`grey wolf seen straight from the front facing the viewer ...`) | `iso_wild_wolf_cand_se` |
| `gull` | 9-16 | 32 tuval, maske 16 x 13 | B' kuş | eski martıyla aynı, kanatlar kapalı, ayakta | `iso_wild_gull_cand_se` |

- İki tilki setinden biri kalır (kızıl tilki, 414t). Oyun `wild.fox` için `iso_wild_fox_v2_<yön>` adını arar (`_cand` olmadan); eski `iso_wild_fox_v2_cand_*` ile karışmaz, arşive gider.

## Doğrulama
780 doğrulaması; `scale_check` sınıfları tilki, kurt, küçük kuş. Yan yana sayfası: 84 px yetişkin, karaca ve ayı (değişmeyen), yeni üç gövde.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
