---
id: "409"
title: "Yaban hayvanları: kuşlar ve böcekler (6)"
status: running
claimed_at: 2026-11-19T03:28Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 31, 29 (kullanıcı 18.11.2026 "önerdiklerinin tamamını yap")
family: hayvan
style_gate: false
cost_est: 14
cost_max: 20
depends_on: ["375"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wild_stork_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_crow_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_owl_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_sparrow_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_butterfly_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_firefly_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-18
---
# 409 Yaban hayvanları: kuşlar ve böcekler (6)

## Amaç
Ziyaretçilerin kuş ve böcek kısmı (belge 31). Ateşböceği ışık kaynağı sayılır: `check`'e `--light` eklenir.

## Tarif
Tarif **375 ile birebir aynı** (106 yol B, 8 yön, v3 döndürme, `style.py fit --family hayvan`, palet referansı `iso_animal_sheep_1_cand_se.png`, açıklama ekine `earthy muted medieval palette, desaturated`). Yalnızca aday tablosu farklı; her tür tek aday. Teslim `cp -n`, `.meta` yok.

| # | Tür id | Dosya (`_aday/`) | Tuval | Açıklama metni |
|---|---|---|---|---|
| 1 | `wild.stork` | `iso_wild_stork_cand_<yön>.png` | 42 | `white stork standing on long red legs, black wing tips` |
| 2 | `wild.crow` | `iso_wild_crow_cand_<yön>.png` | 32 | `black carrion crow standing on ground` |
| 3 | `wild.owl` | `iso_wild_owl_cand_<yön>.png` | 32 | `tawny owl perched low, round face, brown mottled` |
| 4 | `wild.sparrow` | `iso_wild_sparrow_cand_<yön>.png` | 24 | `small house sparrow hopping, brown and grey` |
| 5 | `wild.butterfly` | `iso_wild_butterfly_cand_<yön>.png` | 24 | `yellow brimstone butterfly fluttering low over grass` |
| 6 | `wild.firefly` | `iso_wild_firefly_cand_<yön>.png` | 24 | `tiny glowing firefly, soft yellow-green light dot with faint wings` |

## Doğrulama
375 doğrulaması: `validate_iso.py`, `style.py check --family hayvan`. Bir yön KALDI ise o türün tamamı teslim edilmez, raporda yazılır. Yan yana sayfası koyun 1 ve köpek ile aynı satırda; ölçek oranı gerçeğe uygun (kelebek ve ateşböceği çok küçük).

## Notlar
- Kod eşlemesi hazır: `Assets/Scripts/Game/IsoArt.cs` `Animals` tablosu bu adları arar, dosya yoksa yer tutucu küp çizer. Pivot önerisi tablo dışında 24,10; kullanıcı `IsoPivots.txt`'e düzeltir.
- Dönem: hepsi Avrupa yaban hayvanları; Yeni Dünya türü yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
