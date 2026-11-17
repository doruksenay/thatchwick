---
id: "328"
title: Duvar kaplama dokuları: ahşap lambri ve duvar dokuması (iki eksen)
status: pending
approved_by: pending     # kullanıcı kararı bekliyor (Sims düzeyinde yapı modu, 2026-11-17); "user" yapınca işçi alır
family: yapi
style_gate: false
cost_est: 20
cost_max: 25
depends_on: ["325"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_finish_panel_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_panel_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_tapestry_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_tapestry_se_nw.png
created: 2026-11-17
---
# 328 Duvar kaplama dokuları

## Amaç
Duvar yüzü kaplamaları (`kind: wallFinish`): badana, aşı boyası, ince sıva ve desenli boya motorda renk olarak uygulanır (görsel gerekmez). Ahşap lambri ve duvar dokuması desenli olduğu için kendi dokusu gerekir; oyun `iso_wall_finish_panel_<eksen>` varsa kaplanan yüzü bununla çizer.

## Tarif
- Yöntem **008b ile aynı**: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`; kaynak 102x115 tuvalden 96x96 pencere kırpılır (008b'deki pencereler), çıktı aynı ofsetle 102x115 tuvale geri konur. Çağrı başına 5 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz).
- Kaynak: kabul edilmiş kit parçaları `iso_wall_wall_sw_ne.png`, `iso_wall_wall_se_nw.png` (kapılar için `iso_wall_door_*`, pencereler için 008b'nin `iso_wall_window_*`). Maske: duvar yüzü (çerçeve ve pivot dışı bölge değişmez), tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı kalır.
- Işık sol-üstten, gölge pişirilmez, palet `art/style` ana paletinden.
- Kaynak: varsayılan `iso_wall_wall_*`; yalnızca kameraya bakan yüz boyanır.
- Açıklamalar: lambri = lower half dark wooden wainscot panels, upper half cream plaster; duvar dokuması = hanging woven wall tapestry with deep red and ochre medieval pattern covering the wall face.

## Doğrulama
`python tools/art/validate_iso.py <dosya>...` ve `python tools/art/style.py check --family yapi <dosya>...`; her dosya 102x115, pivot kaynakla aynı, iki eksen aynı malzemeyi gösteriyor, varsayılan duvarla yan yana konunca dikiş yok (yan yana sayfası: `art/results/328-yanyana.png`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
