---
id: "329"
title: Kapı stilleri: çaprazlı, yarım, demir kuşaklı, kemerli (iki eksen)
status: accepted
claimed_at: 2026-11-18T11:38Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: yapi
style_gate: false
cost_est: 40
cost_max: 45
depends_on: ["325"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_door_ledged_sw_ne.png
  - Assets/Art/Iso/iso_wall_door_ledged_se_nw.png
  - Assets/Art/Iso/iso_wall_door_dutch_sw_ne.png
  - Assets/Art/Iso/iso_wall_door_dutch_se_nw.png
  - Assets/Art/Iso/iso_wall_door_iron_sw_ne.png
  - Assets/Art/Iso/iso_wall_door_iron_se_nw.png
  - Assets/Art/Iso/iso_wall_door_arched_sw_ne.png
  - Assets/Art/Iso/iso_wall_door_arched_se_nw.png
created: 2026-11-17
---
# 329 Kapı stilleri

## Amaç
`kind: door` parçaları. Varsayılan kalas kapı mevcut `iso_wall_door_*` görselidir; bu iş dört stili ekler.

## Tarif
- Yöntem **008b ile aynı**: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`; kaynak 102x115 tuvalden 96x96 pencere kırpılır (008b'deki pencereler), çıktı aynı ofsetle 102x115 tuvale geri konur. Çağrı başına 5 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz).
- Kaynak: kabul edilmiş kit parçaları `iso_wall_wall_sw_ne.png`, `iso_wall_wall_se_nw.png` (kapılar için `iso_wall_door_*`, pencereler için 008b'nin `iso_wall_window_*`). Maske: duvar yüzü (çerçeve ve pivot dışı bölge değişmez), tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı kalır.
- Işık sol-üstten, gölge pişirilmez, palet `art/style` ana paletinden.
- Kaynak: `iso_wall_door_sw_ne.png`, `iso_wall_door_se_nw.png`; maske yalnızca kapı kanadı (duvar ve kasa korunur), kemerli kapıda kasanın üstü kemere açılır.
- Açıklamalar: çaprazlı = ledged and braced plank door with Z brace; yarım = dutch stable door split in two halves, top half open; demir kuşaklı = heavy oak door with black iron straps and studs; kemerli = arched oak door with stone arch.

## Doğrulama
`python tools/art/validate_iso.py <dosya>...` ve `python tools/art/style.py check --family yapi <dosya>...`; her dosya 102x115, pivot kaynakla aynı, iki eksen aynı malzemeyi gösteriyor, varsayılan duvarla yan yana konunca dikiş yok (yan yana sayfası: `art/results/329-yanyana.png`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
