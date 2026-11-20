---
id: "499"
title: "Eşya kalitesi rozetleri (4)"
status: review
claimed_at: 2026-11-20T11:10Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("taşı ve başla. tek prda hallet"), Project belgesi 53
priority: 30
family: arayuz
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["480"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_quality_1.png
  - Assets/Art/Iso/_aday/ui_icon_quality_2.png
  - Assets/Art/Iso/_aday/ui_icon_quality_3.png
  - Assets/Art/Iso/_aday/ui_icon_quality_4.png
created: 2026-11-20
---
# 499 Eşya kalitesi rozetleri (4)

## Amaç
Eşya kalitesi (Project belgesi 53, beş renkli düzen): envanter, pazar ve kiler satırında eşya ikonunun sağ alt köşesinde küçük rozet. Kod hazır: `UiSkin.QualityCorner` → `Resources/UI/Icons/ui_icon_quality_<1-4>.png`. Rozet yoksa renkli yazı (★, ★★, ✦) çizilir. Sıradan (0) rozetsizdir, ikonu yok.

| Kalite | Renk | Rozet |
|---|---|---|
| 1 Seçme / Fine | yeşil | tek yıldız |
| 2 Has / Superior | mavi | iki yıldız |
| 3 Ustalık eseri / Masterwork | mor | mühür |
| 4 Yadigâr / Heirloom | altın | mühür |

## Tarif
**480 ile aynı yöntem** (`create_map_object` 32x32, saydam, `view: side`, `single color outline`, `basic shading`, `medium detail`; okunmazsa `create_image_pixflux` 32x32 `no_background=true`; teslim öncesi 16 renge indir, yarı saydam 0). Ortak ek: `, single game icon badge, one object centered, bold simple shape readable at 12 pixels, no frame, no background, no text, no letters, no face`. Kontur koyu kahve; nakış ve mühür dili (belge 17), parlak neon yok.

| Dosya | Açıklama (+ ortak ek) |
|---|---|
| ui_icon_quality_1 | `a single five-pointed star embroidered in green thread on a tiny cream linen patch` |
| ui_icon_quality_2 | `two small five-pointed stars side by side embroidered in blue thread on a tiny cream linen patch` |
| ui_icon_quality_3 | `a round wax seal in deep purple with a simple four-pointed star pressed into it` |
| ui_icon_quality_4 | `a round wax seal in warm gold with a simple four-pointed star pressed into it and a tiny ribbon below` |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk; `python tools/art/style.py check --family arayuz <dosya> ...`.
- 12x12 ve 16x16 küçültmede okunurluk: 4 rozet yan yana `art/results/499-yanyana.png` (koyu ve keten zeminde). Yeşil ve mavi yalnız renkle değil yıldız sayısıyla da ayrılmalı (renk körlüğü); mor ve altın mühür renk dışında kurdeleyle ayrılır.
- Renkler kod rengine yakın: yeşil #73C75C, mavi #5C99F2, mor #B873EB, altın #F2C240 (`ItemQuality.Color`).

## Notlar
- Kabul edilince dosyalar `Assets/Resources/UI/Icons/` altına da kopyalanır (oyun oradan yükler; ad aynı kalır).
- Rozet ikon hücresinin yarısı boyunda çizilir (32 px hücrede 16 px).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
