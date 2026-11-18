---
id: "213a"
title: Arayüz ekranları: Yapı, eşya kartı ve pazar ekranları
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
  - Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png
  - art/results/213a-yapi-onizleme-x2.png
  - art/results/213a-esya_karti-onizleme-x2.png
  - art/results/213a-pazar-onizleme-x2.png
created: 2026-11-18
---
# 213a Yapı, eşya kartı ve pazar ekranları

## Amaç
210'da seçilen yönle oyunun karakter dışı ekranlarından 3 tanesinin boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` içinde (213 yazdı): `yapi`, `esya_karti`, `pazar`.

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
| ui_panel_yapi_<yon> | 416x192 | `ui_mockup.py pieces yapi --scale 2` | `build mode screen in the same material as the reference panel: header strip with a blank title plaque, five blank tabs where the first is lighter and joined to the body, a small blank close button; a column of five small blank square tool buttons on the left, a grid of six blank catalog cards in two rows of three in the middle, on the right two small blank buttons, a blank plaque and a tall blank inset card, and a thin blank text strip along the bottom` | ≤20 |
| ui_panel_esya_karti_<yon> | 416x192 | `ui_mockup.py pieces esya_karti --scale 2` | `item info card in the same material as the reference panel: header strip with a wide blank title plaque and a small blank close button; a large blank square icon frame on the left with a small blank price plaque and a blank wide button under it, three blank list rows in the middle, on the right a blank inset card holding four empty small square slots in a row, and a wide blank inset description card along the bottom` | ≤20 |
| ui_panel_pazar_<yon> | 416x192 | `ui_mockup.py pieces pazar --scale 2` | `market trade screen in the same material as the reference panel: header strip with a blank title plaque, two small blank plaques and a small blank close button; five blank list rows each with a small blank button at the right end on the left side, and a right column with an active tab joined to the body above an inactive tab, a blank inset card and a small blank plaque` | ≤20 |

### Önizleme (PixelLab yok)
```
uv run --with pillow python tools/art/ui_mockup.py mockup yapi --panel Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png --out art/results/213a-yapi-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup esya_karti --panel Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png --out art/results/213a-esya_karti-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup pazar --panel Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png --out art/results/213a-pazar-onizleme-x2.png
```
Eksik ikon ve portre listesini rapora yaz.

### Teslim
`Assets/Art/Iso/_aday/` altına `cp -n`. Masaüstü kopyası 210 ile aynı kurallar (`...\art\arayuz\<yyyy-mm-dd>_213a_<yon>\`); bulutta "masaüstü kopyası: yerel".

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Her çıktı 210 paneliyle aynı malzemeden mi? `art/results/213a-yanyana-x2.png`: 210 paneli + bu işin çıktıları.
- Önizlemede metin ve yer tutucular kutulara sığıyor mu?

## Notlar
- Yazı, ikon, portre, çubuk dolgusu koddan gelir; panelde boş kalır.
- İkonlar 212, 315 ve 391-394 işlerinden; bağımsızdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
