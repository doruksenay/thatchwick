---
id: "368"
title: Animasyon: kazma sallama, kova taşıma, destek kurma (4 yön)
status: review
claimed_at: 2026-11-18T20:03Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 28 (18.11.2026, "tek seferde yapalım")
priority: 50
family: karakter
style_gate: false
cost_est: 24
cost_max: 36
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim_mine_dig_{se,sw,nw,ne}.png
  - Assets/Art/Iso/_aday/anim_mine_bucket_{se,sw,nw,ne}.png
  - Assets/Art/Iso/_aday/anim_mine_support_{se,sw,nw,ne}.png
created: 2026-11-18
---
# 368 Animasyon: kazma sallama, kova taşıma, destek kurma (4 yön, 84 px)

## Amaç
Kazı işleri (TileAction.Dig, Bail, Support) için karakter animasyonu. Etkileşim animasyonları planı (018) ile aynı düzen.

## Tarif
Taban (018, kullanıcı kararı 18.11.2026): 84 px gerçekçi karakterler, erkek `c63fe39d-b168-4732-bf11-b476172c98e8`, kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d`. Yöntem 018c-e ile aynı (`animate_character`, `pixminimax`, `keep_first_frame=true`, `end_frame` = durma karesi, 4 yön: se, sw, nw, ne; kazma ve destek 6 kare, kova taşıma 8 kare). 3 anahtar x 2 karakter x 4 yön = 24 çağrı (24 gen). Ortak işleme kuralları: `art/queue/018c-animasyon-ciftlik-odun.md` 'Ortak işleme'. Çıktı adı: `iso_char_<man|woman>_r84_mine_<dig|bucket|support>_<yön>_<NN>.png`. Eylem metinleri:
- kazma: `swinging a pickaxe at rock in front, overhead strike`
- kova: `carrying a heavy wooden bucket of water in both hands`
- destek: `lifting a timber beam overhead and fixing it in place`
018a (yöntem pilotu) `review` ya da `accepted` olmadan başlamaz (depends_on).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-18: yöntem kapısı 018a3 kabul (aday d, pixminimax + simetrik adım metni); etkileşimlerde aynı pixminimax ve 018b2'deki ücretsiz hizalama kullanılır. -->
