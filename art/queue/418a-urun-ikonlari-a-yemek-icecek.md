---
id: "418a"
title: Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü parça 1/3 (19)
status: pending
approved_by: user        # kullanıcı 18.11.2026: "bu sistemi getir" (ürün ikon sistemi); otomatik akış (art/budget.md)
priority: 30
family: arayuz
style_gate: false
cost_est: 19
cost_max: 38
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_milk.png
  - Assets/Art/Iso/_aday/icon_item_salted_meat.png
  - Assets/Art/Iso/_aday/icon_item_salted_poultry.png
  - Assets/Art/Iso/_aday/icon_item_pastirma.png
  - Assets/Art/Iso/_aday/icon_item_smoked_meat.png
  - Assets/Art/Iso/_aday/icon_item_smoked_poultry.png
  - Assets/Art/Iso/_aday/icon_item_cheese.png
  - Assets/Art/Iso/_aday/icon_item_brined_cheese.png
  - Assets/Art/Iso/_aday/icon_item_smoked_cheese.png
  - Assets/Art/Iso/_aday/icon_item_pickled_carrot.png
  - Assets/Art/Iso/_aday/icon_item_pickled_beet.png
  - Assets/Art/Iso/_aday/icon_item_pickled_onion.png
  - Assets/Art/Iso/_aday/icon_item_pickled_radish.png
  - Assets/Art/Iso/_aday/icon_item_dried_apple.png
  - Assets/Art/Iso/_aday/icon_item_dried_pear.png
  - Assets/Art/Iso/_aday/icon_item_dried_plum.png
  - Assets/Art/Iso/_aday/icon_item_dried_mushroom.png
  - Assets/Art/Iso/_aday/icon_item_jam_berry.png
  - Assets/Art/Iso/_aday/icon_item_jam_plum.png
created: 2026-11-18
---
# 418a Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü parça 1/3 (19)

## Amaç
418-420 ilk hâliyle 55-59 ikonluk tek işti; işçi maliyeti ikon başına 1 gen ölçtü ve tavanı aştığı için durdu (0 gen). Her iş en çok 19 ikona bölündü (ikon başına 1 gen, cost_max 2 katı).

Ürün ikon sistemi oyuna bağlandı (`IsoArt.ItemIcon`: envanter, pazar, hediye seçici). `items.json`'daki 237 hammadde/ürünün 68'inin ikonu var; bu iş 418-420 serisinin bir parçası, kalanları tamamlar.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- 399'daki hata tekrarlanmasın: pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: Yemekler kase/tabak (316 kase stili), içecekler kil kupa/testi, turşular kavanoz, kuruluklar dizi/demet, tütsülenmiş ürünler altın-kahve.
- Dönem (CLAUDE.md, belge 20): patates, domates, mısır, balkabağı yok; havuç mor-turuncu ortaçağ havucu; cam eşya yalnızca cam zinciri ürünlerinde.

## Tarif
| items.json id | Dosya (`_aday/`) | Açıklama |
|---|---|---|
| `item.milk` | `icon_item_milk.png` | `clay jug of milk` |
| `item.salted_meat` | `icon_item_salted_meat.png` | `salted meat slab` |
| `item.salted_poultry` | `icon_item_salted_poultry.png` | `salted chicken` |
| `item.pastirma` | `icon_item_pastirma.png` | `slices of cured beef` |
| `item.smoked_meat` | `icon_item_smoked_meat.png` | `smoked ham` |
| `item.smoked_poultry` | `icon_item_smoked_poultry.png` | `smoked chicken` |
| `item.cheese` | `icon_item_cheese.png` | `wheel of yellow cheese` |
| `item.brined_cheese` | `icon_item_brined_cheese.png` | `white cheese in brine bowl` |
| `item.smoked_cheese` | `icon_item_smoked_cheese.png` | `smoked brown cheese` |
| `item.pickled_carrot` | `icon_item_pickled_carrot.png` | `jar of pickled carrots` |
| `item.pickled_beet` | `icon_item_pickled_beet.png` | `jar of pickled beets` |
| `item.pickled_onion` | `icon_item_pickled_onion.png` | `jar of pickled onions` |
| `item.pickled_radish` | `icon_item_pickled_radish.png` | `jar of pickled radish` |
| `item.dried_apple` | `icon_item_dried_apple.png` | `string of dried apple rings` |
| `item.dried_pear` | `icon_item_dried_pear.png` | `dried pear halves` |
| `item.dried_plum` | `icon_item_dried_plum.png` | `handful of prunes` |
| `item.dried_mushroom` | `icon_item_dried_mushroom.png` | `string of dried mushrooms` |
| `item.jam_berry` | `icon_item_jam_berry.png` | `jar of berry jam` |
| `item.jam_plum` | `icon_item_jam_plum.png` | `jar of plum jam` |

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
