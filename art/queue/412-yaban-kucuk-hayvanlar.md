---
id: "408"
title: "Yaban hayvanları: küçük memeliler ve kurbağa (5)"
status: running
claimed_at: 2026-11-20T06:39Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 31, 29 (kullanıcı 18.11.2026 "önerdiklerinin tamamını yap")
family: hayvan
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["375"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wild_rabbit_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_squirrel_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_hedgehog_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_frog_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_rat_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-18
---
# 408 Yaban hayvanları: küçük memeliler ve kurbağa (5)

## Amaç
Haritada dolaşan yaban ziyaretçileri (`WildRoamers`, belge 31). Şimdi renkli küp olarak görünüyorlar.

## Tarif
Tarif **375 ile birebir aynı** (106 yol B, 8 yön, v3 döndürme, `style.py fit --family hayvan`, palet referansı `iso_animal_sheep_1_cand_se.png`, açıklama ekine `earthy muted medieval palette, desaturated`). Yalnızca aday tablosu farklı; her tür tek aday. Teslim `cp -n`, `.meta` yok.

| # | Tür id | Dosya (`_aday/`) | Tuval | Açıklama metni |
|---|---|---|---|---|
| 1 | `wild.rabbit` | `iso_wild_rabbit_cand_<yön>.png` | 32 | `brown european wild rabbit sitting alert, long ears` |
| 2 | `wild.squirrel` | `iso_wild_squirrel_cand_<yön>.png` | 32 | `red squirrel with bushy tail, tufted ears` |
| 3 | `wild.hedgehog` | `iso_wild_hedgehog_cand_<yön>.png` | 32 | `european hedgehog with brown spines, small snout` |
| 4 | `wild.frog` | `iso_wild_frog_cand_<yön>.png` | 24 | `common green-brown frog crouching` |
| 5 | `wild.rat` | `iso_wild_rat_cand_<yön>.png` | 24 | `brown rat with long bare tail` |

## Doğrulama
375 doğrulaması: `validate_iso.py`, `style.py check --family hayvan`. Bir yön KALDI ise o türün tamamı teslim edilmez, raporda yazılır. Yan yana sayfası koyun 1 ve köpek ile aynı satırda; ölçek oranı gerçeğe uygun (kelebek ve ateşböceği çok küçük).

## Notlar
- Kod eşlemesi hazır: `Assets/Scripts/Game/IsoArt.cs` `Animals` tablosu bu adları arar, dosya yoksa yer tutucu küp çizer. Pivot önerisi tablo dışında 24,10; kullanıcı `IsoPivots.txt`'e düzeltir.
- Dönem: hepsi Avrupa yaban hayvanları; Yeni Dünya türü yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
