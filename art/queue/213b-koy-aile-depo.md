---
id: "213b"
title: Arayüz: köy paneli, aile ağacı, depo
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
  - Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png
  - art/results/213b-koy-onizleme-x2.png
  - art/results/213b-aile-onizleme-x2.png
  - art/results/213b-depo-onizleme-x2.png
created: 2026-11-19
---
# 213b Arayüz: köy paneli, aile ağacı, depo

## Amaç
213 planından: koy, aile, depo ekran(lar)ının boş panelleri. Yerleşimler `tools/art/ui_layouts.json` içinde aynı adlı anahtarlarda. Metin, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
211 ile aynı: `create_ui_asset`, `max_colors=48`, `seed=0`, aynı ortak açıklama eki, aynı `color_palette`. `style_image_base64` = 210'da kabul edilen/seçilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`; araç büyük ölçüyü reddederse en yakın komşuyla yarıya indir, raporda yaz). **Yön:** 210'daki `yon:` satırı, yoksa varsayılan `parsomen`; raporda yaz.

| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_koy_<yon> | 416x192 | `ui_mockup.py pieces koy --scale 2` | `village panel in the same material as the reference panel: header strip with blank name plaque and close button; five blank villager row cards stacked on the left; a blank portrait frame and a blank wide relation card with two thin blank bar tracks in the middle top; two blank cards side by side below it; a blank wide strip card at the bottom right` | ≤20 |
| ui_panel_aile_<yon> | 416x192 | `ui_mockup.py pieces aile --scale 2` | `family tree screen in the same material as the reference panel: header strip with blank name plaque and close button; a large blank tree area on the left holding seven small blank rounded portrait frames arranged in three generations; on the right a small blank heading plaque and four blank history cards stacked` | ≤20 |
| ui_panel_depo_<yon> | 416x192 | `ui_mockup.py pieces depo --scale 2` | `storage screen in the same material as the reference panel: header strip with blank name plaque and close button; a grid of 24 small blank square inventory slots in three rows of eight; under it a thin blank capacity bar track and a blank tab strip; on the right a blank item detail card and two blank buttons; a thin blank note strip along the bottom` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup koy --panel Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png --out art/results/213b-koy-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup aile --panel Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png --out art/results/213b-aile-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup depo --panel Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png --out art/results/213b-depo-onizleme-x2.png
```

## Doğrulama
211'deki ölçütler: ölçü 416x192 (ya da tam katı), renk sayısı, saydamlık, şablona uyum tablosu, 210 paneliyle yan yana (2 kat) aynı malzemeden mi görünüyor (`art/results/213b-yanyana-x2.png`). Yerleşim önizlemeleri `art/results/213-<ekran>-yerlesim-x2.png` hazır.

## Sorular
(yok)

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
