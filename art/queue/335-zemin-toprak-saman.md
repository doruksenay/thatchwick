---
id: "335"
title: Zemin stilleri: sıkıştırılmış toprak (iç), saman, saz serili
status: running
claimed_at: 2026-11-18T11:44Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: zemin
style_gate: false
cost_est: 15
cost_max: 20
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_floor_earth_a.png
  - Assets/Art/Iso/iso_floor_earth_b.png
  - Assets/Art/Iso/iso_floor_straw_a.png
  - Assets/Art/Iso/iso_floor_straw_b.png
  - Assets/Art/Iso/iso_floor_rush_a.png
  - Assets/Art/Iso/iso_floor_rush_b.png
created: 2026-11-17
---
# 335 Zemin stilleri: toprak

## Amaç
`floorBase: Dirt` parçaları. Dış toprak (`iso_ground_dirt_*`) ekin toprağıdır; iç mekân sıkıştırılmış toprağı ayrı karodur (daha düz, ayak izi yok).

## Tarif
- Yöntem **002 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları; 64x32 elmas, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.
- Açıklamalar: sıkıştırılmış toprak = smooth packed earth floor; saman = loose straw scattered on earth; saz serili = rushes strewn on earth floor.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`, 8x8 döşeme testi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-18: yeniden `pending` (Claude, kurtarma): 2026-11-18T09:25Z sahiplenmesinden sonuç gelmedi.
