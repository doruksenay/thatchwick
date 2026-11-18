---
id: "327"
title: Alçak (kesit) ve yarım duvar varyantları: betikle kırpma
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: yapi
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["325", "326"]
max_attempts: 1
pixellab: false
outputs:
  - Assets/Art/Iso/iso_wall_wall_half_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_half_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_plank_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_plank_low_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_plank_half_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_plank_half_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_stone_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_stone_low_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_stone_half_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_stone_half_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_cob_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_cob_low_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_cob_half_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_cob_half_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_timber_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_timber_low_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_timber_half_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_timber_half_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_ashlar_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_ashlar_low_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_ashlar_half_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_ashlar_half_se_nw.png
created: 2026-11-17
---
# 327 Alçak ve yarım duvar varyantları

## Amaç
Kesit modunda ön duvarlar alçak (10 px), yarım duvar (`EdgeKind.HalfWall`) 20 px boyunda çizilir. Her malzeme için bu iki varyant yeni üretim yerine tam duvardan **kırpılarak** elde edilir (generation harcanmaz). Oyun `iso_wall_<parça>_low_<eksen>` ve `_half_` adlarını bulur.

## Tarif
- `pixellab: false`. `tools/art/` altında `wall_variants.py` yoksa yaz: tam duvar PNG'sini okur, taban (pivot) satırından yukarı doğru `low` için 10 px, `half` için 20 px duvar yüksekliği bırakır, üstünü siler; kesilen üst kenara 1 px açık ton kapak çizgisi koyar (duvar üstü). Tuval ve pivot değişmez.
- Varsayılan duvarın alçak hali 008b'de üretildi; burada yalnızca `iso_wall_wall_half_*` eklenir.
- Bulut işçisinde `tools/art` değişikliği PR'a gider (`art/README.md`); betik birleşince iş tamamlanır.

## Doğrulama
`validate_iso.py`; kırpılmış dosyaların alt kenarı kaynakla piksel piksel aynı (fark 0), üst kenar düz.

## Notlar
Kapı ve pencere alçak modda düz alçak duvar çizilir, ayrı varyant gerekmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
