---
id: "325"
title: Duvar malzemesi pilotu: kalas ve taş (iki eksen)
status: review
claimed_at: 2026-11-18T08:42Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: yapi
style_gate: true
cost_est: 20
cost_max: 25
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_wall_plank_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_plank_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_stone_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_stone_se_nw.png
created: 2026-11-17
---
# 325 Duvar malzemesi pilotu: kalas ve taş

## Amaç
Yapı modunda duvar malzemesi seçilebiliyor (`items.json` → `buildParts`, `kind: wallMaterial`). Görsel gelene kadar malzemeler yer tutucu renkle çiziliyor. Bu iş malzeme ailesinin stil pilotudur: kabul edilirse 326-332 aynı yöntemle türer. Oyun `iso_wall_<parça>_<eksen>.png` adını kendiliğinden bulur (`IsoArt.WallPart`).

## Tarif
- Parçalar: `wall_plank` (Kalas: yatay geniş kalaslar, çivi başları, koyu ceviz kahve), `wall_stone` (Taş: düzensiz moloz taş, kireç harçlı, gri-bej).
- Yöntem **008b ile aynı**: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`; kaynak 102x115 tuvalden 96x96 pencere kırpılır (008b'deki pencereler), çıktı aynı ofsetle 102x115 tuvale geri konur. Çağrı başına 5 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz).
- Kaynak: kabul edilmiş kit parçaları `iso_wall_wall_sw_ne.png`, `iso_wall_wall_se_nw.png` (kapılar için `iso_wall_door_*`, pencereler için 008b'nin `iso_wall_window_*`). Maske: duvar yüzü (çerçeve ve pivot dışı bölge değişmez), tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı kalır.
- Işık sol-üstten, gölge pişirilmez, palet `art/style` ana paletinden.
- Açıklama (kalas): medieval village wall made of wide horizontal wooden planks, visible nail heads, weathered brown, flat few-tone pixel art, no shadow.
- Açıklama (taş): medieval rubble stone wall with lime mortar, grey beige stones, flat few-tone pixel art, no shadow.

## Doğrulama
`python tools/art/validate_iso.py <dosya>...` ve `python tools/art/style.py check --family yapi <dosya>...`; her dosya 102x115, pivot kaynakla aynı, iki eksen aynı malzemeyi gösteriyor, varsayılan duvarla yan yana konunca dikiş yok (yan yana sayfası: `art/results/325-yanyana.png`).

## Notlar
Kesit (alçak) ve yarım duvar varyantları 327'de betikle kırpılır; burada üretilmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
