---
id: "213d"
title: Arayüz: evlilik/çocuk teklifi, ayna
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (213 planı)
family: arayuz
style_gate: false
cost_est: 30
cost_max: 40
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # 211 ile aynı: stil görseli base64 olarak araca doğrudan verilmeli; bulut işçisi güvenilir aktaramıyor
outputs:
  - Assets/Art/Iso/_aday/ui_panel_teklif_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_ayna_<yon>.png
  - art/results/213d-teklif-onizleme-x2.png
  - art/results/213d-ayna-onizleme-x2.png
created: 2026-11-19
---
# 213d Arayüz: evlilik/çocuk teklifi, ayna

## Amaç
213 planından: teklif, ayna ekran(lar)ının boş panelleri. Yerleşimler `tools/art/ui_layouts.json` içinde aynı adlı anahtarlarda. Metin, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
211 ile aynı: `create_ui_asset`, `max_colors=48`, `seed=0`, aynı ortak açıklama eki, aynı `color_palette`. `style_image_base64` = 210'da kabul edilen/seçilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`; araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz). **Yön:** 210'daki `yon:` satırı, yoksa varsayılan `parsomen`; raporda yaz.

| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_teklif_<yon> | 416x192 | `ui_mockup.py pieces teklif --scale 2` | `proposal card in the same material as the reference panel: header strip with blank name plaque and close button; a blank portrait frame with a blank name plaque under it; a wide blank info card, a blank trait card and a thin blank card below, in the middle; three blank buttons stacked on the right and a blank price plaque under them` | ≤20 |
| ui_panel_ayna_<yon> | 416x192 | `ui_mockup.py pieces ayna --scale 2` | `mirror screen in the same material as the reference panel: header strip with blank name plaque and close button; a tall blank preview window on the left; a row of four blank tabs (the first lighter, active) and two rows of four blank option buttons on the right; a blank price plaque and a blank confirm button at the bottom` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup teklif --panel Assets/Art/Iso/_aday/ui_panel_teklif_<yon>.png --out art/results/213d-teklif-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayna --panel Assets/Art/Iso/_aday/ui_panel_ayna_<yon>.png --out art/results/213d-ayna-onizleme-x2.png
```

## Doğrulama
211'deki ölçütler: ölçü 416x192 (ya da tam katı), renk sayısı, saydamlık, şablona uyum tablosu, 210 paneliyle yan yana (2 kat) aynı malzemeden mi görünüyor (`art/results/213d-yanyana-x2.png`). Yerleşim önizlemeleri `art/results/213-<ekran>-yerlesim-x2.png` hazır.

## Sorular
(yok)

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
