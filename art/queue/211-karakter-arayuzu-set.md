---
id: "211"
title: Karakter arayüzü seti (Kişi paneli, yaratma ekranı, HUD parçaları)
status: running
claimed_at: 2026-11-18T12:41Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: arayuz
style_gate: false
cost_est: 45
cost_max: 55
depends_on: ["210"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_panel_kisi_<yon>.png
  - Assets/Art/Iso/_aday/ui_panel_yaratma_<yon>.png
  - Assets/Art/Iso/_aday/ui_hud_kit_<yon>.png
  - art/results/211-kisi-onizleme-x2.png
  - art/results/211-yaratma-onizleme-x2.png
created: 2026-11-17
---
# 211 Karakter arayüzü seti

## Amaç
210'da seçilen yönle karakter arayüzünün kalan ekranları üretilir:
- karakter panelinin **Kişi** sekmesi;
- karakter **yaratma** ekranı (İz ve süs sekmesi);
- oyun içi HUD'un parçaları.

Hepsi 210 ile aynı dünyada durmalı. Yerleşimler `tools/art/ui_layouts.json` → `kisi`, `yaratma`.

## Tarif
- **Yön:** `art/queue/210-karakter-arayuzu-pilot.md` dosyasının altındaki `yon:` satırı (`parsomen` ya da `ahsap`). Satır yoksa varsayılan `parsomen` (otomatik akış). Raporda "varsayılan yön" diye belirt.
- **Ortak ayarlar 210 ile aynı.** `create_ui_asset`, `max_colors=48`, `seed=0`, aynı ortak açıklama eki.
  - **Fark:** `style_image_base64` = 210'da kabul edilen panel (`Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png`). Aynı malzeme ve dış hat kullanılsın.
  - Araç `style_image` için en büyük ölçü sınırı koyuyorsa paneli en yakın komşuyla yarıya indirip ver, raporda yaz.
- `color_palette` 210 ile aynı.

### Çağrılar
| Çıktı | Ölçü | `pieces` | Açıklama metni (yön malzemesiyle; + ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_kisi_<yon> | 416x192 | `ui_mockup.py pieces kisi --scale 2` | `character profile panel in the same material as the reference panel, header strip with a blank name plaque and four blank tabs where the second tab is lighter and joined to the body, a small blank close button; body with a large empty portrait frame on the left, under it a small blank round play button, a small blank plaque and a blank wide button; a blank inset card and two blank list cards in the second column; a tall blank inset card in the third column; a list area with three blank cards on the right` | ≤20 |
| ui_panel_yaratma_<yon> | 416x192 | `ui_mockup.py pieces yaratma --scale 2` | `character creator screen in the same material as the reference panel: a darker side panel on the left with two blank toggle buttons, a large blank preview window and three small blank buttons; a large options panel on the right with a row of seven blank tabs (the last one lighter, active), a row of four blank pill buttons, two rows of four blank option buttons, two rows of three blank option buttons, and a footer strip with three blank buttons on the left and one blank confirm button on the right` | ≤20 |
| ui_hud_kit_<yon> | 256x256 | aşağıdaki şablon | `HUD element sheet in the same material as the reference panel, separate blank pieces with transparent space between them: a long blank top bar strip, a blank character card, a square blank job card, a blank menu button, a blank square speed button, a blank notification toast, and a small blank round portrait ring` | ≤10 |

**HUD kit şablonu** (512x512 sanal tuval; yazı ve ikon yok, parçalar arasında saydam boşluk):
```json
[
  {"id": "topbar", "kind": "rounded_rect", "x": 16,  "y": 16,  "w": 480, "h": 56,  "radius": 8,  "label": "blank top bar strip"},
  {"id": "card",   "kind": "rounded_rect", "x": 16,  "y": 96,  "w": 304, "h": 112, "radius": 12, "label": "blank character card"},
  {"id": "job",    "kind": "rounded_rect", "x": 344, "y": 96,  "w": 72,  "h": 72,  "radius": 10, "label": "blank square job card"},
  {"id": "ring",   "kind": "ellipse",      "x": 432, "y": 96,  "w": 64,  "h": 64,                "label": "blank round portrait ring"},
  {"id": "menu",   "kind": "rounded_rect", "x": 16,  "y": 232, "w": 184, "h": 56,  "radius": 10, "label": "blank menu button"},
  {"id": "speed",  "kind": "rounded_rect", "x": 224, "y": 232, "w": 56,  "h": 56,  "radius": 10, "label": "blank square speed button"},
  {"id": "toast",  "kind": "rounded_rect", "x": 16,  "y": 312, "w": 480, "h": 72,  "radius": 10, "label": "blank notification toast"}
]
```

### Önizleme
```
uv run --with pillow python tools/art/ui_mockup.py mockup kisi --panel Assets/Art/Iso/_aday/ui_panel_kisi_<yon>.png --out art/results/211-kisi-onizleme-x2.png
uv run --with pillow python tools/art/ui_mockup.py mockup yaratma --panel Assets/Art/Iso/_aday/ui_panel_yaratma_<yon>.png --out art/results/211-yaratma-onizleme-x2.png
```
HUD kit için önizleme yok. Kit 2 kat büyütülüp `art/results/211-hud-kit-x2.png` olarak yazılır.

### Teslim ve masaüstü kopyası
210 ile aynı kurallar. Masaüstü klasörü: `...\art\arayuz\<yyyy-mm-dd>_set_<yon>\`.

## Doğrulama
- 210'daki ölçütler: ölçü, renk sayısı, saydamlık, şablona uyum tablosu, görsel kontrol.
- **Ek:** üç çıktı 210 paneliyle yan yana (2 kat) aynı malzemeden mi görünüyor? `art/results/211-yanyana-x2.png`: 210 paneli + üç çıktı.

## Notlar
- Kişi sekmesindeki beceri şeritleri, iz kartlarının içeriği ve portre koddan gelir. Panelde boş kalır.
- 315 ve 212 ikonları bu işten bağımsızdır; önizleme bulursa kullanır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
