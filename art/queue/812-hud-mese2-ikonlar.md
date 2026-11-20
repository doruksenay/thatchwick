---
id: "812"
title: "HUD meşe, dünya ölçeği: ihtiyaç, hız ve mevsim ikonları (15)"
status: running
claimed_at: 2026-11-20T23:08Z
approved_by: user        # kullanıcı kararı 2026-11-21: saf meşe; HUD dünya ile aynı piksel boyutu (2x); üç iş toplam üst sınır 40
priority: 1
family: arayuz
style_gate: false
cost_est: 15
cost_max: 25
depends_on: []
max_attempts: 2
pixellab: true
yon: mese2
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_mese2_{need_hunger,need_energy,need_warmth,need_fun,need_hygiene,need_social,need_toilet}.png
  - Assets/Art/Iso/_aday/ui_hud2_mese2_{speed_pause,speed_1,speed_2,speed_3,season_spring,season_summer,season_autumn,season_winter}.png
created: 2026-11-21
---
# 812 HUD meşe (dünya ölçeği): ikonlar

## Ortak kurallar (810, 811, 812)
- **Karar (kullanıcı 2026-11-21):** HUD yönü oyma meşe (745 kabul). HUD dünya ile **aynı piksel boyutunda**: yerel düzen 480x270 (Project belgesi 66 §3), oyunda 2x, telefonda 3x. 774'ün kestiği `ui_hud2_mese_*` parçaları bu ölçeğin yaklaşık iki katı; yalnızca kılavuzdur, teslim edilmez ve üzerine yazılmaz.
- **Ad:** `Assets/Art/Iso/_aday/ui_hud2_mese2_<parça>.png` (774 çıktılarıyla çakışmasın diye `mese2`).
- **Kılavuz:** ilgili 774 parçası `art/tmp/` altında en yakın komşuyla native boyuta indirilir ve **yalnızca `init_image`** olarak verilir (güç 300-450). Teslim edilen her zaman PixelLab çıktısıdır; ölçek düşürülmüş kılavuz teslim edilmez.
- **Araç sınırı:** `create_image_pixflux` en küçük kenar 32; native kenar daha küçükse tuval 32 yapılır, içerik (ve kılavuz) native kutuya ortalanır, sonra yalnızca içerik bölgesi kırpılır (358 ve 008b yöntemi; yeniden boyama yok). `create_image_pixen` 16 tuval; 10 ve 12 px parçalar ortadan kırpılır. İçerik taşarsa kırpılmaz, raporda yazılır.
- **Ayarlar:** arayüz paleti `art/style/palette_arayuz.json`, `max_colors=32` (araç izin verirse), `seed=0`, saydam zemin, renkli koyu dış kontur (saf siyah yok), ışık sol üstten, yazı, harf ve rakam yok. `create_ui_asset` kullanılmaz (en küçük tuval 192; 744-746 dersi).
- **Meşe metin eki:** `carved dark oak UI piece, leaf and vine carving on the rim (not geometric), muted moss green leaves, small wrought iron rivets at the corners, inner cream card for blank text, dark warm brown outline, pixel art game HUD, transparent background, no text, no letters, no numbers`
- **Denetim:** her parça `python tools/art/style.py check --family arayuz <dosya>` GEÇMELİ; geçmeyen parça için **en fazla bir** yeniden deneme. Yine geçmezse teslim edilmez, raporda yazılır.
- **Bütçe:** 810 + 811 + 812 toplamı ≤ 40 gen (kullanıcı onayı). Aşılacaksa dur, `## Sorular` altına yaz.
- Kompozisyon bu işlerde yapılmaz: kod oturumu `ui_mockup.py hud hud_v2 --yon mese2` ile `art/results/420_hud_pilot.png` çıkarır.

## Araç
`create_image_pixen`, tuval 16x16, her ikon ayrı çağrı (1 gen). 745 sayfa 3'te (`ui_hud2_mese_sheet3.png`) tanınan ikon varsa kırpılıp `init_image` olarak verilir (ekmek, hilal, alev, topaç, su damlası, filiz, kar tanesi). İkon eki: `single flat pixel art UI icon, simple readable silhouette, flat colors, 1px dark warm outline, transparent background, no text, no letters, no numbers`

## İkonlar
| Dosya eki | Native | Tarif |
|---|---|---|
| `need_hunger` | 16x16 | Somun ekmek |
| `need_energy` | 16x16 | Hilal ay |
| `need_warmth` | 16x16 | Alev |
| `need_fun` | 16x16 | Ahşap topaç |
| `need_hygiene` | 16x16 | Su damlası ve küçük sabun kalıbı |
| `need_social` | 16x16 | Yan yana iki baş silüeti |
| `need_toilet` | 16x16 | Kapaklı ahşap oturak (kova) |
| `speed_pause` | 12x12 | İki dikey çubuk |
| `speed_1`, `speed_2`, `speed_3` | 12x12 | Bir, iki, üç sağa üçgen; açık ahşap üstünde koyu kahve sembol |
| `season_spring` | 12x12 | Yeşil filiz (iki yaprak) |
| `season_summer` | 12x12 | Sarı güneş |
| `season_autumn` | 12x12 | Turuncu yaprak |
| `season_winter` | 12x12 | Açık mavi kar tanesi |

Hız ve mevsim ikonları 16 tuvalde ortadaki 12x12'ye sığacak şekilde tarif edilir ve oradan kırpılır.

## Teslim
`art/results/812-ikonlar-x3.png`: hepsi 1x, 2x, 3x, adlarıyla. Her biri tek bakışta tanınmalı; tanınmayan ikon için tek yeniden deneme hakkı kullanılabilir.

## Sonuç

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
