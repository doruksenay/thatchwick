---
id: "213c"
title: Arayüz ekranları etkilesim, gun_sonu, ayarlar
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (213 plan işi)
family: arayuz
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["210"]
max_attempts: 1
pixellab: true
worker: yerel            # stil görseli base64 olarak araca doğrudan verilmeli (211'deki gibi, kullanıcı kararı 2026-11-19)
outputs:
  - Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png
  - art/results/213c-etkilesim-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png
  - art/results/213c-gun_sonu-onizleme-x2.png
  - Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png
  - art/results/213c-ayarlar-onizleme-x2.png
created: 2026-11-20
---
# 213c Arayüz ekranları: etkilesim, gun_sonu, ayarlar

## Amaç
210'da seçilen yönle 3 ekranın boş panelini üretir. Yerleşimler `tools/art/ui_layouts.json` içindeki aynı adlı anahtarlardır (213 yazdı).

## Tarif
- **Ön adım:** `tools/art/ui_layouts.json` içinde `etkilesim` anahtarı yoksa 213'ün `art/results/213-ui_layouts-ek.json` dosyasındaki anahtarları bu dosyaya birleştir (yerel işçi `tools/art` yazabilir; 213 bulutta yazamadı).
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` altındaki `yon:` satırı; yoksa varsayılan `parsomen` (raporda "varsayılan yön" yaz).
- **Ortak ayarlar 211 ile aynı:** `create_ui_asset`, `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`, `color_palette` 210 ile aynı. `style_image_base64` = 210'da seçilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`), dosyadan base64'e çevrilir; büyük sınır varsa en yakın komşuyla yarıya indir ve yaz. `pieces`: `ui_mockup.py pieces <ekran> --scale 2` çıktısı; araç sanal tuvali 512'ye sınırlıyorsa orantılı ölçekle (`rect` -> `rounded_rect` radius 0), raporda yaz.
- Her açıklamanın sonuna ek: `, pixel art game UI screen panel for a cozy medieval village life simulation, earthy muted medieval palette, flat simple shading, clean dark outline, light from top-left, crisp readable shapes, no dithering, no noise, no text, no letters, no numbers, no icons, no faces, all plaques, tabs, cards, slots and bars empty and blank`
- Önce araç açıklamasını oku; tek çağrı 20 gen'i geçecekse çağırma, `blocked` yap.

### Çağrılar (ekran başına bir, bağımsız: paralel gönderilebilir)
| Çıktı | Ölçü | `pieces` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_etkilesim_<yon> | 416x192 | `ui_mockup.py pieces etkilesim --scale 2` | `object interaction menu screen in the same material as the reference panel, header strip with a blank title plaque and a small blank close button; a blank target name card on the left, a centered tall menu card holding five blank option buttons and a thin empty scrollbar track on its right edge` | ≤20 |
| ui_panel_gun_sonu_<yon> | 416x192 | `ui_mockup.py pieces gun_sonu --scale 2` | `end of day summary screen in the same material as the reference panel, header strip with a blank title plaque and a small blank close button; a large blank summary card on the left with four blank rows; on the right a blank winter gauge card holding an empty horizontal gauge frame and a blank wide confirm button below` | ≤20 |
| ui_panel_ayarlar_<yon> | 416x192 | `ui_mockup.py pieces ayarlar --scale 2` | `settings and pause screen in the same material as the reference panel, header strip with a blank title plaque and a small blank close button; six blank setting row cards in two columns of three, and two blank wide buttons side by side at the bottom` | ≤20 |

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup etkilesim --panel Assets/Art/Iso/_aday/ui_panel_etkilesim_<yon>.png --out art/results/213c-etkilesim-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup gun_sonu --panel Assets/Art/Iso/_aday/ui_panel_gun_sonu_<yon>.png --out art/results/213c-gun_sonu-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup ayarlar --panel Assets/Art/Iso/_aday/ui_panel_ayarlar_<yon>.png --out art/results/213c-ayarlar-onizleme-x2.png
```
Yerleşim önizlemeleri (PixelLab'siz): `art/results/213-<ekran>-yerlesim-x2.png`.

## Doğrulama
210'daki ölçütler (ölçü, renk sayısı, saydamlık, şablona uyum, görsel kontrol) + üç çıktı 210 paneliyle yan yana (`art/results/213c-yanyana-x2.png`).

## Notlar
Yazı, ikon, portre ve değerler koddan gelir; panel boş kalır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
