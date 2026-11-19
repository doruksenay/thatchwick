---
id: "420c"
title: Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 3/4 (15)
status: review
claimed_at: 2026-11-19T07:16Z
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
created: 2026-11-18
---
# 420c Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 3/4 (15)

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

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
