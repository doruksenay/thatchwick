---
id: "419b"
title: Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et parça 2/3 (19)
status: running
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
created: 2026-11-18
---
# 419b Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et parça 2/3 (19)

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

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
