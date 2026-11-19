---
id: "418b"
title: Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü parça 2/3 (19)
status: running
claimed_at: 2026-11-19T06:57Z
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
created: 2026-11-18
---
# 418b Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü parça 2/3 (19)

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

## Doğrulama
Boyut 32x32, ≤16 renk, `python tools/art/style.py check --family arayuz <dosyalar>`. Yüz/göz/figür içeren ikon teslim edilmez (red klasörü). Sonuç tablosunda teslim edilmeyenleri listele.

## Notlar
- Dosya adı `icon_item_<id>` olduğu sürece kod kendiliğinden bulur; v2 gibi farklı ad gerekirse kod oturumu `IsoArt.IconOverrides`'a ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
