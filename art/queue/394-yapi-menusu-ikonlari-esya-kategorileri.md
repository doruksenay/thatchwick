---
id: "394"
title: Yapı menüsü ikonları: eşya kategorileri
status: review
claimed_at: 2026-11-18T14:06Z
approved_by: user
priority: 15
family: arayuz
style_gate: false
cost_est: 18
cost_max: 24
depends_on: ["210"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_cat_seating.png
  - Assets/Art/Iso/_aday/ui_icon_cat_sleep.png
  - Assets/Art/Iso/_aday/ui_icon_cat_surfaces.png
  - Assets/Art/Iso/_aday/ui_icon_cat_storage.png
  - Assets/Art/Iso/_aday/ui_icon_cat_kids.png
  - Assets/Art/Iso/_aday/ui_icon_cat_kitchen.png
  - Assets/Art/Iso/_aday/ui_icon_cat_work.png
  - Assets/Art/Iso/_aday/ui_icon_cat_hygiene.png
  - Assets/Art/Iso/_aday/ui_icon_cat_heat_light.png
  - Assets/Art/Iso/_aday/ui_icon_cat_decor.png
  - Assets/Art/Iso/_aday/ui_icon_cat_decor_wall.png
  - Assets/Art/Iso/_aday/ui_icon_cat_rug.png
  - Assets/Art/Iso/_aday/ui_icon_cat_ceiling.png
  - Assets/Art/Iso/_aday/ui_icon_cat_garden.png
  - Assets/Art/Iso/_aday/ui_icon_cat_farm.png
  - Assets/Art/Iso/_aday/ui_icon_cat_barn.png
  - Assets/Art/Iso/_aday/ui_icon_cat_animals.png
  - Assets/Art/Iso/_aday/ui_icon_cat_structure.png
created: 2026-11-18
---
# 394 Yapı menüsü ikonları: eşya kategorileri

## Amaç
Eşya sekmesinin alt kategorileri (`ItemCatalog.Categories` kimlikleri) ve Hayvan sekmesi.

Kullanıcı kararı (2026-11-18, sohbet): "tek seferde yap, PixelLab kuyruğuna at". Bağlam: proje belgesi 33-menu-gorsel-dili-ve-asset-listesi; kod `Assets/Scripts/Game/BuildMode.cs`.

## Tarif
- **Yöntem (315 ve 212 dersleri):** sayfa üretimi kesilemedi ve 16x16 tek ikon okunmadı. Bu yüzden **her ikon ayrı çağrı**: `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, `seed=0`. İkon başına 1 gen.
- **Dil birliği (315 sorun 6):** hepsi **çerçevesiz**, tek nesne, 32x32 tuvalde 24-26 px, ortalı. Renk ≤ 12, 1 px koyu mürekkep (#2F2926) kontur, ışık sol-üstten.
- Palet: `"linen cream #E9DDC7, burgundy #7A2E3B, wheat gold #C89B3C, leaf green #5E7F45, hearth orange #D97A2B, ink #2F2926, wood browns, iron grey"` (marka paleti, proje belgesi 17).
- Stil referansı: varsa `Assets/Art/Iso/_aday/ui_icon_misc_coin.png` ve `ui_icon_act_craft.png` (315) `style_image` olarak; araç desteklemiyorsa yalnızca açıklamaya ek.
- Ortak açıklama eki: `, single pixel art UI icon, one object centered, no frame, no background, flat simple shading, clean dark outline, light from top-left, no text, no letters, no numbers`.
- Teslim: `Assets/Art/Iso/_aday/<ad>.png`, `cp -n`, `.meta` yazma. Var olan dosya varsa üzerine yazma, raporda yaz.

### İkonlar (18 çağrı)
| Dosya | Koddaki anahtar | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_cat_seating | seating | `wooden three-legged stool` |
| ui_icon_cat_sleep | sleep | `straw bed with a blanket` |
| ui_icon_cat_surfaces | surfaces | `simple wooden table` |
| ui_icon_cat_storage | storage | `wicker basket` |
| ui_icon_cat_kids | kids | `wooden toy horse` |
| ui_icon_cat_kitchen | kitchen | `wooden spoon and bowl` |
| ui_icon_cat_work | work | `spinning wheel` |
| ui_icon_cat_hygiene | hygiene | `wooden wash tub with soap` |
| ui_icon_cat_heat_light | heat_light | `lit candle in a holder` |
| ui_icon_cat_decor | decor | `small clay jug with flowers` |
| ui_icon_cat_decor_wall | decor_wall | `framed woven tapestry` |
| ui_icon_cat_rug | rug | `rolled woven rug` |
| ui_icon_cat_ceiling | ceiling | `hanging bundle of dried herbs` |
| ui_icon_cat_garden | garden | `small sapling in soil` |
| ui_icon_cat_farm | farm | `wooden hay rake` |
| ui_icon_cat_barn | barn | `small wooden chicken coop` |
| ui_icon_cat_animals | animals | `white hen` |
| ui_icon_cat_structure | structure | `wooden ladder and stairs` |

## Doğrulama
- Her ikon 32x32, yarı saydam 0, renk sayısı (≤ 12 hedef; aşanları raporda ⚠).
- Toplu önizleme x3, koyu (#333333) ve keten (#E9DDC7) zemin üstünde: `art/results/394-yanyana.png`. Aynı sayfadaki ikonlar birbirinden şekliyle ayırt ediliyor mu (renk körü dostu)?
- `style.py check --family arayuz` (UYARI raporda).
- Okunmayan ikon için yeniden deneme yok; raporda listele.

## Sorular
- 10 ikon okunmadığı için teslim edilmedi (animals, barn, decor_wall, farm, hygiene, rug, storage, structure, surfaces, work). Ayrıntı: `art/results/394-yapi-menusu-ikonlari.md`. Yeniden üretim için yeni iş (pro model ya da farklı seed) gerekir.

## Notlar
- Ad kuralı `ui_icon_<grup>_<anahtar>` (315 ile aynı). Kod tarafı anahtar → dosya eşlemesini yapar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
