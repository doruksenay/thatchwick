---
id: "420"
title: Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat (59)
status: running
claimed_at: 2026-11-18T20:02Z
approved_by: user        # kullanıcı 18.11.2026: "bu sistemi getir" (ürün ikon sistemi); otomatik akış (art/budget.md)
priority: 30
family: arayuz
style_gate: false
cost_est: 20
cost_max: 34
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
  - Assets/Art/Iso/_aday/icon_item_flour.png
  - Assets/Art/Iso/_aday/icon_item_beeswax.png
  - Assets/Art/Iso/_aday/icon_item_pelt.png
  - Assets/Art/Iso/_aday/icon_item_fur.png
  - Assets/Art/Iso/_aday/icon_item_remedy.png
  - Assets/Art/Iso/_aday/icon_item_brick.png
  - Assets/Art/Iso/_aday/icon_item_roof_tiles.png
  - Assets/Art/Iso/_aday/icon_item_cut_stone.png
  - Assets/Art/Iso/_aday/icon_item_dye_red.png
  - Assets/Art/Iso/_aday/icon_item_dye_yellow.png
  - Assets/Art/Iso/_aday/icon_item_dye_blue.png
  - Assets/Art/Iso/_aday/icon_item_soap.png
  - Assets/Art/Iso/_aday/icon_item_felt.png
  - Assets/Art/Iso/_aday/icon_item_manure.png
  - Assets/Art/Iso/_aday/icon_item_straw.png
  - Assets/Art/Iso/_aday/icon_item_rye_flour.png
  - Assets/Art/Iso/_aday/icon_item_linen_yarn.png
  - Assets/Art/Iso/_aday/icon_item_linen.png
  - Assets/Art/Iso/_aday/icon_item_broken_pottery.png
  - Assets/Art/Iso/_aday/icon_item_broken_glass.png
  - Assets/Art/Iso/_aday/icon_item_ash.png
  - Assets/Art/Iso/_aday/icon_item_fine_sand.png
  - Assets/Art/Iso/_aday/icon_item_clear_glass.png
  - Assets/Art/Iso/_aday/icon_item_mirror_sphere.png
  - Assets/Art/Iso/_aday/icon_item_mirror_glass.png
  - Assets/Art/Iso/_aday/icon_item_hunting_bow.png
  - Assets/Art/Iso/_aday/icon_item_snare.png
  - Assets/Art/Iso/_aday/icon_item_tin.png
  - Assets/Art/Iso/_aday/icon_item_mussel_shell.png
created: 2026-11-18
---
# 420 Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat (59)

## Amaç
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
| `item.flour` | `icon_item_flour.png` | `small sack of flour` |
| `item.beeswax` | `icon_item_beeswax.png` | `yellow block of beeswax` |
| `item.pelt` | `icon_item_pelt.png` | `raw pelt` |
| `item.fur` | `icon_item_fur.png` | `folded fur pelt` |
| `item.remedy` | `icon_item_remedy.png` | `small pot of healing salve` |
| `item.brick` | `icon_item_brick.png` | `two red clay bricks` |
| `item.roof_tiles` | `icon_item_roof_tiles.png` | `stack of clay roof tiles` |
| `item.cut_stone` | `icon_item_cut_stone.png` | `cut stone block` |
| `item.dye_red` | `icon_item_dye_red.png` | `pot of red dye` |
| `item.dye_yellow` | `icon_item_dye_yellow.png` | `pot of yellow dye` |
| `item.dye_blue` | `icon_item_dye_blue.png` | `pot of blue dye` |
| `item.soap` | `icon_item_soap.png` | `bar of soap` |
| `item.felt` | `icon_item_felt.png` | `folded sheet of felt` |
| `item.manure` | `icon_item_manure.png` | `heap of manure` |
| `item.straw` | `icon_item_straw.png` | `bundle of straw` |
| `item.rye_flour` | `icon_item_rye_flour.png` | `sack of dark rye flour` |
| `item.linen_yarn` | `icon_item_linen_yarn.png` | `skein of linen yarn` |
| `item.linen` | `icon_item_linen.png` | `folded roll of linen` |
| `item.broken_pottery` | `icon_item_broken_pottery.png` | `pottery shards` |
| `item.broken_glass` | `icon_item_broken_glass.png` | `shards of broken glass` |
| `item.ash` | `icon_item_ash.png` | `small heap of grey wood ash` |
| `item.fine_sand` | `icon_item_fine_sand.png` | `small heap of fine pale sand` |
| `item.clear_glass` | `icon_item_clear_glass.png` | `small sheet of clear glass` |
| `item.mirror_sphere` | `icon_item_mirror_sphere.png` | `small glass mirror sphere` |
| `item.mirror_glass` | `icon_item_mirror_glass.png` | `small round mirror glass` |
| `item.hunting_bow` | `icon_item_hunting_bow.png` | `wooden hunting bow` |
| `item.snare` | `icon_item_snare.png` | `wire and cord snare trap` |
| `item.tin` | `icon_item_tin.png` | `tin ingot` |
| `item.mussel_shell` | `icon_item_mussel_shell.png` | `mussel shells` |

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
