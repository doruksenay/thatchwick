---
id: "811"
title: "HUD meşe, dünya ölçeği: Yapı ve Defter düğmeleri, rozet"
status: review
claimed_at: 2026-11-20T23:08Z
approved_by: user        # kullanıcı kararı 2026-11-21: saf meşe; HUD dünya ile aynı piksel boyutu (2x); üç iş toplam üst sınır 40
priority: 1
family: arayuz
style_gate: false
cost_est: 5
cost_max: 10
depends_on: []
max_attempts: 2
pixellab: true
yon: mese2
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_mese2_{btn_build,btn_build_down,btn_book,btn_book_down,badge}.png
created: 2026-11-21
---
# 811 HUD meşe (dünya ölçeği): düğmeler ve rozet

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

## Parçalar
| Parça | Native | Araç | Tarif |
|---|---|---|---|
| `btn_build` | 32x32 | pixflux 32x32 | Oyma meşe yuvarlak kalkan düğme, kenarda yaprak oyması ve demir perçin; ortada krem daire üstünde çapraz çekiç ve mala |
| `btn_build_down` | 32x32 | pixflux, `btn_build` çıktısı `init_image` (güç ≤ 300) | Basılı hâl: iç biraz koyu, gölge az, ikon 1 px aşağı. Tutmazsa Python: iç daireyi paletten bir basamak koyulaştır, ikonu 1 px aşağı kaydır (0 gen; raporda yaz) |
| `btn_book` | 32x32 | pixflux 32x32 | Aynı kalkan; ortada kapalı deri kitap, bordo kapak, metal köşe |
| `btn_book_down` | 32x32 | `btn_build_down` gibi | |
| `badge` | 10x10 | pixen 16, ortadan kırp | Demir halkalı küçük yuvarlak rozet, bordo iç, krem ünlem |

Ölçüt: çekiç+mala ve kitap 1x'te tek bakışta tanınmalı; normal ve basılı hâlin farkı görülmeli. Stardew'in düz ahşap düğmelerine benzememeli (oyma yaprak kenar ve perçin şart).

## Teslim
`art/results/811-yanyana.png`: dört düğme ve rozet 1x, 2x, 3x; rozet `btn_book` düğmesinin sağ üstüne bindirilmiş hâliyle de.

## Sonuç

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
