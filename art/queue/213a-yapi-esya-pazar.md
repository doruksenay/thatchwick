---
id: "213a"
title: Arayüz: yapı modu, eşya kartı, pazar
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
  - Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png
  - art/results/213a-yapi-onizleme-x2.png
  - art/results/213a-esya_karti-onizleme-x2.png
  - art/results/213a-pazar-onizleme-x2.png
created: 2026-11-19
---
# 213a Arayüz: yapı modu, eşya kartı, pazar

## Amaç
213 planından: yapi, esya_karti, pazar ekran(lar)ının boş panelleri. Yerleşimler `tools/art/ui_layouts.json` içinde aynı adlı anahtarlarda. Metin, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
211 ile aynı: `create_ui_asset`, `max_colors=48`, `seed=0`, aynı ortak açıklama eki, aynı `color_palette`. `style_image_base64` = 210'da kabul edilen/seçilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`; araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz). **Yön:** 210'daki `yon:` satırı, yoksa varsayılan `parsomen`; raporda yaz.

| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_yapi_<yon> | 416x192 | `ui_mockup.py pieces yapi --scale 2` | `building mode screen in the same material as the reference panel: header strip with a blank name plaque and a small blank close button; a column of four blank square tool buttons and two small blank floor buttons on the left; a row of five blank category tabs (the middle one lighter, active); a grid of six blank catalog cards in two rows of three; a blank value card and two blank wide buttons on the right` | ≤20 |
| ui_panel_esya_karti_<yon> | 416x192 | `ui_mockup.py pieces esya_karti --scale 2` | `item info card in the same material as the reference panel: header strip with blank name plaque and close button; a blank square item preview window with a blank price plaque and a small blank badge under it; two wide blank info cards stacked in the middle; three blank buttons stacked on the right; a thin blank bar track at the bottom of the middle column` | ≤20 |
| ui_panel_pazar_<yon> | 416x192 | `ui_mockup.py pieces pazar --scale 2` | `market screen in the same material as the reference panel: header strip with blank name plaque and close button; two small blank tabs (first lighter, active) and a blank coin plaque; five blank wide list row cards stacked on the left with a thin blank scroll track beside them; a blank offer card on the right, two small blank square buttons with a blank plaque between them, and a blank wide confirm button` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup yapi --panel Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png --out art/results/213a-yapi-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup esya_karti --panel Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png --out art/results/213a-esya_karti-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup pazar --panel Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png --out art/results/213a-pazar-onizleme-x2.png
```

## Doğrulama
211'deki ölçütler: ölçü 416x192 (ya da tam katı), renk sayısı, saydamlık, şablona uyum tablosu, 210 paneliyle yan yana (2 kat) aynı malzemeden mi görünüyor (`art/results/213a-yanyana-x2.png`). Yerleşim önizlemeleri `art/results/213-<ekran>-yerlesim-x2.png` hazır.

## Sorular
(yok)

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
