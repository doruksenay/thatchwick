---
id: "340"
title: Yol karoları: patika, çakıl, kaldırım (sağlam ve yıpranmış)
status: review
claimed_at: 2026-11-18T09:30Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 10
family: zemin
style_gate: false
cost_est: 16
cost_max: 22
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_road_path_a.png
  - Assets/Art/Iso/iso_road_path_b.png
  - Assets/Art/Iso/iso_road_gravel_a.png
  - Assets/Art/Iso/iso_road_gravel_worn.png
  - Assets/Art/Iso/iso_road_paved_a.png
  - Assets/Art/Iso/iso_road_paved_worn.png
  - Assets/Art/Iso/iso_road_paved_broken.png
created: 2026-11-17
---
# 340 Yol karoları

## Amaç
Yeni yol katmanı (zeminin üstünde, yalnızca açık havada). Patika x1,6, çakıl x1,8, kaldırım x2 hız verir; üstünden geçildikçe aşınır. Sık geçilen çim kendiliğinden patikaya döner (silik patika motorda çimle karıştırılır, ayrı görsel gerekmez). Bozuk patika ve yıpranmış patika motorda koyulaştırılır.

## Tarif
- Yöntem **002 ile aynı**: `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin karoları; 64x32 elmas, gölgesiz, dikişsiz döşenir. Çağrıdan önce maliyeti yaz.
- Açıklamalar: patika = trodden bare earth footpath with faint footprints and a few grass tufts at the edges; çakıl = compacted pale gravel road; çakıl yıpranmış = gravel road with ruts and potholes; kaldırım = medieval cobblestone road; kaldırım yıpranmış = cobblestones with gaps and mud; kaldırım bozuk = broken cobblestones, missing stones, puddles.
- Kenarlar komşu çim karosuna dikişsiz geçmeli (yol karosu yan yana ve çimin yanında döşenir).

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`, 3x3 döşeme testi (yol + çim kenarı).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
