---
id: "419a"
title: Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et parça 1/3 (19)
status: review
claimed_at: 2026-11-19T07:02Z
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
  - Assets/Art/Iso/_aday/icon_item_parsnip.png
  - Assets/Art/Iso/_aday/icon_item_seed_parsnip.png
  - Assets/Art/Iso/_aday/icon_item_carrot.png
  - Assets/Art/Iso/_aday/icon_item_seed_carrot.png
  - Assets/Art/Iso/_aday/icon_item_onion.png
  - Assets/Art/Iso/_aday/icon_item_seed_onion.png
  - Assets/Art/Iso/_aday/icon_item_garlic.png
  - Assets/Art/Iso/_aday/icon_item_seed_garlic.png
  - Assets/Art/Iso/_aday/icon_item_beet.png
  - Assets/Art/Iso/_aday/icon_item_seed_beet.png
  - Assets/Art/Iso/_aday/icon_item_radish.png
  - Assets/Art/Iso/_aday/icon_item_seed_radish.png
  - Assets/Art/Iso/_aday/icon_item_lettuce.png
  - Assets/Art/Iso/_aday/icon_item_seed_lettuce.png
  - Assets/Art/Iso/_aday/icon_item_broad_bean.png
  - Assets/Art/Iso/_aday/icon_item_seed_broad_bean.png
  - Assets/Art/Iso/_aday/icon_item_barley.png
  - Assets/Art/Iso/_aday/icon_item_seed_barley.png
  - Assets/Art/Iso/_aday/icon_item_spinach.png
created: 2026-11-18
---
# 419a Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et parça 1/3 (19)

## Amaç
418-420 ilk hâliyle 55-59 ikonluk tek işti; işçi maliyeti ikon başına 1 gen ölçtü ve tavanı aştığı için durdu (0 gen). Her iş en çok 19 ikona bölündü (ikon başına 1 gen, cost_max 2 katı).

Ürün ikon sistemi oyuna bağlandı (`IsoArt.ItemIcon`: envanter, pazar, hediye seçici). `items.json`'daki 237 hammadde/ürünün 68'inin ikonu var; bu iş 418-420 serisinin bir parçası, kalanları tamamlar.

## Yöntem
- 316 ile aynı stil: 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk; sayfa başı 6 ikon üretip kes (316 yöntemi).
- 399'daki hata tekrarlanmasın: pixen yüz/göz/figür üretirse `create_map_object` 32x32 kullan (401/417 yöntemi). Her açıklamanın sonuna: `, simple still-life object, no face, no eyes, no creature, no person`.
- Aile kuralı: Tohumlar 316'daki aynı küçük bez kese (kesenin önünde tohum şekli), fidanlar toprak topaklı küçük fidan, ham ürünler doğal renk, balıklar yandan.
- Dönem (CLAUDE.md, belge 20): patates, domates, mısır, balkabağı yok; havuç mor-turuncu ortaçağ havucu; cam eşya yalnızca cam zinciri ürünlerinde.

## Tarif
| items.json id | Dosya (`_aday/`) | Açıklama |
|---|---|---|
| `item.parsnip` | `icon_item_parsnip.png` | `pale parsnip` |
| `item.seed_parsnip` | `icon_item_seed_parsnip.png` | `small cloth seed pouch with parsnip seeds` |
| `item.carrot` | `icon_item_carrot.png` | `orange-purple medieval carrot` |
| `item.seed_carrot` | `icon_item_seed_carrot.png` | `small cloth seed pouch with carrot seeds` |
| `item.onion` | `icon_item_onion.png` | `brown onion` |
| `item.seed_onion` | `icon_item_seed_onion.png` | `small cloth seed pouch with onion seeds` |
| `item.garlic` | `icon_item_garlic.png` | `garlic bulb` |
| `item.seed_garlic` | `icon_item_seed_garlic.png` | `small cloth seed pouch with garlic seeds` |
| `item.beet` | `icon_item_beet.png` | `red beetroot` |
| `item.seed_beet` | `icon_item_seed_beet.png` | `small cloth seed pouch with beet seeds` |
| `item.radish` | `icon_item_radish.png` | `red radish` |
| `item.seed_radish` | `icon_item_seed_radish.png` | `small cloth seed pouch with radish seeds` |
| `item.lettuce` | `icon_item_lettuce.png` | `head of lettuce` |
| `item.seed_lettuce` | `icon_item_seed_lettuce.png` | `small cloth seed pouch with lettuce seeds` |
| `item.broad_bean` | `icon_item_broad_bean.png` | `green broad bean pods` |
| `item.seed_broad_bean` | `icon_item_seed_broad_bean.png` | `small cloth seed pouch with broad bean seeds` |
| `item.barley` | `icon_item_barley.png` | `bundle of barley ears` |
| `item.seed_barley` | `icon_item_seed_barley.png` | `small cloth seed pouch with barley seeds` |
| `item.spinach` | `icon_item_spinach.png` | `bunch of spinach` |

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
