---
id: "860"
title: "Sıcak çim karoları pilotu: cozy ana çim (3 çeşit) ve yumuşak toprak"
status: pending
approved_by: user        # kullanıcı 21.11.2026 02:29: "daha cozy ağaçlar ve tilelar yaptıralım"
priority: 10
family: zemin
style_gate: true         # kabul edilirse geçiş setleri (614, 615, 617, 770 ve yol geçişleri) bu tona göre yeniden yazılır
cost_est: 10
cost_max: 24
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_ground_grass_cozy_{a,b,c}.png
  - Assets/Art/Iso/_aday/iso_ground_dirt_cozy_a.png
  - art/results/860-*.md
created: 2026-11-21
---
# 860 Sıcak çim karoları pilotu

## Amaç
Unity testi (kullanıcı, 21.11.2026): harita "cozy" durmuyor. Ana çim (`iso_ground_grass_a/b/c`) parlak, soğuk yeşil ve ince karıncalı gürültülü; geniş alanda düz, yapay bir halı gibi görünüyor, karo ızgarası seçiliyor. Hedef: Stardew Valley, Fields of Mistria ve Ooblets çimlerindeki gibi **sıcak, yumuşak, elle boyanmış** bir zemin: göz dinlenir, üstündeki eşyalar öne çıkar (tasarım dili, Project belgesi 68b: %70 sakin alan).

Bu iş pilottur: yalnızca ana çim ve düz toprak. Kabul edilirse geçiş setleri ayrı işlerle bu tona taşınır.

## Tarif
- Araç: `create_tiles_pro`, bağımsız karolar; isometric, `tile_size=64`, `tile_view_angle=30`, `tile_depth_ratio=0`, `tile_flat_top_px=2`, `outline_mode=segmentation`, `max_colors=6`. Çağrıdan önce maliyeti yaz.
- Çim açıklaması: `soft warm meadow grass ground, hand-painted cozy farming game style, gentle large clumps of slightly lighter and darker green blades, warm olive and sage greens with a hint of yellow, low contrast, calm and quiet, no flowers, no stones, no fine pixel noise, no grid lines, seamless, top face only, no shadow`.
  - a: temel; b: aynı ton, öbekler farklı yerde; c: aynı ton, 2-3 çok küçük açık sarı-yeşil tutam vurgusu (çiçek değil).
- Toprak açıklaması: `soft packed earth ground, warm light brown, a few faint small pebbles, gentle low-contrast texture, hand-painted cozy farming game style, seamless, top face only, no shadow`.
- Aday setten her biri için en iyisini seç: dikişsiz, kenar tonları karo içiyle aynı, ince gürültü yok. Renk: `python tools/art/style.py fit --family zemin <girdi> <çıktı>`; ancak hedef ton bilinçli olarak eski çimden **daha sıcak ve daha az doygun**: `fit` tonu eski çime geri çekiyorsa `fit` uygulanmaz, `check` sonucu raporda UYARI olarak yazılır (KALDI yalnızca palet dışı ışık/parlaklık için).

## Doğrulama
- `python tools/art/validate_iso.py <dosya> --kind ground --max-colors 8`
- Döşeme testi: 12x12 elmas ızgarada a/b/c karışık döş (`art/results/860-doseme.png`); dikiş / karo içi parlaklık oranı 1,25 altı. Izgara çizgisi gözle seçilmemeli.
- Yan yana sayfası (`art/results/860-yanyana.png`, 2 kat): eski `iso_ground_grass_a` döşemesi ile yeni çim döşemesi; üstlerine aynı birkaç görsel (`iso_tree_oak_grown_se_v2`, `iso_item_well_se`, `iso_char_man_r84_idle_se_00`, bir patika karosu) konur. Yeni zeminde eşyalar daha rahat okunmalı.

## Notlar
- Dosyalar `_aday/` altına; oyun eski adları kullanmaya devam eder. Kabul edilirse ad değişimi ve geçiş setleri ayrı iş.
- Gölge pişirilmez; 64x32 elmas, tam 2:1, saydam arka plan, anti-alias yok (`docs/gorsel-yonerge.md`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
