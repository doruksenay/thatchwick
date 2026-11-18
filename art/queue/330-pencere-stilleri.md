---
id: "330"
title: Pencere stilleri: mazgal, kafesli, geniş, kurşun çerçeveli, bodrum (iki eksen)
status: review
claimed_at: 2026-11-18T11:38Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: yapi
style_gate: false
cost_est: 50
cost_max: 55
depends_on: ["325", "008b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_window_slit_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_slit_se_nw.png
  - Assets/Art/Iso/iso_wall_window_lattice_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_lattice_se_nw.png
  - Assets/Art/Iso/iso_wall_window_wide_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_wide_se_nw.png
  - Assets/Art/Iso/iso_wall_window_leaded_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_leaded_se_nw.png
  - Assets/Art/Iso/iso_wall_window_cellar_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_cellar_se_nw.png
created: 2026-11-17
---
# 330 Pencere stilleri

## Amaç
`kind: window` parçaları. Varsayılan kepenkli pencere 008b'nin `iso_wall_window_*` görselidir. Bodrum penceresi yalnızca -1. katta kullanılır (duvarın üst kenarına yakın, küçük, yatay).

## Tarif
- Yöntem **008b ile aynı**: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`; kaynak 102x115 tuvalden 96x96 pencere kırpılır (008b'deki pencereler), çıktı aynı ofsetle 102x115 tuvale geri konur. Çağrı başına 5 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz).
- Kaynak: kabul edilmiş kit parçaları `iso_wall_wall_sw_ne.png`, `iso_wall_wall_se_nw.png` (kapılar için `iso_wall_door_*`, pencereler için 008b'nin `iso_wall_window_*`). Maske: duvar yüzü (çerçeve ve pivot dışı bölge değişmez), tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı kalır.
- Işık sol-üstten, gölge pişirilmez, palet `art/style` ana paletinden.
- Kaynak: 008b çıktısı `iso_wall_window_*`; maske yalnızca açıklık.
- Açıklamalar: mazgal = very narrow vertical slit window; kafesli = small window with diagonal wooden lattice; geniş = wide double window with open wooden shutters; kurşun çerçeveli = leaded glass window with small diamond panes; bodrum = small low horizontal window near the top of the wall with iron bars.

## Doğrulama
`python tools/art/validate_iso.py <dosya>...` ve `python tools/art/style.py check --family yapi <dosya>...`; her dosya 102x115, pivot kaynakla aynı, iki eksen aynı malzemeyi gösteriyor, varsayılan duvarla yan yana konunca dikiş yok (yan yana sayfası: `art/results/330-yanyana.png`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
