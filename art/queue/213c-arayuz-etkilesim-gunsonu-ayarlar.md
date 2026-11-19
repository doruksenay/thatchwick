---
id: "213c"
title: Arayüz ekranları (Etkileşim menüsü, Gün sonu özeti, Ayarlar)
status: pending
approved_by: user        # otomatik akış: 213 plan işi yazdı
family: arayuz
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # 211 ile aynı: stil görseli base64 olarak araca doğrudan verilmeli
outputs:
  - Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png
  - art/results/213c-etkilesim-onizleme-x2.png
  - art/results/213c-gun_sonu-onizleme-x2.png
  - art/results/213c-ayarlar-onizleme-x2.png
created: 2026-11-19
---
# 213c Arayüz ekranları: Etkileşim menüsü, Gün sonu özeti, Ayarlar

## Amaç
210'da seçilen yönle 3 ekranın boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` → `etkilesim`, `gun_sonu`, `ayarlar` (213 yazdı). Yazı, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Ortak ayarlar 211 ile aynı:** `create_ui_asset`, `max_colors=48`, `seed=0`, 210'daki ortak açıklama eki, `color_palette` 210 ile aynı, `style_image_base64` = `Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png` (en büyük ölçü sınırı varsa yarıya indir, raporda yaz).
- Ekran başına bir çağrı; bağımsız oldukları için paralel gönderilir.

### Çağrılar
| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_etkilesim_<yon> | 416x192 | `ui_mockup.py pieces etkilesim --scale 2` | `object interaction menu in the same material as the reference panel: header strip with blank title plaque and blank close button; a large blank inset menu area with six blank list buttons and a thin empty scrollbar track; on the right a small blank inset card and a larger blank inset card` | ≤20 |
| ui_panel_gun_sonu_<yon> | 416x192 | `ui_mockup.py pieces gun_sonu --scale 2` | `end of day summary in the same material as the reference panel: header strip with blank title plaque and blank close button; a large blank inset summary card with five blank list cards; on the right a blank inset winter gauge card with an empty progress bar frame, and a large blank wide button` | ≤20 |
| ui_panel_ayarlar_<yon> | 416x192 | `ui_mockup.py pieces ayarlar --scale 2` | `settings screen in the same material as the reference panel: header strip with blank title plaque and blank close button; a large blank inset list area with six blank list cards, three small blank option buttons on the third card` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup etkilesim --panel Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png --out art/results/213c-etkilesim-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup gun_sonu --panel Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png --out art/results/213c-gun_sonu-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayarlar --panel Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png --out art/results/213c-ayarlar-onizleme-x2.png
```

### Teslim ve masaüstü kopyası
210 ile aynı kurallar. Masaüstü klasörü: `...\art\arayuz\<yyyy-mm-dd>_etkilesim-gunsonu-ayarlar_<yon>\`.

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Çıktılar 210 paneliyle yan yana aynı malzemeden mi görünüyor? `art/results/213c-yanyana-x2.png`.

## Notlar
- Kutu etiketleri yerleşim dosyasından gelir; yeni ekran eklenirse önce `ui_layouts.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
