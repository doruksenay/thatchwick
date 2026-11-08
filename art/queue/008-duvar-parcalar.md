---
id: "008"
title: Duvar parçaları (düz ve köşe, iki eksen)
status: pending
approved_by: auto
cost_est: 15
cost_max: 25
depends_on: ["007"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_straight_se_nw.png
  - Assets/Art/Iso/iso_wall_straight_sw_ne.png
  - Assets/Art/Iso/iso_wall_corner_se.png
  - Assets/Art/Iso/iso_wall_corner_sw.png
  - Assets/Art/Iso/iso_wall_corner_nw.png
  - Assets/Art/Iso/iso_wall_corner_ne.png
created: 2026-11-08
---
# 008 Duvar parçaları

## Amaç
Yapı modu (Adım 10): her iki eksen için düz duvar ve dört köşe. Kabul edilen malzeme örneğiyle (007) aynı görünüm.

## Tarif
- Araç: `create_building_kit`, isometric 64; `wall_tiles=1` ise 15, aksi halde 25 generation (çağrıdan önce araç açıklamasındaki maliyeti yaz). Malzeme: 007'de kabul edilen.
- Yalnızca duvar (çatı yok), gölge yok. Düz duvar parçası bir karo kenarına oturur; köşe parçası iki kenarın birleştiği karo.
- Hangi kit karosunun hangi dosyaya karşılık geldiğini rapora tablo olarak yaz.

## Doğrulama
`validate_iso.py` (her dosya), 2:1 kenarlar, ışık (sol yüz orta, sağ yüz en koyu), parçaların yan yana döşenince dikişsiz birleşmesi (düz+düz, düz+köşe), pivot kuralı (ayak izi taban elmasının merkezi).

## Notlar
Adlandırma `docs/gorsel-yonerge.md`'deki kural. `cost_max` 25'i aşacaksa `blocked` yap ve sor.
