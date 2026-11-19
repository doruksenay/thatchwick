---
id: "420d"
title: Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 4/4 (14)
status: running
claimed_at: 2026-11-19T07:16Z
approved_by: user        # kullanıcı 18.11.2026: "bu sistemi getir" (ürün ikon sistemi); otomatik akış (art/budget.md)
priority: 30
family: arayuz
style_gate: false
cost_est: 14
cost_max: 28
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
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
# 420d Ürün ikonları C: malzeme, alet, kumaş, iksir, inşaat parça 4/4 (14)

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
