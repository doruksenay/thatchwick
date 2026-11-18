---
id: "419"
title: Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et (55)
status: blocked
claimed_at: 2026-11-18T19:55Z
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
  - Assets/Art/Iso/_aday/icon_item_seed_spinach.png
  - Assets/Art/Iso/_aday/icon_item_apple.png
  - Assets/Art/Iso/_aday/icon_item_sapling_apple.png
  - Assets/Art/Iso/_aday/icon_item_pear.png
  - Assets/Art/Iso/_aday/icon_item_sapling_pear.png
  - Assets/Art/Iso/_aday/icon_item_cherry.png
  - Assets/Art/Iso/_aday/icon_item_sapling_cherry.png
  - Assets/Art/Iso/_aday/icon_item_plum.png
  - Assets/Art/Iso/_aday/icon_item_sapling_plum.png
  - Assets/Art/Iso/_aday/icon_item_wild_garlic.png
  - Assets/Art/Iso/_aday/icon_item_nettle.png
  - Assets/Art/Iso/_aday/icon_item_elderflower.png
  - Assets/Art/Iso/_aday/icon_item_wild_strawberry.png
  - Assets/Art/Iso/_aday/icon_item_chestnut.png
  - Assets/Art/Iso/_aday/icon_item_rosehip.png
  - Assets/Art/Iso/_aday/icon_item_pinecone.png
  - Assets/Art/Iso/_aday/icon_item_cress.png
  - Assets/Art/Iso/_aday/icon_item_sapling_birch.png
  - Assets/Art/Iso/_aday/icon_item_sapling_pine.png
  - Assets/Art/Iso/_aday/icon_item_sapling_willow.png
  - Assets/Art/Iso/_aday/icon_item_meat.png
  - Assets/Art/Iso/_aday/icon_item_poultry.png
  - Assets/Art/Iso/_aday/icon_item_fish_crayfish.png
  - Assets/Art/Iso/_aday/icon_item_fish_carp.png
  - Assets/Art/Iso/_aday/icon_item_fish_trout.png
  - Assets/Art/Iso/_aday/icon_item_fish_pike.png
  - Assets/Art/Iso/_aday/icon_item_fish_eel.png
  - Assets/Art/Iso/_aday/icon_item_game_meat.png
  - Assets/Art/Iso/_aday/icon_item_wheat.png
  - Assets/Art/Iso/_aday/icon_item_seed_wheat.png
  - Assets/Art/Iso/_aday/icon_item_rye.png
  - Assets/Art/Iso/_aday/icon_item_seed_rye.png
  - Assets/Art/Iso/_aday/icon_item_oats.png
  - Assets/Art/Iso/_aday/icon_item_seed_oats.png
  - Assets/Art/Iso/_aday/icon_item_flax.png
  - Assets/Art/Iso/_aday/icon_item_seed_flax.png
created: 2026-11-18
---
# 419 Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et (55)

## Amaç
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
| `item.seed_spinach` | `icon_item_seed_spinach.png` | `small cloth seed pouch with spinach seeds` |
| `item.apple` | `icon_item_apple.png` | `red apple` |
| `item.sapling_apple` | `icon_item_sapling_apple.png` | `apple sapling in earth` |
| `item.pear` | `icon_item_pear.png` | `green pear` |
| `item.sapling_pear` | `icon_item_sapling_pear.png` | `pear sapling` |
| `item.cherry` | `icon_item_cherry.png` | `pair of red cherries` |
| `item.sapling_cherry` | `icon_item_sapling_cherry.png` | `cherry sapling` |
| `item.plum` | `icon_item_plum.png` | `purple plum` |
| `item.sapling_plum` | `icon_item_sapling_plum.png` | `plum sapling` |
| `item.wild_garlic` | `icon_item_wild_garlic.png` | `bunch of wild garlic leaves` |
| `item.nettle` | `icon_item_nettle.png` | `bunch of nettles` |
| `item.elderflower` | `icon_item_elderflower.png` | `sprig of white elderflowers` |
| `item.wild_strawberry` | `icon_item_wild_strawberry.png` | `wild strawberries` |
| `item.chestnut` | `icon_item_chestnut.png` | `brown chestnuts` |
| `item.rosehip` | `icon_item_rosehip.png` | `red rosehips` |
| `item.pinecone` | `icon_item_pinecone.png` | `pinecone` |
| `item.cress` | `icon_item_cress.png` | `bunch of cress` |
| `item.sapling_birch` | `icon_item_sapling_birch.png` | `birch sapling` |
| `item.sapling_pine` | `icon_item_sapling_pine.png` | `pine sapling` |
| `item.sapling_willow` | `icon_item_sapling_willow.png` | `willow sapling` |
| `item.meat` | `icon_item_meat.png` | `raw cut of meat` |
| `item.poultry` | `icon_item_poultry.png` | `plucked raw chicken` |
| `item.fish_crayfish` | `icon_item_fish_crayfish.png` | `crayfish` |
| `item.fish_carp` | `icon_item_fish_carp.png` | `carp fish` |
| `item.fish_trout` | `icon_item_fish_trout.png` | `trout fish` |
| `item.fish_pike` | `icon_item_fish_pike.png` | `pike fish` |
| `item.fish_eel` | `icon_item_fish_eel.png` | `eel` |
| `item.game_meat` | `icon_item_game_meat.png` | `cut of dark game meat` |
| `item.wheat` | `icon_item_wheat.png` | `bundle of wheat ears` |
| `item.seed_wheat` | `icon_item_seed_wheat.png` | `small cloth seed pouch with wheat seeds` |
| `item.rye` | `icon_item_rye.png` | `bundle of rye ears` |
| `item.seed_rye` | `icon_item_seed_rye.png` | `small cloth seed pouch with rye seeds` |
| `item.oats` | `icon_item_oats.png` | `bundle of oats` |
| `item.seed_oats` | `icon_item_seed_oats.png` | `small cloth seed pouch with oats seeds` |
| `item.flax` | `icon_item_flax.png` | `bundle of flax stalks` |
| `item.seed_flax` | `icon_item_seed_flax.png` | `small cloth seed pouch with flax seeds` |

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

## Sorular
- (işçi, 2026-11-18) Hiç PixelLab çağrısı yapılmadı, harcama 0. `create_map_object` 32x32 çağrısı 1 gen (401/417 raporları); tarifteki 55 ikon = 55 gen. Bu, `cost_max: 34` ve `cost_est` 20'nin 2 katını (40) aşıyor, bütçe kuralı gereği durdum.
- 316 raporuna göre 6'lı sayfa üretimi ızgaraya uymadı; sayfa yöntemi güvenilir değil.
- Öneri: işi iki iş dosyasına böl (ör. 419a: ekin/tohum 28 ikon; 419b: fidan, toplanan, balık, et 27 ikon), her biri `cost_max <= 34`, ya da `cost_est`/`cost_max` değerlerini 55-60'a çıkar. Bakiye: 4041,7.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
