---
id: "213a"
title: Arayüz ekranları (yapi, esya_karti, pazar)
status: pending
approved_by: user        # otomatik akış: 213 plan işinin ürettiği iş
family: arayuz
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # stil görseli base64 olarak araca verilir (211 notu); yerel işçi
priority: 50
outputs:
  - Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png
created: 2026-11-19
---
# 213a Arayüz ekranları: yapi, esya_karti, pazar

## Amaç
210'da seçilen yönle 3 ekranın boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` → yapi, esya_karti, pazar. Yazı, portre ve ikonlar koddan gelir.

## Tarif
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Araç:** `create_ui_asset`, 211 ile aynı ortak ayarlar (`max_colors=48`, `seed=0`, `color_palette` = 210'daki).
- `style_image_base64` = `Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png` (210'da kabul edilen/varsayılan panel). Araç sınır koyarsa en yakın komşuyla yarıya indir, raporda yaz.
- Ekran başına bir çağrı (≤ 20 gen). Ortak açıklama eki 210'daki gibi; ekrana özgü kısım parça etiketlerinden türetilir.

| Çıktı | Ölçü | `pieces` | Açıklama | Gen |
|---|---|---|---|---|
| ui_panel_yapi_<yon> | 416x192 | `ui_mockup.py pieces yapi --scale 2` (832 -> 512 ölçekle; rect -> rounded_rect radius 0) | `yapi` ekranının parçaları `tools/art/ui_layouts.json` içinde etiketli; açıklama = ortak ek + parça etiketlerinin kısa özeti (hepsi boş, yazısız) | ≤20 |
| ui_panel_esya_karti_<yon> | 416x192 | `ui_mockup.py pieces esya_karti --scale 2` (832 -> 512 ölçekle; rect -> rounded_rect radius 0) | `esya_karti` ekranının parçaları `tools/art/ui_layouts.json` içinde etiketli; açıklama = ortak ek + parça etiketlerinin kısa özeti (hepsi boş, yazısız) | ≤20 |
| ui_panel_pazar_<yon> | 416x192 | `ui_mockup.py pieces pazar --scale 2` (832 -> 512 ölçekle; rect -> rounded_rect radius 0) | `pazar` ekranının parçaları `tools/art/ui_layouts.json` içinde etiketli; açıklama = ortak ek + parça etiketlerinin kısa özeti (hepsi boş, yazısız) | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup yapi --panel Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png --out art/results/213a-yapi-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup esya_karti --panel Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png --out art/results/213a-esya_karti-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup pazar --panel Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png --out art/results/213a-pazar-onizleme-x2.png
```

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Çıktılar 210 paneliyle yan yana aynı malzemeden mi? `art/results/213a-yanyana-x2.png`.
- Placeholder yerleşim önizlemeleri: `art/results/213-yapi-yerlesim-x2.png`, `art/results/213-esya_karti-yerlesim-x2.png`, `art/results/213-pazar-yerlesim-x2.png`.

## Notlar
- 211 gibi: stil görselinin base64'ü yerel işçide aktarılabiliyorsa çalışır; bulut şeridi aktaramıyorsa `blocked` + soru yazar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
