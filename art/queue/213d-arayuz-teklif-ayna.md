---
id: "213d"
title: Arayüz ekranları (teklif, ayna)
status: pending
approved_by: user        # otomatik akış (213 plan işi yazdı)
family: arayuz
style_gate: false
cost_est: 30
cost_max: 45
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # create_ui_asset stil görseli base64 ister; bulutta aktarılamıyor (211 Sorular)
priority: 50
outputs:
  - Assets/Art/Iso/_aday/ui_panel_teklif_<yon>.png
  - art/results/213d-teklif-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_ayna_<yon>.png
  - art/results/213d-ayna-onizleme-x2.png
created: 2026-11-18
---
# 213d Arayüz ekranları: teklif, ayna

## Amaç
210'da seçilen yönle (`yon:` satırı yoksa varsayılan `parsomen`) 2 ekranın boş paneli. Yerleşim `tools/art/ui_layouts.json` içinde: teklif, ayna. Yazı, ikon ve portre koddan gelir.

## Tarif
211 ile aynı ayarlar: `create_ui_asset`, `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`, `color_palette` 210 ile aynı. `style_image_base64` = 210'da kabul edilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`); araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz. Her açıklamanın sonuna 210'daki ortak eki ekle. Önce araç açıklamasındaki maliyeti oku; 20 gen'i aşıyorsa çağırma, `blocked` yap.

Çağrılar (ekran başına bir çağrı, bağımsız oldukları için paralel):
| Çıktı | Ölçü | `pieces` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_teklif_<yon> | 416x192 | `ui_mockup.py pieces teklif --scale 2` | `marriage and child proposal card in the same material as the reference panel: header strip with a wide blank title plaque and a close button; an empty large portrait frame on the left with a blank name plaque under it; a blank inset terms card in the middle, a tall blank inset child card on the right, and two blank wide buttons at the bottom` | <= 20 |
| ui_panel_ayna_<yon> | 416x192 | `ui_mockup.py pieces ayna --scale 2` | `mirror screen in the same material as the reference panel: header strip with a blank title plaque and a close button; a large blank preview window on the left; a grid of eight blank option cards in two rows of four on the right; a blank gem price plaque and a blank wide button at the bottom right` | <= 20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup teklif --panel Assets/Art/Iso/_aday/ui_panel_teklif_<yon>.png --out art/results/213d-teklif-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayna --panel Assets/Art/Iso/_aday/ui_panel_ayna_<yon>.png --out art/results/213d-ayna-onizleme-x2.png
```

## Doğrulama
210'daki ölçütler (ölçü, renk sayısı, saydamlık, şablona uyum, görsel kontrol). Ek: çıktılar 210 paneliyle yan yana, aynı malzemeden mi (`art/results/213d-yanyana-x2.png`).

## Notlar
Yerleşim önizlemeleri (boş kutular): `art/results/213-teklif-yerlesim-x2.png`, `art/results/213-ayna-yerlesim-x2.png`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
