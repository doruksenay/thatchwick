---
id: "213a"
title: Arayüz ekranları (Yapı modu, Eşya bilgi kartı, Pazar)
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
  - Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png
  - art/results/213a-yapi-onizleme-x2.png
  - art/results/213a-esya_karti-onizleme-x2.png
  - art/results/213a-pazar-onizleme-x2.png
created: 2026-11-19
---
# 213a Arayüz ekranları: Yapı modu, Eşya bilgi kartı, Pazar

## Amaç
210'da seçilen yönle 3 ekranın boş paneli üretilir. Yerleşimler `tools/art/ui_layouts.json` → `yapi`, `esya_karti`, `pazar` (213 yazdı). Yazı, ikon ve portre koddan gelir; panelde boş kalır.

## Tarif
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Ortak ayarlar 211 ile aynı:** `create_ui_asset`, `max_colors=48`, `seed=0`, 210'daki ortak açıklama eki, `color_palette` 210 ile aynı, `style_image_base64` = `Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png` (en büyük ölçü sınırı varsa yarıya indir, raporda yaz).
- Ekran başına bir çağrı; bağımsız oldukları için paralel gönderilir.

### Çağrılar
| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_yapi_<yon> | 416x192 | `ui_mockup.py pieces yapi --scale 2` | `build mode screen in the same material as the reference panel: header strip with a blank title plaque and a small blank close button; a row of six blank square tool buttons; a row of six blank pill-shaped category tabs; a row of six blank tall catalog cards; a bottom row with a blank floor selector plaque, a wide blank value plaque and a blank confirm button` | ≤20 |
| ui_panel_esya_karti_<yon> | 416x192 | `ui_mockup.py pieces esya_karti --scale 2` | `item info card in the same material as the reference panel: header strip with blank title plaque and blank close button; a large blank square icon frame at the left, a blank name plaque and a small blank price plaque beside it, two small blank tag plaques under them, a wide blank inset description card, and a bottom row of three blank buttons` | ≤20 |
| ui_panel_pazar_<yon> | 416x192 | `ui_mockup.py pieces pazar --scale 2` | `market screen in the same material as the reference panel: header strip with blank title plaque and blank close button; under it two blank tabs (the first lighter, active) and a blank coin purse plaque; a large blank inset list area on the left with five blank list cards and a thin empty scrollbar track; a blank inset detail card on the right with a blank wide button under it` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup yapi --panel Assets/Art/Iso/_aday/ui_panel_yapi_<yon>.png --out art/results/213a-yapi-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup esya_karti --panel Assets/Art/Iso/_aday/ui_panel_esya_karti_<yon>.png --out art/results/213a-esya_karti-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup pazar --panel Assets/Art/Iso/_aday/ui_panel_pazar_<yon>.png --out art/results/213a-pazar-onizleme-x2.png
```

### Teslim ve masaüstü kopyası
210 ile aynı kurallar. Masaüstü klasörü: `...\art\arayuz\<yyyy-mm-dd>_yapi-esya-pazar_<yon>\`.

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- Çıktılar 210 paneliyle yan yana aynı malzemeden mi görünüyor? `art/results/213a-yanyana-x2.png`.

## Notlar
- Kutu etiketleri yerleşim dosyasından gelir; yeni ekran eklenirse önce `ui_layouts.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
