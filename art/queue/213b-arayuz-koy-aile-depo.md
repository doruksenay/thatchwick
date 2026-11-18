---
id: "213b"
title: Arayüz ekranları (koy, aile, depo)
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
  - Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png
  - art/results/213b-koy-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png
  - art/results/213b-aile-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png
  - art/results/213b-depo-onizleme-x2.png
created: 2026-11-18
---
# 213b Arayüz ekranları: koy, aile, depo

## Amaç
210'da seçilen yönle (`yon:` satırı yoksa varsayılan `parsomen`) 3 ekranın boş paneli. Yerleşim `tools/art/ui_layouts.json` içinde: koy, aile, depo. Yazı, ikon ve portre koddan gelir.

## Tarif
211 ile aynı ayarlar: `create_ui_asset`, `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`, `color_palette` 210 ile aynı. `style_image_base64` = 210'da kabul edilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`); araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz. Her açıklamanın sonuna 210'daki ortak eki ekle. Önce araç açıklamasındaki maliyeti oku; 20 gen'i aşıyorsa çağırma, `blocked` yap.

Çağrılar (ekran başına bir çağrı, bağımsız oldukları için paralel):
| Çıktı | Ölçü | `pieces` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_koy_<yon> | 416x192 | `ui_mockup.py pieces koy --scale 2` | `village panel in the same material as the reference panel: header strip with a blank title plaque, three blank tabs (the first lighter and joined to the body) and a close button; six blank villager row cards in a column, each with an empty thin progress bar frame; on the right a blank festival card and two blank project cards` | <= 20 |
| ui_panel_aile_<yon> | 416x192 | `ui_mockup.py pieces aile --scale 2` | `family tree screen in the same material as the reference panel: header strip with a wide blank title plaque and a close button; a large blank inset tree area on the left with six empty round portrait frames; a tall blank inset list card on the right with four blank row cards` | <= 20 |
| ui_panel_depo_<yon> | 416x192 | `ui_mockup.py pieces depo --scale 2` | `storage inventory screen in the same material as the reference panel: header strip with a blank title plaque and a close button; a blank grid area with four rows of eight empty square item slots, a thin empty capacity bar frame under it; on the right a blank inset info card and a blank wide button` | <= 20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup koy --panel Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png --out art/results/213b-koy-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup aile --panel Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png --out art/results/213b-aile-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup depo --panel Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png --out art/results/213b-depo-onizleme-x2.png
```

## Doğrulama
210'daki ölçütler (ölçü, renk sayısı, saydamlık, şablona uyum, görsel kontrol). Ek: çıktılar 210 paneliyle yan yana, aynı malzemeden mi (`art/results/213b-yanyana-x2.png`).

## Notlar
Yerleşim önizlemeleri (boş kutular): `art/results/213-koy-yerlesim-x2.png`, `art/results/213-aile-yerlesim-x2.png`, `art/results/213-depo-yerlesim-x2.png`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
