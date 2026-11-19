---
id: "395d"
title: Beşinci tur: ahşap çatkı, kandil, kömür yığını (sıfırdan, çok adaylı)
status: pending
approved_by: user        # kullanıcı 19.11.2026: "önerilerini yap"
priority: 20
family: esya
style_gate: false
cost_est: 15
cost_max: 25
depends_on: ["364"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_mine_support_wood_e_se.png
  - Assets/Art/Iso/_aday/iso_item_mine_lamp_e_se.png
  - Assets/Art/Iso/_aday/iso_item_charcoal_clamp_e_2x2_se.png
  - art/results/395d-catki-kandil-komur.md
created: 2026-11-19
---
# 395d Beşinci tur (yöntem değişikliği)

## Amaç
395c reddedildi: üçü de tarife uymadı. Arka plan/inpaint ile stil eşleme şekli bozdu (çatkıda kafes kaldı, kandil ateş çanağına, kömür yığını davula döndü). Bu turda önce **şekil** doğru çıkacak, palet sonra yerel olarak eşlenecek.

## Tarif
Var olan PNG'lerin üzerine yazılmaz (`_e`). Her parça için `create_map_object` **arka plan görseli olmadan**, yalnızca metinle, izometrik `low top-down`, `no_background`. Her parçada 4 aday üretilir (toplam 12 çağrı civarı); şekil ölçütüne en uyan seçilir. Sonra yerel `reduce_colors` + `style.py fit --family esya`.
1. **Ahşap çatkı** (64x96): `mine tunnel timber support frame: exactly two plain straight vertical light brown pine posts and one horizontal beam across the top, rectangular doorway shape, no diagonal braces, no cross bars, no dark red paint, isometric game asset`. Ölçüt: iki dikme + bir kiriş, çapraz yok.
2. **Kandil** (32x32): `tiny handheld clay oil lamp, small open shallow saucer of grey-brown clay with a pinched spout and a small single flame at the spout, no stand, no bowl of fire, isometric game asset`. Ölçüt: küçük, tek alev, yerden yüksek ayak yok.
3. **Kömür yığını** (2x2, 128x96): `charcoal clamp kiln: a low wide rounded dome mound of dark earth covered with patches of green grass turf, a few small log ends sticking out at the base, a thin wisp of grey smoke from the top, no walls, no barrel, no decoration, isometric game asset`. Ölçüt: alçak, geniş kubbe; dikey kenar yok.

## Doğrulama
`validate_iso.py`, `style.py check --family esya` (kandilde `--light`). Yan yana sayfası: her parçanın 4 adayı (seçilen işaretli) ve 395c'deki reddedilen hali. Hiçbir aday ölçütü geçmezse o parça teslim edilmez, adayları `art/results/395d-red/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
