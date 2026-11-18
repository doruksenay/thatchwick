---
id: "213b"
title: Arayüz ekranları: Köy, aile ağacı ve depo ekranları
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
  - Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png
  - art/results/213b-koy-onizleme-x2.png
  - art/results/213b-aile-onizleme-x2.png
  - art/results/213b-depo-onizleme-x2.png
created: 2026-11-18
---
# 213b Köy, aile ağacı ve depo ekranları

## Amaç
210'da seçilen yönle oyunun karakter dışı ekranlarından 3 tanesinin boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` içinde (213 yazdı): `koy`, `aile`, `depo`.

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
| ui_panel_koy_<yon> | 416x192 | `ui_mockup.py pieces koy --scale 2` | `village panel in the same material as the reference panel: header strip with a blank title plaque and four blank tabs where the first is lighter and joined to the body, a small blank close button; four blank list rows on the left each with an empty small portrait frame, an empty thin gauge frame and a small blank button; on the right one blank inset card above two blank list rows` | ≤20 |
| ui_panel_aile_<yon> | 416x192 | `ui_mockup.py pieces aile --scale 2` | `family tree screen in the same material as the reference panel: header strip with a blank title plaque, two blank tabs where the first is lighter and joined to the body, a small blank close button; a large blank inset area on the left holding seven empty small portrait frames arranged in three generation rows (two, three, two); on the right a tall blank inset card and a small blank plaque below it` | ≤20 |
| ui_panel_depo_<yon> | 416x192 | `ui_mockup.py pieces depo --scale 2` | `storage and inventory screen in the same material as the reference panel: header strip with a blank title plaque, two blank tabs where the first is lighter and joined to the body, a small blank close button; on the left a blank inset area holding a grid of eighteen empty square slots in three rows of six, a blank wide button under it; on the right a blank inset card with an empty thin gauge frame and a larger blank inset detail card below` | ≤20 |

### Önizleme (PixelLab yok)
```
uv run --with pillow python tools/art/ui_mockup.py mockup koy --panel Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png --out art/results/213b-koy-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup aile --panel Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png --out art/results/213b-aile-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup depo --panel Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png --out art/results/213b-depo-onizleme-x2.png
```
Eksik ikon ve portre listesini rapora yaz.

### Teslim
`Assets/Art/Iso/_aday/` altına `cp -n`. Masaüstü kopyası 210 ile aynı kurallar (`...\art\arayuz\<yyyy-mm-dd>_213b_<yon>\`); bulutta "masaüstü kopyası: yerel".

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Her çıktı 210 paneliyle aynı malzemeden mi? `art/results/213b-yanyana-x2.png`: 210 paneli + bu işin çıktıları.
- Önizlemede metin ve yer tutucular kutulara sığıyor mu?

## Notlar
- Yazı, ikon, portre, çubuk dolgusu koddan gelir; panelde boş kalır.
- İkonlar 212, 315 ve 391-394 işlerinden; bağımsızdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
