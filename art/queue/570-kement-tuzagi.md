---
id: "570"
title: Kement tuzağı (haritaya kurulan eşya)
status: pending
approved_by: user        # otomatik akış (CLAUDE.md, 2026-11-17); kullanıcı isteği sohbette 2026-11-20: "kementi de haritaya ekle"
family: esya
style_gate: false
cost_est: 1
cost_max: 3
depends_on: ["304"]      # yöntem 304 ile aynı (kabul edildi)
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_snare_se.png
created: 2026-11-20
---
# 570 Kement tuzağı

## Amaç
`item.snare` artık envanterde sayı değil, haritaya kurulan 1x1 dış mekân eşyası (kendi arsana ya da ormana, göle, sazlığa, tepeye; en çok 3). Şu an görseli yok, yer tutucuyla çiziliyor. Oyuncu ormanda uzaktan tuzağını seçebilmeli.

## Tarif
Yöntem **304 ile aynı** (1x1, `--n 1`). Tuval 80x56, yükseklik 14. Takım: Köylü (`rough light pine wood, rope lashing`).

Açıklama metni: `small medieval snare trap for rabbits on the ground: a thin bent hazel sapling stuck in the soil, a twisted flax cord noose held open by two short wooden pegs, a few dry leaves around, no metal parts, no animal`

- Dönem kuralı: metal yaylı kapan değil; ip ilmek ve eğik dal (ortaçağ kement tuzağı).
- Maliyet 1 gen. İkinci deneme payı: `cost_max` 3.

## Doğrulama
304 doğrulaması (`validate_iso.py`, `style.py check --family esya`). Yan yana sayfası: balık sepeti (`iso_item_fish_trap_se`), korkuluk (`iso_item_scarecrow_se_v2`) ve bir çalı döşemesiyle. Ölçüt: orman zemininde seçiliyor, balık sepetinden küçük duruyor, ilmek okunuyor.

## Notlar
- Kabul edilince `items.json` > `item.snare` için `art` alanı eklenir: `{"alongX": "iso_item_snare_se", "pivotX": 40.0, "pivotY": 24.0, "mirror": true}` (balık sepetiyle aynı pivot). Bu, kod oturumunun işidir.
- Dolu tuzak (av düşmüş) için ayrı kare istenmedi; gerekirse ayrı iş açılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
