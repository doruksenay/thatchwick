---
id: "410"
title: "Yaban hayvanları: karaca, tilki, kurt, yaban domuzu, ayı (5)"
status: running
claimed_at: 2026-11-20T00:42Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 31, 29 (kullanıcı 18.11.2026 "önerdiklerinin tamamını yap")
family: hayvan
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["375"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wild_roe_deer_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_fox_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_wolf_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_boar_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_bear_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-18
---
# 410 Yaban hayvanları: karaca, tilki, kurt, yaban domuzu, ayı (5)

## Amaç
Büyük yaban hayvanları: karaca ve tilki ziyaretçi, kurt baskın sahnesi, ayı uzaktan görülür (belge 31).

## Tarif
Tarif **375 ile birebir aynı** (106 yol B, 8 yön, v3 döndürme, `style.py fit --family hayvan`, palet referansı `iso_animal_sheep_1_cand_se.png`, açıklama ekine `earthy muted medieval palette, desaturated`). Yalnızca aday tablosu farklı; her tür tek aday. Teslim `cp -n`, `.meta` yok.

| # | Tür id | Dosya (`_aday/`) | Tuval | Açıklama metni |
|---|---|---|---|---|
| 1 | `wild.roe_deer` | `iso_wild_roe_deer_cand_<yön>.png` | 48 | `roe deer with small antlers, reddish-brown coat, white rump` |
| 2 | `wild.fox` | `iso_wild_fox_cand_<yön>.png` | 38 | `red fox with white-tipped bushy tail` |
| 3 | `wild.wolf` | `iso_wild_wolf_cand_<yön>.png` | 44 | `grey eurasian wolf, lean, head low` |
| 4 | `wild.boar` | `iso_wild_boar_cand_<yön>.png` | 44 | `wild boar with dark bristly coat and small tusks` |
| 5 | `wild.bear` | `iso_wild_bear_cand_<yön>.png` | 56 | `brown bear on all fours, heavy shoulders` |

## Doğrulama
375 doğrulaması: `validate_iso.py`, `style.py check --family hayvan`. Bir yön KALDI ise o türün tamamı teslim edilmez, raporda yazılır. Yan yana sayfası koyun 1 ve köpek ile aynı satırda; ölçek oranı gerçeğe uygun (kelebek ve ateşböceği çok küçük).

## Notlar
- Kod eşlemesi hazır: `Assets/Scripts/Game/IsoArt.cs` `Animals` tablosu bu adları arar, dosya yoksa yer tutucu küp çizer. Pivot önerisi tablo dışında 24,10; kullanıcı `IsoPivots.txt`'e düzeltir.
- Dönem: hepsi Avrupa yaban hayvanları; Yeni Dünya türü yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
