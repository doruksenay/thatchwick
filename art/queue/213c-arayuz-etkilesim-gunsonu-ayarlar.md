---
id: "213c"
title: Arayüz ekranları (etkilesim, gun_sonu, ayarlar)
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
  - Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png
  - art/results/213c-etkilesim-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png
  - art/results/213c-gun_sonu-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png
  - art/results/213c-ayarlar-onizleme-x2.png
created: 2026-11-18
---
# 213c Arayüz ekranları: etkilesim, gun_sonu, ayarlar

## Amaç
210'da seçilen yönle (`yon:` satırı yoksa varsayılan `parsomen`) 3 ekranın boş paneli. Yerleşim `tools/art/ui_layouts.json` içinde: etkilesim, gun_sonu, ayarlar. Yazı, ikon ve portre koddan gelir.

## Tarif
211 ile aynı ayarlar: `create_ui_asset`, `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`, `color_palette` 210 ile aynı. `style_image_base64` = 210'da kabul edilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`); araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz. Her açıklamanın sonuna 210'daki ortak eki ekle. Önce araç açıklamasındaki maliyeti oku; 20 gen'i aşıyorsa çağırma, `blocked` yap.

Çağrılar (ekran başına bir çağrı, bağımsız oldukları için paralel):
| Çıktı | Ölçü | `pieces` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_etkilesim_<yon> | 416x192 | `ui_mockup.py pieces etkilesim --scale 2` | `small interaction menu popup in the same material as the reference panel: a rounded frame with a blank title plaque at the top, six blank row buttons in a column and a thin empty scrollbar track on the right; transparent outside the frame` | <= 20 |
| ui_panel_gun_sonu_<yon> | 416x192 | `ui_mockup.py pieces gun_sonu --scale 2` | `end of day summary in the same material as the reference panel: header strip with a wide blank title plaque and a close button; a blank inset summary card with four blank row bars; on the right a blank winter gauge card with an empty horizontal gauge frame and a blank wide button below; a blank inset note card at the bottom left` | <= 20 |
| ui_panel_ayarlar_<yon> | 416x192 | `ui_mockup.py pieces ayarlar --scale 2` | `settings screen in the same material as the reference panel: header strip with a blank title plaque and a close button; five blank full-width settings row bars, two empty slider tracks in the first two rows, and three blank pill option buttons at the bottom right` | <= 20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup etkilesim --panel Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png --out art/results/213c-etkilesim-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup gun_sonu --panel Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png --out art/results/213c-gun_sonu-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayarlar --panel Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png --out art/results/213c-ayarlar-onizleme-x2.png
```

## Doğrulama
210'daki ölçütler (ölçü, renk sayısı, saydamlık, şablona uyum, görsel kontrol). Ek: çıktılar 210 paneliyle yan yana, aynı malzemeden mi (`art/results/213c-yanyana-x2.png`).

## Notlar
Yerleşim önizlemeleri (boş kutular): `art/results/213-etkilesim-yerlesim-x2.png`, `art/results/213-gun_sonu-yerlesim-x2.png`, `art/results/213-ayarlar-yerlesim-x2.png`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
