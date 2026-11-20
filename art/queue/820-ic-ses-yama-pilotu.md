---
id: "820"
title: "İç Sesler nakış yaması pilotu: boş yama, üç dikiş, kurdele, dört işlemeli ikon"
status: running
claimed_at: 2026-11-20T23:54Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 21.11.2026 sohbet (yama dili A "nakış yaması", hâl katmanı A), Project belgesi 68a
priority: 1           # 21.11.2026: kullanıcı sonucu hemen görmek istiyor
family: arayuz
style_gate: true
cost_est: 10
cost_max: 16
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_patch_base.png
  - Assets/Art/Iso/_aday/ui_patch_stitch_{single,double,gold}.png
  - Assets/Art/Iso/_aday/ui_patch_ribbon.png
  - Assets/Art/Iso/_aday/ui_icon_voice_{good_meal,nice_chat,grief,inspired}.png
created: 2026-11-21
---
# 820 İç Sesler nakış yaması pilotu

## Amaç
Alttaki karakter kartında her iç ses bir **nakış yaması** olarak görünecek (Project belgesi 68a, kullanıcı kararı 21.11.2026). Yamanın kumaş rengi iç sesin **hâlini** gösterir (Huzurlu, Neşeli, Şevkli, Gururlu, Gergin, Kederli, Hırçın), kenar dikişi gücünü (tek, çift, sırma), üstten solma kalan süreyi. Kod yamayı hâl rengine **palet değişimiyle** boyar; bu yüzden PixelLab yamayı tek, nötr keten renginde çizer.

Bu iş stil pilotudur (`style_gate: true`): kullanıcı beğenirse bütün iç ses ikonları (~280) bu dille yeniden üretilir. Elle çizilmiş taslak: sohbetteki `yama_dili.png` ve `hal_karti_yakin.png` (Claude'un PIL taslağı; görsel referans değil, yalnızca yerleşim).

## Tarif
Her parça **ayrı çağrı**, `create_image_pixflux` (en küçük kenar 32). 24'lük parça 32x32 tuvalde ortalanır, yalnızca içerik kutusu kırpılır (753'teki yöntem; ölçek düşürülmez, içerik yeniden boyanmaz). Ayarlar: arayüz paleti `art/style/palette_arayuz.json`, saydam zemin, flat shading, koyu ılık dış kontur, ışık sol üstten, saf siyah yok.

Ortak ek: `, embroidered cloth patch, visible woven linen texture, cozy medieval craft, pixel art game UI, transparent background, no text, no letters`

| Dosya | Native | Açıklama |
|---|---|---|
| ui_patch_base | 24x24 | `a small square patch of undyed linen cloth with slightly rounded cut corners, subtle woven texture, soft frayed edge, dark warm brown outline, empty center` |
| ui_patch_stitch_single | 24x24 (saydam, yalnızca dikiş) | `only a single row of short running stitches in cream thread forming a square border 2 pixels inside the edge, everything else transparent` |
| ui_patch_stitch_double | 24x24 | `only two parallel rows of short running stitches in cream thread forming a square border, everything else transparent` |
| ui_patch_stitch_gold | 24x24 | `only a row of gold thread running stitches forming a square border with a small gold knot at each corner, everything else transparent` |
| ui_patch_ribbon | 64x16 (tuval 64x32) | `a horizontal embroidered cloth ribbon banner in undyed linen with forked swallowtail ends and a stitched border along top and bottom, empty middle for text` |
| ui_icon_voice_good_meal | 16x16 (tuval 32) | `a steaming clay bowl of stew, embroidered in colored thread` |
| ui_icon_voice_nice_chat | 16x16 | `two small speech bubbles overlapping, embroidered in colored thread` |
| ui_icon_voice_grief | 16x16 | `a single wilted flower with a drooping head, embroidered in colored thread` |
| ui_icon_voice_inspired | 16x16 | `a small lit candle with a bright flame and sparkle, embroidered in colored thread` |

İkonlar "işlenmiş" görünmeli: çizgiler iplik gibi kısa, paralel dikiş izleri; düz doldurulmuş leke değil. Renk sayısı ikon başına en çok 8.

## Doğrulama
- `python tools/art/style.py check --family arayuz <dosya> ...`; `inspired` ışık içerir: `--light`.
- Yama tabanı palet değişimine uygun: keten tonları en çok 4 ton, kontur ayrı renk (kod konturu koyulaştırır, ketenleri hâl rengine eşler).
- Önizleme `art/results/820-yama-pilotu-x4.png`: taban + üç dikiş üst üste, yedi hâl rengine boyanmış hâlleri (rapordaki betikle) ve dört ikon yamanın üstünde; krem (#F3EAD6) ve meşe çerçeve (ui_hud2_mese_window) zemininde.
- Göz kontrolü 3 kat ölçekte: 16x16 ikon yamanın üstünde okunuyor mu; dikiş üç gücü ayrı ayrı belli mi.

## Notlar
- Hâl renkleri (palet değişimi hedefi, belge 68a): Huzurlu #7E9B66, Neşeli #D6A542, Şevkli #4E9A8F, Gururlu #8A5A8C, Gergin #B8803A, Kederli #6C8299, Hırçın #A84A3E.
- Pilot kabul edilirse sonraki iş: bütün iç ses ikonları bu dille, 16x16 işlemeli (~280, 60'lık parçalara bölünerek).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
