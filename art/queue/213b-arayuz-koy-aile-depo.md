---
id: "213b"
title: Arayüz ekranları (Köy paneli, Aile ağacı, Depo ve envanter)
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
  - Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png
  - art/results/213b-koy-onizleme-x2.png
  - art/results/213b-aile-onizleme-x2.png
  - art/results/213b-depo-onizleme-x2.png
created: 2026-11-19
---
# 213b Arayüz ekranları: Köy paneli, Aile ağacı, Depo ve envanter

## Amaç
210'da seçilen yönle 3 ekranın boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` → `koy`, `aile`, `depo` (213 yazdı). Yazı, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Ortak ayarlar 211 ile aynı:** `create_ui_asset`, `max_colors=48`, `seed=0`, 210'daki ortak açıklama eki, `color_palette` 210 ile aynı, `style_image_base64` = `Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png` (en büyük ölçü sınırı varsa yarıya indir, raporda yaz).
- Ekran başına bir çağrı; bağımsız oldukları için paralel gönderilir.

### Çağrılar
| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_koy_<yon> | 416x192 | `ui_mockup.py pieces koy --scale 2` | `village panel in the same material as the reference panel: header strip with blank title plaque and blank close button; a tall blank inset list area on the left with six blank list cards; on the right a blank inset card holding three empty progress bar frames, a smaller blank inset card, and a blank inset card with one empty progress bar frame at the bottom` | ≤20 |
| ui_panel_aile_<yon> | 416x192 | `ui_mockup.py pieces aile --scale 2` | `family tree screen in the same material as the reference panel: header strip with blank title plaque and blank close button; a large blank inset tree area on the left with six empty small portrait node frames arranged in three rows; a tall blank inset list area on the right with four blank list cards` | ≤20 |
| ui_panel_depo_<yon> | 416x192 | `ui_mockup.py pieces depo --scale 2` | `storage and inventory screen in the same material as the reference panel: header strip with blank title plaque and blank close button; a large blank inset area with a grid of four rows by eight empty square inventory slots; on the right a small blank inset card with an empty progress bar frame, a blank inset info card and a blank wide button` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup koy --panel Assets/Art/Iso/_aday/ui_panel_koy_<yon>.png --out art/results/213b-koy-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup aile --panel Assets/Art/Iso/_aday/ui_panel_aile_<yon>.png --out art/results/213b-aile-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup depo --panel Assets/Art/Iso/_aday/ui_panel_depo_<yon>.png --out art/results/213b-depo-onizleme-x2.png
```

### Teslim ve masaüstü kopyası
210 ile aynı kurallar. Masaüstü klasörü: `...\art\arayuz\<yyyy-mm-dd>_koy-aile-depo_<yon>\`.

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Çıktılar 210 paneliyle yan yana aynı malzemeden mi görünüyor? `art/results/213b-yanyana-x2.png`.

## Notlar
- Kutu etiketleri yerleşim dosyasından gelir; yeni ekran eklenirse önce `ui_layouts.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
