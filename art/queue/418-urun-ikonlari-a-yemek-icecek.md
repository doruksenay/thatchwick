---
id: "418"
title: Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü (55)
status: pending
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
  - Assets/Art/Iso/_aday/icon_item_salted_fish.png
  - Assets/Art/Iso/_aday/icon_item_smoked_fish.png
  - Assets/Art/Iso/_aday/icon_item_bread.png
  - Assets/Art/Iso/_aday/icon_item_ale.png
  - Assets/Art/Iso/_aday/icon_item_cider.png
  - Assets/Art/Iso/_aday/icon_item_mead.png
  - Assets/Art/Iso/_aday/icon_item_yogurt.png
  - Assets/Art/Iso/_aday/icon_item_butter.png
  - Assets/Art/Iso/_aday/icon_item_ayran.png
  - Assets/Art/Iso/_aday/icon_item_boza.png
  - Assets/Art/Iso/_aday/icon_item_serbet.png
  - Assets/Art/Iso/_aday/icon_item_vinegar.png
  - Assets/Art/Iso/_aday/icon_item_tarhana.png
  - Assets/Art/Iso/_aday/icon_item_soup_bean.png
  - Assets/Art/Iso/_aday/icon_item_soup_fish.png
  - Assets/Art/Iso/_aday/icon_item_stew_meat.png
  - Assets/Art/Iso/_aday/icon_item_soup_chicken.png
  - Assets/Art/Iso/_aday/icon_item_saute_mushroom.png
  - Assets/Art/Iso/_aday/icon_item_kaygana.png
  - Assets/Art/Iso/_aday/icon_item_breakfast.png
  - Assets/Art/Iso/_aday/icon_item_pilaf.png
  - Assets/Art/Iso/_aday/icon_item_sarma.png
  - Assets/Art/Iso/_aday/icon_item_hosaf.png
  - Assets/Art/Iso/_aday/icon_item_helva.png
  - Assets/Art/Iso/_aday/icon_item_pekmez.png
  - Assets/Art/Iso/_aday/icon_item_stew_game.png
  - Assets/Art/Iso/_aday/icon_item_wrap_fish.png
  - Assets/Art/Iso/_aday/icon_item_borek_leek.png
  - Assets/Art/Iso/_aday/icon_item_kete_meat.png
  - Assets/Art/Iso/_aday/icon_item_tart_apple.png
  - Assets/Art/Iso/_aday/icon_item_simit.png
  - Assets/Art/Iso/_aday/icon_item_rusk.png
  - Assets/Art/Iso/_aday/icon_item_bread_rye.png
  - Assets/Art/Iso/_aday/icon_item_bread_maslin.png
  - Assets/Art/Iso/_aday/icon_item_porridge_oats.png
  - Assets/Art/Iso/_aday/icon_item_porridge_wheat.png
created: 2026-11-18
---
# 418 Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü (55)

## Amaç
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
| `item.salted_fish` | `icon_item_salted_fish.png` | `salted dried fish` |
| `item.smoked_fish` | `icon_item_smoked_fish.png` | `smoked golden fish` |
| `item.bread` | `icon_item_bread.png` | `round loaf of wheat bread` |
| `item.ale` | `icon_item_ale.png` | `clay mug of brown ale` |
| `item.cider` | `icon_item_cider.png` | `clay mug of apple cider` |
| `item.mead` | `icon_item_mead.png` | `clay cup of mead` |
| `item.yogurt` | `icon_item_yogurt.png` | `bowl of yogurt` |
| `item.butter` | `icon_item_butter.png` | `pat of butter on a dish` |
| `item.ayran` | `icon_item_ayran.png` | `clay cup of salted yogurt drink` |
| `item.boza` | `icon_item_boza.png` | `clay cup of thick millet drink` |
| `item.serbet` | `icon_item_serbet.png` | `clay cup of fruit sherbet` |
| `item.vinegar` | `icon_item_vinegar.png` | `clay bottle of vinegar` |
| `item.tarhana` | `icon_item_tarhana.png` | `bowl of tarhana soup` |
| `item.soup_bean` | `icon_item_soup_bean.png` | `bowl of bean soup` |
| `item.soup_fish` | `icon_item_soup_fish.png` | `bowl of fish soup` |
| `item.stew_meat` | `icon_item_stew_meat.png` | `bowl of meat stew` |
| `item.soup_chicken` | `icon_item_soup_chicken.png` | `bowl of chicken soup` |
| `item.saute_mushroom` | `icon_item_saute_mushroom.png` | `pan of fried mushrooms` |
| `item.kaygana` | `icon_item_kaygana.png` | `folded egg pancake on plate` |
| `item.breakfast` | `icon_item_breakfast.png` | `plate with bread, cheese and egg` |
| `item.pilaf` | `icon_item_pilaf.png` | `bowl of pilaf` |
| `item.sarma` | `icon_item_sarma.png` | `stuffed vine leaves on plate` |
| `item.hosaf` | `icon_item_hosaf.png` | `bowl of dried fruit compote` |
| `item.helva` | `icon_item_helva.png` | `slab of flour halva` |
| `item.pekmez` | `icon_item_pekmez.png` | `jar of grape molasses` |
| `item.stew_game` | `icon_item_stew_game.png` | `bowl of game stew` |
| `item.wrap_fish` | `icon_item_wrap_fish.png` | `flatbread fish wrap` |
| `item.borek_leek` | `icon_item_borek_leek.png` | `baked leek pastry slice` |
| `item.kete_meat` | `icon_item_kete_meat.png` | `meat-filled bread bun` |
| `item.tart_apple` | `icon_item_tart_apple.png` | `apple tart` |
| `item.simit` | `icon_item_simit.png` | `ring-shaped bread with seeds` |
| `item.rusk` | `icon_item_rusk.png` | `dry twice-baked bread slices` |
| `item.bread_rye` | `icon_item_bread_rye.png` | `dark rye bread loaf` |
| `item.bread_maslin` | `icon_item_bread_maslin.png` | `round loaf of mixed grain bread` |
| `item.porridge_oats` | `icon_item_porridge_oats.png` | `bowl of oat porridge` |
| `item.porridge_wheat` | `icon_item_porridge_wheat.png` | `bowl of wheat porridge` |

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
