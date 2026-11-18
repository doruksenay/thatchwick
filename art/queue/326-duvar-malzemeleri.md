---
id: "326"
title: Duvar malzemeleri: kerpiç, ahşap çatkı, kesme taş (iki eksen)
status: running
claimed_at: 2026-11-18T11:31Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: yapi
style_gate: false
cost_est: 30
cost_max: 35
depends_on: ["325"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_wall_cob_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_cob_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_timber_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_timber_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_ashlar_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_ashlar_se_nw.png
created: 2026-11-17
---
# 326 Duvar malzemeleri: kerpiç, ahşap çatkı, kesme taş

## Amaç
325 pilotuyla aynı yöntemle kalan malzemeler. Fiyat ve yalıtım `buildParts`'ta: kerpiç (Sv2), ahşap çatkı (Sv2), kesme taş (Sv7).

## Tarif
- Yöntem **008b ile aynı**: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`; kaynak 102x115 tuvalden 96x96 pencere kırpılır (008b'deki pencereler), çıktı aynı ofsetle 102x115 tuvale geri konur. Çağrı başına 5 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz).
- Kaynak: kabul edilmiş kit parçaları `iso_wall_wall_sw_ne.png`, `iso_wall_wall_se_nw.png` (kapılar için `iso_wall_door_*`, pencereler için 008b'nin `iso_wall_window_*`). Maske: duvar yüzü (çerçeve ve pivot dışı bölge değişmez), tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı kalır.
- Işık sol-üstten, gölge pişirilmez, palet `art/style` ana paletinden.
- Açıklamalar: kerpiç = sun-dried mud brick wall with straw, ochre earth tones; ahşap çatkı = exposed dark oak timber frame with cream plaster infill (007'deki kabul edilen görünüm); kesme taş = dressed ashlar stone blocks, regular courses, light grey. Hepsi flat few-tone pixel art, no shadow.

## Doğrulama
`python tools/art/validate_iso.py <dosya>...` ve `python tools/art/style.py check --family yapi <dosya>...`; her dosya 102x115, pivot kaynakla aynı, iki eksen aynı malzemeyi gösteriyor, varsayılan duvarla yan yana konunca dikiş yok (yan yana sayfası: `art/results/326-yanyana.png`).

## Notlar
325 kabul edilmeden başlamaz (stil pilotu).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
