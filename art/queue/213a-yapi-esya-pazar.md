---
id: "213a"
title: Arayüz ekranları yapi, esya_karti, pazar
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (213 plan işi)
family: arayuz
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # stil görseli base64 olarak araca doğrudan verilmeli (211'deki gibi, kullanıcı kararı 2026-11-19)
outputs:
  - Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png
  - art/results/213a-yapi-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png
  - art/results/213a-esya_karti-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png
  - art/results/213a-pazar-onizleme-x2.png
created: 2026-11-20
---
# 213a Arayüz ekranları: yapi, esya_karti, pazar

## Amaç
210'da seçilen yönle 3 ekranın boş panelini üretir. Yerleşimler `tools/art/ui_layouts.json` içindeki aynı adlı anahtarlardır (213 yazdı).

## Tarif
- **Ön adım:** `tools/art/ui_layouts.json` içinde `yapi` anahtarı yoksa 213'ün `art/results/213-ui_layouts-ek.json` dosyasındaki anahtarları bu dosyaya birleştir (yerel işçi `tools/art` yazabilir; 213 bulutta yazamadı).
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Ortak ayarlar 211 ile aynı:** `create_ui_asset`, `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`, `color_palette` 210 ile aynı. `style_image_base64` = 210'da seçilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`), dosyadan base64'e çevrilir; büyük sınır varsa en yakın komşuyla yarıya indir ve yaz. `pieces`: `ui_mockup.py pieces <ekran> --scale 2` çıktısı; araç sanal tuvali 512'ye sınırlıyorsa orantılı ölçekle (`rect` -> `rounded_rect` radius 0), raporda yaz.
- Her açıklamanın sonuna ek: `, pixel art game UI screen panel for a cozy medieval village life simulation, earthy muted medieval palette, flat simple shading, clean dark outline, light from top-left, crisp readable shapes, no dithering, no noise, no text, no letters, no numbers, no icons, no faces, all plaques, tabs, cards, slots and bars empty and blank`
- Önce araç açıklamasını oku; tek çağrı 20 gen'i geçecekse çağırma, `blocked` yap.

### Çağrılar (ekran başına bir, bağımsız: paralel gönderilebilir)
| Çıktı | Ölçü | `pieces` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_yapi_<yon> | 416x192 | `ui_mockup.py pieces yapi --scale 2` | `build mode screen in the same material as the reference panel, header strip with a blank title plaque and a small blank close button; a row of six blank category tabs where the first is lighter and joined to the body, a row of five blank square tool buttons, a grid of six blank catalog cards in two rows of three; on the right a blank floor selector card and a tall blank house value card` | ≤20 |
| ui_panel_esya_karti_<yon> | 416x192 | `ui_mockup.py pieces esya_karti --scale 2` | `item info card in the same material as the reference panel, header strip with a blank title plaque and a small blank close button; a square empty item icon frame on the left, a wide blank description card beside it, a row of three blank plaques (price, set, comfort) and a blank wide action button at the bottom right` | ≤20 |
| ui_panel_pazar_<yon> | 416x192 | `ui_mockup.py pieces pazar --scale 2` | `market trade screen in the same material as the reference panel, header strip with a blank title plaque and a small blank close button; a large blank list area on the left holding five blank rows each with an empty small icon slot; on the right a blank offer card, a blank coin plaque, two blank buttons side by side and a thin blank tab strip` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup yapi --panel Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png --out art/results/213a-yapi-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup esya_karti --panel Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png --out art/results/213a-esya_karti-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup pazar --panel Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png --out art/results/213a-pazar-onizleme-x2.png
```
Yerleşim önizlemeleri (PixelLab'siz): `art/results/213-<ekran>-yerlesim-x2.png`.

## Doğrulama
210'daki ölçütler (ölçü, renk sayısı, saydamlık, şablona uyum, görsel kontrol) + üç çıktı 210 paneliyle yan yana (`art/results/213a-yanyana-x2.png`).

## Notlar
Yazı, ikon, portre ve değerler koddan gelir; panel boş kalır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
