---
id: "419c"
title: Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et parça 3/3 (17)
status: running
claimed_at: 2026-11-19T07:03Z
approved_by: user        # kullanıcı 18.11.2026: "bu sistemi getir" (ürün ikon sistemi); otomatik akış (art/budget.md)
priority: 30
family: arayuz
style_gate: false
cost_est: 17
cost_max: 34
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
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
# 419c Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan, balık, et parça 3/3 (17)

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

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
