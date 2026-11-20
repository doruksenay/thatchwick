---
id: "810"
title: "HUD meşe, dünya ölçeği: paneller ve sarmaşıklı kart"
status: running
approved_by: user        # kullanıcı kararı 2026-11-21: saf meşe; HUD dünya ile aynı piksel boyutu (2x); üç iş toplam üst sınır 40
priority: 1
family: arayuz
style_gate: false
cost_est: 7
cost_max: 15
depends_on: []
max_attempts: 2
pixellab: true
yon: mese2
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_mese2_{clock,money,tracker,frame,card,bar,bar_fill_green,bar_fill_amber,bar_fill_red}.png
created: 2026-11-21
---
# 810 HUD meşe (dünya ölçeği): paneller

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

## Parçalar (`create_image_pixflux`, her biri ayrı çağrı)
| Parça | Native (tuval) | Kılavuz (774) | Not |
|---|---|---|---|
| `clock` | 96x40 | `ui_hud2_mese_clock` | Asılı oyma levha, üstte iki kısa demir zincir. Solda 12x12 mevsim yuvası (x 6-18, y 14-26), sağında iki satır boş krem yazı alanı (x 24-92, y 6-34). 774'teki büyük yuvarlak delik yalnızca soldaki küçük yuva olarak kalır |
| `money` | 72x20 (72x32) | `ui_hud2_mese_money` | Tek satır: iki koyu ahşap yuva yan yana; her yuvanın solunda 10x10 ikon yeri, sağında boş sayı alanı |
| `tracker` | 48x18 (48x32), 9-slice köşe 6 | `ui_hud2_mese_tracker` | Krem şerit, uçlarda küçük yaprak oyma; kenar ve orta düz tekrarlanabilir |
| `frame` | 24x24 (32x32), 9-slice köşe 6 | `ui_hud2_mese_frame` | Köşede perçin, ortası düz krem; kenarda yırtık kâğıt çıkıntısı **yok** (774'te gerilince çoğaldı) |
| `card` | 132x112, sabit boy | `ui_hud2_mese_window` | **Kullanıcının beğendiği sarmaşıklı çerçeve**: koyu meşe, soluk mavi-yeşil sarmaşık ve yapraklar, köşe perçinleri, krem iç. İç krem alan en az x 10-122, y 10-102 ve düz (ihtiyaç satırları buraya yazılır) |
| `bar` | 40x8 (40x32), 9-slice köşe 3 | `ui_hud2_mese_bar` | Koyu iç oluk (y 2-5), 1 px ahşap kılıf, uçta demir başlık |
| `bar_fill_green/amber/red` | 1x6 | 774 `bar_fill_*` renkleri | PixelLab yok: Python ile paletten düz renk, üst 1 px açık ton; 0 gen |

## Teslim
`art/results/810-yanyana.png` (3x, adlarıyla, 774 kılavuzu yanında); `art/results/810-9slice-test.png` (`tracker`, `frame`, `bar` üç boyda).

## Sonuç

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
