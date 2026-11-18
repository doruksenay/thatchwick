---
id: "368"
title: Animasyon: kazma sallama, kova taşıma, destek kurma (4 yön)
status: blocked   # 018 (84 px animasyon planı) tarifini güncelleyip pending yapacak
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 28 (18.11.2026, "tek seferde yapalım")
priority: 50
family: karakter
style_gate: false
cost_est: 24
cost_max: 36
depends_on: ["018"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim_mine_dig_{se,sw,nw,ne}.png
  - Assets/Art/Iso/_aday/anim_mine_bucket_{se,sw,nw,ne}.png
  - Assets/Art/Iso/_aday/anim_mine_support_{se,sw,nw,ne}.png
created: 2026-11-18
---
# 368 Animasyon: kazma sallama, kova taşıma, destek kurma (4 yön)

## Amaç
Kazı işleri (TileAction.Dig, Bail, Support) için karakter animasyonu. Etkileşim animasyonları planı (018) ile aynı düzen.

## Tarif
Yöntem 018 planındaki etkileşim animasyonu tarifiyle aynı (animate_character, 4 yön, 6 kare). Eylem metinleri:
- kazma: `swinging a pickaxe at rock in front, overhead strike`
- kova: `carrying a heavy wooden bucket of water in both hands`
- destek: `lifting a timber beam overhead and fixing it in place`
018 henüz plan aşamasındaysa işçi durur ve `## Sorular` ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
