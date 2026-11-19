---
id: "418c"
title: Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü parça 3/3 (17)
status: pending
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
# 418c Ürün ikonları A: yemek, içecek, turşu, kurutma, tütsü parça 3/3 (17)

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
