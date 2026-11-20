---
id: "753"
title: "HUD karma yön: meşe paneller ve sarmaşıklı kart çerçevesi (parça başına)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21: meşe + ilmek karışık; meşedeki sarmaşıklı çerçeve beğenildi; parça başına üretim
priority: 1
family: arayuz
style_gate: false
cost_est: 10
cost_max: 20
depends_on: []
max_attempts: 2
pixellab: true
yon: karma
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_karma_{clock,money,tracker,frame,card,bar,bar_fill_green,bar_fill_amber,bar_fill_red}.png
created: 2026-11-21
---
# 753 HUD karma yön: meşe paneller (parça başına)

## Amaç ve karar
744-746 pilotunda sayfa yöntemi tutmadı: `create_ui_asset` en küçük 192 px tuval istiyor, parçaları tek panelde ve büyük çiziyor (sonuçlar `art/results/744-746`). Kullanıcı kararı (2026-11-21): **karma yön**.
- Paneller ve çerçeveler: **meşe** (745) dili; özellikle `ui_hud2_mese_sheet2.png`'deki sarmaşıklı büyük çerçeve beğenildi.
- Düğmeler ve rozet: **ilmek** (744) çıktıları olduğu gibi kullanılır (`ui_hud2_ilmek_btn_*`, `ui_hud2_ilmek_badge`); bu iş onları üretmez.
- İkonlar: 754.

Bu iş her parçayı **ayrı çağrıyla, native boyutta** üretir. `create_ui_asset` kullanılmaz.

## Araç
- `create_image_pixflux` (1 gen/çağrı; en küçük kenar 32). Native kenarı 32'den küçük parçada tuval 32'ye büyütülür, içerik native kutuya göre `init_image` ile ortalanır, sonra yalnızca içerik bölgesi kırpılır (358 ve 008b'deki yöntem; içerik yeniden boyanmaz, ölçek düşürülmez).
- **Stil yönlendirmesi:** `ui_hud2_mese_sheet1.png` ve `ui_hud2_mese_sheet2.png`'den ilgili parça kırpılır, `art/tmp/` altında en yakın komşuyla native boyuta indirilir ve **yalnızca `init_image` olarak** verilir (güç 300-450). Bu indirgenmiş kılavuz teslim edilmez; teslim edilen her zaman PixelLab çıktısıdır.
- Ayarlar: arayüz paleti `art/style/palette_arayuz.json` (`color_palette`), `max_colors=32`, `seed=0`, saydam zemin, flat/basic shading, renkli koyu dış kontur, ışık sol üstten, saf siyah yok.
- **Açıklama eki (her çağrıya):** `carved dark oak UI piece, leaf and vine carving on the rim (not geometric), small wrought iron rivets at the corners, inner cream card for blank text, dark warm outline, pixel art game HUD, transparent background, no text, no letters, no numbers`

## Parçalar
Ad biçimi: `Assets/Art/Iso/_aday/ui_hud2_karma_<parça>.png`

| Parça | Native | Kılavuz (mese sayfasından) | Not |
|---|---|---|---|
| `clock` | 96×40 | sheet1 zincirli levha | Üstte iki kısa demir zincir; solda 12×12 mevsim yuvası (y≈14), sağda iki satır boş krem yazı alanı (x 26-92) |
| `money` | 72×20 (tuval 72×32) | sheet1 krem şerit | Krem şerit, iki uçta küçük yaprak oyma; sol (x 4-14) ve orta (x 38-48) 10×10 ikon yuvası boş |
| `tracker` | 48×18, 9-slice köşe 6 (tuval 48×32) | sheet1 krem şerit | Kenar ve orta düz tekrarlanabilir; desen yalnızca köşelerde |
| `frame` | 24×24, 9-slice köşe 6 (tuval 32×32) | sheet1 küçük çerçeve | Hız düğmeleri ve portre kenarı; köşede perçin, ortası düz krem |
| `card` | 132×112, **sabit boy (9-slice değil)** | **sheet2 sarmaşıklı çerçeve** | Karakter kartı. Kullanıcının beğendiği çerçeve: koyu meşe, mavi-yeşil sarmaşık ve yapraklar, köşe perçinleri, krem iç. İç krem alan en az x 8-124, y 8-104 (ihtiyaç satırları buraya yazılır), iç alan düz |
| `bar` | 40×8, 9-slice köşe 3 (tuval 40×32) | sheet1 çubuk kılıfı | İç oluk koyu, 1 px kontur |
| `bar_fill_green/amber/red` | 1×6 | yok | PixelLab çağrısı yok: Python ile paletten düz renk (yeşil, hardal, bordo-kırmızı); 0 gen |

## Denetim ve teslim
- Her parça `python tools/art/style.py check --family arayuz <dosya>` GEÇMELİ. Geçmeyen parça için **en fazla bir** yeniden deneme; yine geçmezse teslim edilmez, raporda yazılır.
- `art/results/753-yanyana.png`: bütün parçalar 3×, adlarıyla. `art/results/753-9slice-test.png`: `tracker`, `frame`, `bar` üç farklı boyda (ör. 48×18, 120×18, 200×40).
- Bütçe: `cost_max: 20`. Karma pilotun toplamı (753 + 754) ≤ 50; aşılacaksa dur, `## Sorular`'a yaz.

## Sonuç

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
