---
id: "547"
title: "Görünür dev ürünler: 7 tarla görseli ve 7 eşya ikonu (14)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 25
family: bitki
style_gate: false
cost_est: 14
cost_max: 28
depends_on: ["374", "316"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_giant_turnip_2x2_se.png
  - Assets/Art/Iso/_aday/iso_crop_giant_cabbage_2x2_se.png
  - Assets/Art/Iso/_aday/iso_crop_giant_pumpkin_2x2_se.png
  - Assets/Art/Iso/_aday/iso_crop_giant_melon_2x2_se.png
  - Assets/Art/Iso/_aday/iso_crop_giant_watermelon_2x2_se.png
  - Assets/Art/Iso/_aday/iso_crop_giant_radish_2x2_se.png
  - Assets/Art/Iso/_aday/iso_crop_giant_beet_2x2_se.png
  - Assets/Art/Iso/_aday/icon_item_giant_turnip.png
  - Assets/Art/Iso/_aday/icon_item_giant_cabbage.png
  - Assets/Art/Iso/_aday/icon_item_giant_pumpkin.png
  - Assets/Art/Iso/_aday/icon_item_giant_melon.png
  - Assets/Art/Iso/_aday/icon_item_giant_watermelon.png
  - Assets/Art/Iso/_aday/icon_item_giant_radish.png
  - Assets/Art/Iso/_aday/icon_item_giant_beet.png
created: 2026-11-20
---
# 547 Görünür dev ürünler: 7 tarla görseli ve 7 eşya ikonu (14)

## Amaç
Project belgesi 59 §2: aynı ekinden 2×2 olgun karo küçük şansla **tek dev ürüne** dönüşür. Dev ürün tarlada 2×2 yer kaplar, hasat edilince "dev ürün" eşyası olur (Hasat sergisi, ev dekoru).

## Tarif
- Tarla görseli: 2x2 karo ayak izi (`tools/art/footprint_guide.py` ile 2x2 kılavuz ve maske; çok karolu eşya yöntemi, 301). Aile `bitki`, 374'teki açıklama eki. Ürün ayak izinin çoğunu doldursun, yüksekliği bir karakterin beline gelsin (yaklaşık 44 px).
- İkon: 316 ile aynı stil (32x32, saydam, tek renk koyu kontur, en çok 16 renk); açıklamanın sonuna `, simple still-life object, no face, no eyes, no creature, no person`.

| Dosya | Boyut | Açıklama |
|---|---|---|
| `iso_crop_giant_turnip_2x2_se.png` | 2x2 | `one enormous white-and-purple turnip half out of the soil with huge leaves` |
| `icon_item_giant_turnip.png` | 32x32 | `a huge white-and-purple turnip half out of the soil, show-quality vegetable` |
| `iso_crop_giant_cabbage_2x2_se.png` | 2x2 | `one enormous round green cabbage with wide outer leaves` |
| `icon_item_giant_cabbage.png` | 32x32 | `a huge round green cabbage, show-quality vegetable` |
| `iso_crop_giant_pumpkin_2x2_se.png` | 2x2 | `one enormous pale green bottle gourd lying on its side with vine leaves` |
| `icon_item_giant_pumpkin.png` | 32x32 | `a huge pale green bottle gourd lying on its side, show-quality vegetable` |
| `iso_crop_giant_melon_2x2_se.png` | 2x2 | `one enormous netted yellow melon with vine leaves` |
| `icon_item_giant_melon.png` | 32x32 | `a huge netted yellow melon, show-quality vegetable` |
| `iso_crop_giant_watermelon_2x2_se.png` | 2x2 | `one enormous dark green striped watermelon with vine leaves` |
| `icon_item_giant_watermelon.png` | 32x32 | `a huge dark green striped watermelon, show-quality vegetable` |
| `iso_crop_giant_radish_2x2_se.png` | 2x2 | `one enormous black radish half out of the soil with rough leaves` |
| `icon_item_giant_radish.png` | 32x32 | `a huge black radish half out of the soil, show-quality vegetable` |
| `iso_crop_giant_beet_2x2_se.png` | 2x2 | `one enormous dark red beet half out of the soil with red-veined leaves` |
| `icon_item_giant_beet.png` | 32x32 | `a huge dark red beet half out of the soil, show-quality vegetable` |

## Doğrulama
`validate_iso.py`, `style.py check --family bitki` (tarla), `--family arayuz` (ikon). Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod: `iso_crop_giant_<ad>_2x2_se`, ikon `icon_item_giant_<ad>`. Görsel gelene kadar dört olgun kare büyütülerek çizilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
