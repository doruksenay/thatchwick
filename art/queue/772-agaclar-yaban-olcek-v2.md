---
id: "772"
title: "Yaban ağaçları yeniden ölçek (meşe, huş, çam, söğüt; genç ve olgun, _v2)"
status: pending
approved_by: user        # otomatik akış; kullanıcı 21.11.2026: "ağaçları ... yeniden üret" (Project belgesi 66, 66b)
priority: 10
family: bitki
style_gate: true
cost_est: 8
cost_max: 16
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_oak_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_birch_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_pine_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_willow_{young,grown}_se_v2.png
created: 2026-11-21
---
# 772 Yaban ağaçları, ölçek tablosuna göre (_v2)

## Amaç
Tester raporu 66 §1.2: olgun meşe pivot üstü 75 px, yani 76 px'lik insan boyunda. Köy ve orman insan boyunda ağaçlarla
dolu görünüyor. Yeni ölçek tablosu (`docs/gorsel-yonerge.md` "Ölçek referansı"): olgun yaban ağacı **120-140 px**, genç **60-85 px**.
Var olan dosyaların üzerine yazılmaz: yeni sürüm `_v2` ekiyle gelir, oyun onu kendiliğinden tercih eder (`IsoArt.GetPlant`).

## Tarif
319/301 ağaç yöntemi (`create_map_object` stil eşleme, bitki ailesi referansları; aynı türün eski görseli de referans: renk ve
taç biçimi korunsun, yalnızca boy ve oran büyüsün). Farklar:
- **Tuval 112x160, pivot (56, 24)**: gövde tabanı tuvalin yatay ortasında, alttan 24 px'te (eski 96x112 tuvalde pivot x=40 idi).
- Olgun: gövde insan boyunun en az yarısı kadar görünür (taç gövdenin üstünde başlar, yere değmez), toplam pivot üstü 120-140 px.
  Genç: ince gövde, seyrek taç, 60-85 px.
- Türler: meşe (geniş yayvan taç, kalın gövde), huş (beyaz kabuk, ince uzun, seyrek açık yeşil), çam (koyu yeşil konik, düz gövde),
  söğüt (sarkık ince dallar, gümüşi yeşil).
- Açıklama sonu: `, isometric view, single tree, no ground, no grass, transparent background, light from top-left, single dark outline`.
- 8 görsel × ~1 gen; ikinci deneme payı → `cost_max` 16.

## Doğrulama
`validate_iso.py --kind object`, `style.py check --family bitki`, **`python tools/art/scale_check.py <png>...`** (KALDI teslim
edilmez). Yan yana sayfası: yeni ağaç, eski ağaç ve `iso_char_man_r84_idle_se_00` aynı taban çizgisinde.

## Notlar
- Ulu ağaçlar (492a2-c) bu boydan en az 1,4 kat büyük kalır.
- Gölge sprite'a pişirilmez; rüzgârda sallanma oyunun gölgelendiricisinde.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
