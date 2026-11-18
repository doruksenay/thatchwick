---
id: "332"
title: Çit kapıları ve korkuluklar (iki eksen)
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: yapi
style_gate: false
cost_est: 50
cost_max: 55
depends_on: ["331"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_gate_wood_sw_ne.png
  - Assets/Art/Iso/iso_wall_gate_wood_se_nw.png
  - Assets/Art/Iso/iso_wall_gate_wicket_sw_ne.png
  - Assets/Art/Iso/iso_wall_gate_wicket_se_nw.png
  - Assets/Art/Iso/iso_wall_railing_wood_sw_ne.png
  - Assets/Art/Iso/iso_wall_railing_wood_se_nw.png
  - Assets/Art/Iso/iso_wall_railing_turned_sw_ne.png
  - Assets/Art/Iso/iso_wall_railing_turned_se_nw.png
  - Assets/Art/Iso/iso_wall_railing_iron_sw_ne.png
  - Assets/Art/Iso/iso_wall_railing_iron_se_nw.png
created: 2026-11-17
---
# 332 Çit kapıları ve korkuluklar

## Amaç
`EdgeKind.Gate` (çit hattında geçilebilir) ve `EdgeKind.Railing` (balkon ve merdiven boşluğu kenarı; üst katta). 331 çitleriyle aynı yöntem ve boy.

## Tarif
- Yöntem **008b ile aynı**: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`; kaynak 102x115 tuvalden 96x96 pencere kırpılır (008b'deki pencereler), çıktı aynı ofsetle 102x115 tuvale geri konur. Çağrı başına 5 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz).
- Kaynak: kabul edilmiş kit parçaları `iso_wall_wall_sw_ne.png`, `iso_wall_wall_se_nw.png` (kapılar için `iso_wall_door_*`, pencereler için 008b'nin `iso_wall_window_*`). Maske: duvar yüzü (çerçeve ve pivot dışı bölge değişmez), tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı kalır.
- Işık sol-üstten, gölge pişirilmez, palet `art/style` ana paletinden.
- Kaynak: 331'de kabul edilen örgü çit; maske kenar bölgesi.
- Açıklamalar: bahçe kapısı = wooden garden gate with diagonal brace between two posts; küçük çit kapısı = small wicket gate; ahşap korkuluk = simple wooden balcony railing; torna korkuluk = railing with turned wooden balusters; dövme demir korkuluk = black wrought iron railing.

## Doğrulama
`python tools/art/validate_iso.py <dosya>...` ve `python tools/art/style.py check --family yapi <dosya>...`; her dosya 102x115, pivot kaynakla aynı, iki eksen aynı malzemeyi gösteriyor, varsayılan duvarla yan yana konunca dikiş yok (yan yana sayfası: `art/results/332-yanyana.png`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
