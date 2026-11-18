---
id: "402"
title: Alçak ve yarım duvar varyantları, eksikler (kaplamalar): betikle kırpma
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 18.11.2026 ("duvarların alçak görseli")
priority: 10
family: yapi
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["327", "328"]
max_attempts: 1
pixellab: false
outputs:
  - Assets/Art/Iso/iso_wall_finish_panel_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_panel_low_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_panel_half_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_panel_half_se_nw.png
  - Assets/Art/Iso/iso_wall_finish_tapestry_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_finish_tapestry_half_sw_ne.png
created: 2026-11-18
---
# 402 Alçak ve yarım duvar varyantları: eksik kalanlar

## Amaç
Kesit modunda ön duvarlar, Aşağı modunda bütün duvarlar **alçak** çizilir. Kod (18.11.2026, `WallView.cs`) artık alçak modda tam boy görsele hiç düşmüyor:
1. Parçanın `_low` görseli (`iso_wall_<parça>_low_<eksen>`) varsa o çizilir, opak.
2. Kapı ve pencere alçakta **düz alçak duvar** olur: aynı hattaki komşu duvarın malzemesinin `_low` görseli, yoksa `iso_wall_wall_low_*` (327 notu: kapı ve pencereye ayrı alçak varyant gerekmez).
3. Kaplama (`finish_*`) varsa önce kaplamanın `_low` görseli aranır; yoksa malzemenin alçak görseli kaplama rengiyle boyanır.
4. Hiçbiri yoksa yer tutucu alçak parça (renkli) çizilir.

Bugün eksik olan yalnızca dokulu kaplamaların alçak ve yarım hâli (lambri, duvar dokuması). Bunlar gelince kaplamalı duvar alçakta da kendi dokusunu gösterir.

## Tarif
- `pixellab: false`. 327'de yazılan `tools/art/wall_variants.py` kullanılır: tam görselden taban (pivot) satırından yukarı `low` için 10 px, `half` için 20 px bırakır; kesilen üst kenara 1 px açık ton kapak çizgisi koyar. Tuval ve pivot değişmez.
- `iso_wall_finish_tapestry` yalnızca `sw_ne` yönünde var; çıktı da yalnızca `sw_ne`.
- Teslim: `cp -n`, `.meta` yazılmaz (Unity üretir).

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`; kırpılmış dosyaların alt kenarı kaynakla piksel piksel aynı, üst kenar düz.

## Notlar
- `wall_wattle` (varsayılan dal-örgü) ve `wall_whitewash` malzemelerinin henüz kendi görseli yok; varsayılan duvar seti (`iso_wall_wall_*`, alçak hâli 008b'de var) kullanılıyor. Bu iki malzemenin görseli geldiğinde aynı betikle `_low` ve `_half` de üretilmeli.
- Oyunda kontrol: Duvar düğmesi → Kesit ve Aşağı; ön duvarlar kısa ve opak, kapı ve pencere boşluğu alçakta düz duvar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
