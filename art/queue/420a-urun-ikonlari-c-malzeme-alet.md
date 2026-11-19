---
id: "420a"
title: Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 1/4 (15)
status: review
claimed_at: 2026-11-19T07:09Z
approved_by: user        # kullanıcı 18.11.2026: "bu sistemi getir" (ürün ikon sistemi); otomatik akış (art/budget.md)
priority: 30
family: arayuz
style_gate: false
cost_est: 15
cost_max: 30
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_nails.png
  - Assets/Art/Iso/_aday/icon_item_feed.png
  - Assets/Art/Iso/_aday/icon_item_wool.png
  - Assets/Art/Iso/_aday/icon_item_compost.png
  - Assets/Art/Iso/_aday/icon_item_resin.png
  - Assets/Art/Iso/_aday/icon_item_withy.png
  - Assets/Art/Iso/_aday/icon_item_salt.png
  - Assets/Art/Iso/_aday/icon_item_hide.png
  - Assets/Art/Iso/_aday/icon_item_leather.png
  - Assets/Art/Iso/_aday/icon_item_saddle.png
  - Assets/Art/Iso/_aday/icon_item_saddle_fine.png
  - Assets/Art/Iso/_aday/icon_item_stone.png
  - Assets/Art/Iso/_aday/icon_item_ice.png
  - Assets/Art/Iso/_aday/icon_item_leather_bag.png
  - Assets/Art/Iso/_aday/icon_item_halter.png
created: 2026-11-18
---
# 420a Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 1/4 (15)

## Amaç
418-420 ilk hâliyle 55-59 ikonluk tek işti; işçi maliyeti ikon başına 1 gen ölçtü ve tavanı aştığı için durdu (0 gen). İşler en çok 19 ikona bölündü (ikon başına 1 gen, cost_max 2 katı).

Ürün ikon sistemi oyuna bağlandı (`IsoArt.ItemIcon`: envanter, pazar, hediye seçici). `items.json`'daki 237 hammadde/ürünün 68'inin ikonu var; bu iş 418-420 serisinin bir parçası, kalanları tamamlar.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- 399'daki hata tekrarlanmasın: pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: Hammaddeler yığın/külçe/demet, kumaşlar katlı top, iksirler kil şişe (mantar rengiyle ayrılır), aletler tek nesne.
- Dönem (CLAUDE.md, belge 20): patates, domates, mısır, balkabağı yok; havuç mor-turuncu ortaçağ havucu; cam eşya yalnızca cam zinciri ürünlerinde.

## Tarif
| items.json id | Dosya (`_aday/`) | Açıklama |
|---|---|---|
| `item.nails` | `icon_item_nails.png` | `handful of iron nails` |
| `item.feed` | `icon_item_feed.png` | `sack of animal feed` |
| `item.wool` | `icon_item_wool.png` | `tuft of raw wool` |
| `item.compost` | `icon_item_compost.png` | `heap of dark compost` |
| `item.resin` | `icon_item_resin.png` | `drops of amber pine resin` |
| `item.withy` | `icon_item_withy.png` | `bundle of willow withies` |
| `item.salt` | `icon_item_salt.png` | `small heap of salt` |
| `item.hide` | `icon_item_hide.png` | `raw animal hide` |
| `item.leather` | `icon_item_leather.png` | `tanned leather piece` |
| `item.saddle` | `icon_item_saddle.png` | `plain leather saddle` |
| `item.saddle_fine` | `icon_item_saddle_fine.png` | `decorated leather saddle with brass` |
| `item.stone` | `icon_item_stone.png` | `rough stones` |
| `item.ice` | `icon_item_ice.png` | `block of ice` |
| `item.leather_bag` | `icon_item_leather_bag.png` | `leather satchel` |
| `item.halter` | `icon_item_halter.png` | `rope horse halter` |

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
