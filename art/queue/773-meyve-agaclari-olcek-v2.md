---
id: "773"
title: "Meyve ağaçları stil ve ölçek (armut, kiraz, erik, zeytin; genç, olgun, meyveli, _v2)"
status: pending
approved_by: user        # otomatik akış; kullanıcı 21.11.2026: "ağaçları ... yeniden üret" (Project belgesi 66, 66b)
priority: 10
family: bitki
style_gate: false
cost_est: 15
cost_max: 28
depends_on: ["772"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_pear_{young,grown,fruit}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_cherry_{young,grown,fruit}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_plum_{young,grown,fruit}_se_v2.png
  - Assets/Art/Iso/_aday/iso_plant_olive_{young,grown,fruit}_se_v2.png
created: 2026-11-21
---
# 773 Meyve ağaçları, ölçek tablosuna göre (_v2)

## Amaç
Tester raporu 66 §1.2: meyve ağaçları 58-80 px (insan 76 px); bazı türlerde meyveli evre olgundan uzun, genç evre olgundan büyük.
Ölçek tablosu: olgun ve meyveli **95-115 px**, genç **50-72 px**. Yalnızca oyunda eşyası olan türler (`item.tree_apple/pear/cherry/plum`,
`item.tree_olive` → `iso_plant_olive`). Eşyası olmayan türler (badem, kayısı, incir, limon, şeftali...) eşya eklenince aynı tabloyla ve **772 stil çapasıyla** üretilir; eski görselleri (botanik ya da yumuşak taç) referans alınmaz.

## Tarif
**772 tarifi ve stil çapası** (2026-11-21 kullanıcı kararı: bütün ağaçlar kabarık kümeli taç stilinde). Türün eski görseli **stil referansı değildir**: elma, kiraz, erik eski hâliyle yumuşak düz gölgeli tek taç lekesi, zeytin koyu düz leke; yalnızca meyve rengi için bakılır. Stil referansı 772'nin yeni elması, olgun meşesi ve dut. Elma 772 pilotunda üretildi, bu işte yok. Farklar:
- **Tuval 96x136, pivot (48, 24)**.
- Meyveli evre olgunun **aynı gövde ve taçla** düzenlenmiş hali: önce olgun üretilir, meyveli `edit_image_pro_flash` ile olgun
  görselden (`same tree, 10-14 visible ripe fruits on the crown`); taç ve gövde piksel piksel aynı kalır.
- Meyve renkleri 319'daki gibi; zeytin: gümüşi yeşil yaprak, küçük koyu mor-yeşil zeytinler, eğri gövde.
- 12 görsel (4 meyveli düzenleme) ≈ 12-16 gen; ikinci deneme payıyla `cost_max` 28.
- Stil sayfası `773-stil.png`: yeni görseller + 772'nin elması ve meşesi + eski elma, kiraz, erik.

## Doğrulama
772 doğrulaması; ek: olgun ↔ meyveli fark yalnızca meyve noktaları mı; `scale_check.py` (genç < olgun; meyveli = olgun ±4 px).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
