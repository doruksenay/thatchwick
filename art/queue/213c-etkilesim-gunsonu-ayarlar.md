---
id: "213c"
title: Arayüz: etkileşim menüsü, gün sonu, ayarlar
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (213 planı)
family: arayuz
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # 211 ile aynı: stil görseli base64 olarak araca doğrudan verilmeli; bulut işçisi güvenilir aktaramıyor
outputs:
  - Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png
  - art/results/213c-etkilesim-onizleme-x2.png
  - art/results/213c-gun_sonu-onizleme-x2.png
  - art/results/213c-ayarlar-onizleme-x2.png
created: 2026-11-19
---
# 213c Arayüz: etkileşim menüsü, gün sonu, ayarlar

## Amaç
213 planından: etkilesim, gun_sonu, ayarlar ekran(lar)ının boş panelleri. Yerleşimler `tools/art/ui_layouts.json` içinde aynı adlı anahtarlarda. Metin, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
211 ile aynı: `create_ui_asset`, `max_colors=48`, `seed=0`, aynı ortak açıklama eki, aynı `color_palette`. `style_image_base64` = 210'da kabul edilen/seçilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`; araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz). **Yön:** 210'daki `yon:` satırı, yoksa varsayılan `parsomen`; raporda yaz.

| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_etkilesim_<yon> | 416x192 | `ui_mockup.py pieces etkilesim --scale 2` | `interaction menu in the same material as the reference panel: header strip with blank name plaque and close button; a blank square item preview window with a blank name plaque and a blank state card under it on the left; five blank wide menu row buttons stacked in the middle; a tall blank scroll track with a small blank thumb on the right` | ≤20 |
| ui_panel_gun_sonu_<yon> | 416x192 | `ui_mockup.py pieces gun_sonu --scale 2` | `end of day summary in the same material as the reference panel: header strip with blank name plaque and close button; a row of four blank stat cards; a wide blank winter indicator card holding a thin blank gauge track; a blank note card at the bottom left and a blank confirm button at the bottom right` | ≤20 |
| ui_panel_ayarlar_<yon> | 416x192 | `ui_mockup.py pieces ayarlar --scale 2` | `settings screen in the same material as the reference panel: header strip with blank name plaque and close button; three blank settings row cards stacked on the left each with a thin blank slider track; on the right a blank heading plaque, a row of three blank segment buttons (the middle one lighter, active) and a blank note card; a row of three blank buttons and a thin blank footer strip at the bottom` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup etkilesim --panel Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png --out art/results/213c-etkilesim-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup gun_sonu --panel Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png --out art/results/213c-gun_sonu-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayarlar --panel Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png --out art/results/213c-ayarlar-onizleme-x2.png
```

## Doğrulama
211'deki ölçütler: ölçü 416x192 (ya da tam katı), renk sayısı, saydamlık, şablona uyum tablosu, 210 paneliyle yan yana (2 kat) aynı malzemeden mi görünüyor (`art/results/213c-yanyana-x2.png`). Yerleşim önizlemeleri `art/results/213-<ekran>-yerlesim-x2.png` hazır.

## Sorular
(yok)

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
