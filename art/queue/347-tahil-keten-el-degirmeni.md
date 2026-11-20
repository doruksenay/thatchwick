---
id: "347"
title: Buğday, çavdar, yulaf, keten ekin evreleri ve el değirmeni
status: review
claimed_at: 2026-11-20T10:02Z
approved_by: user        # kullanıcı onayı 2026-11-20 ("hepsinin üretimini başlat")
priority: 20
family: bitki
style_gate: false
cost_est: 13
cost_max: 20
depends_on: ["301", "318"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_wheat_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_rye_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_oats_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_flax_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_item_quern_se.png
created: 2026-11-17
---
# 347 Tahıl, keten ve el değirmeni

## Amaç
Ortaçağ köylüsünün temel ekinleri: buğday ve çavdar (güzün ekilir, kışı toprakta uyuyarak geçirir), yulaf (lapa, at yemi) ve keten (keten bezi: havlu, şilte, yel değirmeni kanadı). El değirmeni (kol taşı) köylünün evdeki yavaş değirmenidir; büyük değirmen taşından önce gelir.

## Tarif
Ekinler için yöntem **318 ile aynı** (1x1, 64x64, evre yükseklikleri 10 / 22 / 32-36, toprak çizilmez, "muted green and earthy colours", üç evre aynı palet ve hizada). El değirmeni için **301** yöntemi (1x1 eşya).
- wheat: `{1: thin grass-like green blades, 2: tall green stalks with young green ears, 3: tall golden stalks with plump unbearded ears of wheat}`
- rye: `{1: thin blue-green grass blades, 2: very tall slender blue-green stalks, 3: very tall thin golden-grey stalks with long slim nodding ears with fine awns}` (buğdaydan daha uzun ve ince)
- oats: `{1: thin green blades, 2: green stalks with loose drooping spikelets starting, 3: pale golden stalks with loose open drooping clusters of oat spikelets}`
- flax: `{1: tiny slender green shoots, 2: slender upright stems with narrow leaves, 3: slender stems topped with small sky-blue five-petal flowers and round seed bolls}`
- quern: `small rotary hand quern of two round grey millstones on a low wooden stand, a wooden handle peg on the top stone, a little flour dust`
- Maliyet: 12 ekin evresi + 1 eşya = 13. İkinci deneme payı 7 → `cost_max` 20.

## Doğrulama
318 doğrulama tablosu; arpa (318) ile yan yana: dört tahıl siluetten ayrılıyor mu (buğday dolgun başak, çavdar en uzun ve ince, yulaf sarkık salkım, arpa kılçıklı). El değirmeni 301 ölçütleri.

## Notlar
Ürün ikonları (buğday, çavdar, yulaf, keten sapı, un, çavdar unu, saman, keten ipliği, keten bezi, ekmekler, lapalar) ikon işi geldiğinde 316 düzeniyle eklenir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
