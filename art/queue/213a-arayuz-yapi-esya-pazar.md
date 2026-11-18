---
id: "213a"
title: Arayüz ekranları (yapi, esya_karti, pazar)
status: pending
approved_by: user        # otomatik akış (213 plan işi yazdı)
family: arayuz
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # create_ui_asset stil görseli base64 ister; bulutta aktarılamıyor (211 Sorular)
priority: 50
outputs:
  - Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png
  - art/results/213a-yapi-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png
  - art/results/213a-esya_karti-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png
  - art/results/213a-pazar-onizleme-x2.png
created: 2026-11-18
---
# 213a Arayüz ekranları: yapi, esya_karti, pazar

## Amaç
210'da seçilen yönle (`yon:` satırı yoksa varsayılan `parsomen`) 3 ekranın boş paneli. Yerleşim `tools/art/ui_layouts.json` içinde: yapi, esya_karti, pazar. Yazı, ikon ve portre koddan gelir.

## Tarif
211 ile aynı ayarlar: `create_ui_asset`, `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`, `color_palette` 210 ile aynı. `style_image_base64` = 210'da kabul edilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`); araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz. Her açıklamanın sonuna 210'daki ortak eki ekle. Önce araç açıklamasındaki maliyeti oku; 20 gen'i aşıyorsa çağırma, `blocked` yap.

Çağrılar (ekran başına bir çağrı, bağımsız oldukları için paralel):
| Çıktı | Ölçü | `pieces` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_yapi_<yon> | 416x192 | `ui_mockup.py pieces yapi --scale 2` | `build mode screen in the same material as the reference panel: header strip with a blank title plaque, five blank square tool buttons in a row and a close button; a row of five blank category tabs; a grid of six blank catalog cards in two rows of three; on the right a blank floor plaque, two small blank buttons and a tall blank inset card` | <= 20 |
| ui_panel_esya_karti_<yon> | 416x192 | `ui_mockup.py pieces esya_karti --scale 2` | `item info card in the same material as the reference panel: header strip with a wide blank title plaque and a close button; a large empty square icon frame on the left with two small blank plaques under it; a wide blank description card on the right and three blank stat bars under it, a blank wide button at the bottom right` | <= 20 |
| ui_panel_pazar_<yon> | 416x192 | `ui_mockup.py pieces pazar --scale 2` | `market screen in the same material as the reference panel: header strip with a blank title plaque, two blank tabs (the first lighter and joined to the body), a blank coin purse plaque and a close button; a list of six blank row cards with a thin empty scrollbar on the left; a blank inset detail card and two blank wide buttons on the right` | <= 20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup yapi --panel Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png --out art/results/213a-yapi-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup esya_karti --panel Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png --out art/results/213a-esya_karti-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup pazar --panel Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png --out art/results/213a-pazar-onizleme-x2.png
```

## Doğrulama
210'daki ölçütler (ölçü, renk sayısı, saydamlık, şablona uyum, görsel kontrol). Ek: çıktılar 210 paneliyle yan yana, aynı malzemeden mi (`art/results/213a-yanyana-x2.png`).

## Notlar
Yerleşim önizlemeleri (boş kutular): `art/results/213-yapi-yerlesim-x2.png`, `art/results/213-esya_karti-yerlesim-x2.png`, `art/results/213-pazar-yerlesim-x2.png`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
