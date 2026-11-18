---
id: "331"
title: Çitler: örgü, çıtalı, budanmış çit bitkisi, alçak taş duvar (iki eksen)
status: running
claimed_at: 2026-11-18T11:44Z
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
  - Assets/Art/Iso/iso_wall_fence_wattle_sw_ne.png
  - Assets/Art/Iso/iso_wall_fence_wattle_se_nw.png
  - Assets/Art/Iso/iso_wall_fence_picket_sw_ne.png
  - Assets/Art/Iso/iso_wall_fence_picket_se_nw.png
  - Assets/Art/Iso/iso_wall_fence_hedge_sw_ne.png
  - Assets/Art/Iso/iso_wall_fence_hedge_se_nw.png
  - Assets/Art/Iso/iso_wall_fence_stone_sw_ne.png
  - Assets/Art/Iso/iso_wall_fence_stone_se_nw.png
created: 2026-11-17
---
# 331 Çitler

## Amaç
`EdgeKind.Fence` (`kind: fence`): odayı bölmez ama geçilmez; bahçe ve hayvan alanı için. Kenar parçası olarak duvarla aynı tuval ve pivotta çizilir, boyu ~14 px.

## Tarif
- Yöntem **008b ile aynı**: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`; kaynak 102x115 tuvalden 96x96 pencere kırpılır (008b'deki pencereler), çıktı aynı ofsetle 102x115 tuvale geri konur. Çağrı başına 5 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz).
- Kaynak: kabul edilmiş kit parçaları `iso_wall_wall_sw_ne.png`, `iso_wall_wall_se_nw.png` (kapılar için `iso_wall_door_*`, pencereler için 008b'nin `iso_wall_window_*`). Maske: duvar yüzü (çerçeve ve pivot dışı bölge değişmez), tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı kalır.
- Işık sol-üstten, gölge pişirilmez, palet `art/style` ana paletinden.
- Kaynak: 008b'nin alçak duvarı `iso_wall_wall_low_*` (tuval, pivot ve kenar çizgisi için); maske alçak duvar bölgesi ve üstündeki ~10 px, arkası saydam kalır.
- Açıklamalar: örgü çit = woven wattle hurdle fence of hazel branches; çıtalı çit = simple wooden picket fence; çit bitkisi = low trimmed green hedge; alçak taş duvar = low dry stone field wall.

## Doğrulama
`python tools/art/validate_iso.py <dosya>...` ve `python tools/art/style.py check --family yapi <dosya>...`; her dosya 102x115, pivot kaynakla aynı, iki eksen aynı malzemeyi gösteriyor, varsayılan duvarla yan yana konunca dikiş yok (yan yana sayfası: `art/results/331-yanyana.png`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
