---
id: "213c"
title: Arayüz ekranları: Etkileşim menüsü, gün sonu ve ayarlar ekranları
status: pending
approved_by: user        # otomatik akış: 213 plan işi yazdı
family: arayuz
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # create_ui_asset stil görselini yalnızca base64 metni olarak alıyor (211 bu yüzden bulutta takıldı)
outputs:
  - Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png
  - art/results/213c-etkilesim-onizleme-x2.png
  - art/results/213c-gun_sonu-onizleme-x2.png
  - art/results/213c-ayarlar-onizleme-x2.png
created: 2026-11-18
---
# 213c Etkileşim menüsü, gün sonu ve ayarlar ekranları

## Amaç
210'da seçilen yönle oyunun karakter dışı ekranlarından 3 tanesinin boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` içinde (213 yazdı): `etkilesim`, `gun_sonu`, `ayarlar`.

## Tarif
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı. Satır yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Araç ve ortak ayarlar 211 ile aynı:** `create_ui_asset`, `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`. Önce araç açıklamasını oku; 416x192 desteklenmiyorsa ya da bir çağrı 20 gen'i geçiyorsa çağırma, `## Sorular` ekle, `blocked` yap.
- `style_image_base64` = 210'da kabul edilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`). Araç en büyük ölçüyü sınırlıyorsa en yakın komşuyla yarıya indir, raporda yaz.
- `color_palette` = `"earthy muted medieval browns, cream parchment, dark iron grey, muted moss green accent"`.
- `pieces`: her ekran için kendi komutunun çıktısı (aşağıdaki tablo), koordinatlar 2 kat (832x384 sanal tuval). `elements` verme.
- Her açıklamanın sonuna şu ek: `, pixel art game UI screen panel for a cozy medieval village life simulation, earthy muted medieval palette, flat simple shading, clean dark outline, light from top-left, crisp readable shapes, no dithering, no noise, no text, no letters, no numbers, no icons, no faces, all plaques, tabs, cards, slots and bars empty and blank`
- Ekranlar birbirine bağlı değil: çağrıları paralel gönder.

### Çağrılar
| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_etkilesim_<yon> | 416x192 | `ui_mockup.py pieces etkilesim --scale 2` | `item interaction popup screen in the same material as the reference panel: no header strip; a blank inset card with a small empty square slot at the top left, a tall popup menu panel in the middle holding six blank list rows and a thin empty scrollbar track on its right edge; on the right a blank inset card and a blank wide button below it` | ≤20 |
| ui_panel_gun_sonu_<yon> | 416x192 | `ui_mockup.py pieces gun_sonu --scale 2` | `end of day summary screen in the same material as the reference panel: header strip with a blank title plaque and a small blank close button; a row of four blank inset cards under the header; below on the left a large blank inset card with a small empty square icon slot and an empty wide thin gauge frame; on the right a small blank inset card above a blank wide button` | ≤20 |
| ui_panel_ayarlar_<yon> | 416x192 | `ui_mockup.py pieces ayarlar --scale 2` | `pause and settings screen in the same material as the reference panel: header strip with a blank title plaque and a small blank close button; a column of five blank wide buttons on the left, a large blank inset card on the right holding two empty thin slider tracks, three blank segmented tabs in a row where the middle one is lighter and joined to the card, and a thin blank text strip at the bottom` | ≤20 |

### Önizleme (PixelLab yok)
```
uv run --with pillow python tools/art/ui_mockup.py mockup etkilesim --panel Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png --out art/results/213c-etkilesim-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup gun_sonu --panel Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png --out art/results/213c-gun_sonu-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayarlar --panel Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png --out art/results/213c-ayarlar-onizleme-x2.png
```
Eksik ikon ve portre listesini rapora yaz.

### Teslim
`Assets/Art/Iso/_aday/` altına `cp -n`. Masaüstü kopyası 210 ile aynı kurallar (`...\art\arayuz\<yyyy-mm-dd>_213c_<yon>\`); bulutta "masaüstü kopyası: yerel".

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Her çıktı 210 paneliyle aynı malzemeden mi? `art/results/213c-yanyana-x2.png`: 210 paneli + bu işin çıktıları.
- Önizlemede metin ve yer tutucular kutulara sığıyor mu?

## Notlar
- Yazı, ikon, portre, çubuk dolgusu koddan gelir; panelde boş kalır.
- İkonlar 212, 315 ve 391-394 işlerinden; bağımsızdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
