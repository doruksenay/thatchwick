---
id: "213d"
title: Arayüz ekranları (Teklif kartı, Ayna)
status: pending
approved_by: user        # otomatik akış: 213 plan işi yazdı
family: arayuz
style_gate: false
cost_est: 30
cost_max: 40
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # 211 ile aynı: stil görseli base64 olarak araca doğrudan verilmeli
outputs:
  - Assets/Art/Iso/_aday/ui_panel_teklif_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_ayna_<yon>.png
  - art/results/213d-teklif-onizleme-x2.png
  - art/results/213d-ayna-onizleme-x2.png
created: 2026-11-19
---
# 213d Arayüz ekranları: Teklif kartı, Ayna

## Amaç
210'da seçilen yönle 2 ekranın boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` → `teklif`, `ayna` (213 yazdı). Yazı, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Ortak ayarlar 211 ile aynı:** `create_ui_asset`, `max_colors=48`, `seed=0`, 210'daki ortak açıklama eki, `color_palette` 210 ile aynı, `style_image_base64` = `Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png` (en büyük ölçü sınırı varsa yarıya indir, raporda yaz).
- Ekran başına bir çağrı; bağımsız oldukları için paralel gönderilir.

### Çağrılar
| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_teklif_<yon> | 416x192 | `ui_mockup.py pieces teklif --scale 2` | `proposal card in the same material as the reference panel: header strip with blank title plaque and blank close button; a large empty portrait frame at the left, a blank name plaque and a blank inset card beside it, a blank inset card on the right, a wide blank inset message card, and a bottom row of three blank buttons (the first one a confirm button)` | ≤20 |
| ui_panel_ayna_<yon> | 416x192 | `ui_mockup.py pieces ayna --scale 2` | `mirror screen in the same material as the reference panel: header strip with blank title plaque and blank close button; a tall blank preview window at the left with two small blank square buttons under it; a blank inset area with two rows of four blank option cards; a bottom row with a blank diamond price slot and a blank confirm button` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup teklif --panel Assets/Art/Iso/_aday/ui_panel_teklif_<yon>.png --out art/results/213d-teklif-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayna --panel Assets/Art/Iso/_aday/ui_panel_ayna_<yon>.png --out art/results/213d-ayna-onizleme-x2.png
```

### Teslim ve masaüstü kopyası
210 ile aynı kurallar. Masaüstü klasörü: `...\art\arayuz\<yyyy-mm-dd>_teklif-ayna_<yon>\`.

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Çıktılar 210 paneliyle yan yana aynı malzemeden mi görünüyor? `art/results/213d-yanyana-x2.png`.

## Notlar
- Kutu etiketleri yerleşim dosyasından gelir; yeni ekran eklenirse önce `ui_layouts.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
