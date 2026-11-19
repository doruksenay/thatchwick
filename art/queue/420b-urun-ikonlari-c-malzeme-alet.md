---
id: "420b"
title: Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 2/4 (15)
status: pending
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
  - Assets/Art/Iso/_aday/icon_item_yarn.png
  - Assets/Art/Iso/_aday/icon_item_cloth.png
  - Assets/Art/Iso/_aday/icon_item_blanket.png
  - Assets/Art/Iso/_aday/icon_item_basket.png
  - Assets/Art/Iso/_aday/icon_item_potion_energy.png
  - Assets/Art/Iso/_aday/icon_item_potion_warmth.png
  - Assets/Art/Iso/_aday/icon_item_potion_calm.png
  - Assets/Art/Iso/_aday/icon_item_potion_focus.png
  - Assets/Art/Iso/_aday/icon_item_potion_growth.png
  - Assets/Art/Iso/_aday/icon_item_animal_salve.png
  - Assets/Art/Iso/_aday/icon_item_clay.png
  - Assets/Art/Iso/_aday/icon_item_iron_ore.png
  - Assets/Art/Iso/_aday/icon_item_iron_bar.png
  - Assets/Art/Iso/_aday/icon_item_horseshoe.png
  - Assets/Art/Iso/_aday/icon_item_gear.png
created: 2026-11-18
---
# 420b Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 2/4 (15)

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
| `item.yarn` | `icon_item_yarn.png` | `ball of wool yarn` |
| `item.cloth` | `icon_item_cloth.png` | `folded roll of wool cloth` |
| `item.blanket` | `icon_item_blanket.png` | `folded wool blanket` |
| `item.basket` | `icon_item_basket.png` | `empty woven willow basket` |
| `item.potion_energy` | `icon_item_potion_energy.png` | `small clay vial with red cork` |
| `item.potion_warmth` | `icon_item_potion_warmth.png` | `small clay vial with orange cork` |
| `item.potion_calm` | `icon_item_potion_calm.png` | `small clay vial with blue cork` |
| `item.potion_focus` | `icon_item_potion_focus.png` | `small clay vial with green cork` |
| `item.potion_growth` | `icon_item_potion_growth.png` | `small clay vial with leaf on cork` |
| `item.animal_salve` | `icon_item_animal_salve.png` | `small pot of animal salve` |
| `item.clay` | `icon_item_clay.png` | `lump of wet grey clay` |
| `item.iron_ore` | `icon_item_iron_ore.png` | `lump of iron ore` |
| `item.iron_bar` | `icon_item_iron_bar.png` | `iron bar` |
| `item.horseshoe` | `icon_item_horseshoe.png` | `iron horseshoe` |
| `item.gear` | `icon_item_gear.png` | `wooden gear wheel` |

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
