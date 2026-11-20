---
id: "455c"
title: Seviye kartı çerçevesi ve beceri çubuğu
status: running
claimed_at: 2026-11-20T07:07Z
approved_by: user
priority: 20
family: arayuz
style_gate: false
cost_est: 4
cost_max: 8
depends_on: ["455a"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_skill_card_frame.png
  - Assets/Art/Iso/_aday/ui_skill_bar_frame.png
created: 2026-11-20
---
# 455c Seviye kartı çerçevesi ve beceri çubuğu

## Amaç
Belge 45 bölüm 10: seviye atlayınca ekranın sağ üstünde çıkan kart (240x~100 px, 9-dilim) ve karakterin başının üstündeki ince beceri çubuğunun çerçevesi (110x7 px, 3-dilim). Kod şimdilik düz renkle çiziyor (`StatusHud.DrawLevelCards`, `DrawSkillFlash`); dosyalar gelince bağlanır.

## Tarif
`create_image_pixflux`, `no_background=true`.
| Dosya | Boyut | Açıklama |
|---|---|---|
| ui_skill_card_frame | 64x64 (9-dilim, kenar 12 px) | `cross-stitch embroidered linen panel, cream linen center, burgundy stitched border with small wheat-gold corner knots, flat, no text` |
| ui_skill_bar_frame | 64x8 (3-dilim, kenar 3 px) | `thin horizontal embroidered bar frame, dark brown stitched outline, empty cream inside, flat, no text` |
Marka dili belge 17 (nakış, bordo yazı). 16 renk, yarı saydam 0.

## Doğrulama
9-dilimde orta alan düz (desensiz) olmalı; esneyince dikiş tekrarlanmamalı. `art/results/455c-yanyana.png`: kart 240x100 ve çubuk 110x7 ölçeklenmiş hâliyle.

## Notlar
Kabul edilince `Assets/Resources/UI/` altına kopyalanır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
