---
id: "541"
title: "Yeni ekinler 2/7: pamuk, kavun, karpuz, hıyar, şerbetçiotu (15)"
status: running
claimed_at: 2026-11-20T13:21Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 20
family: bitki
style_gate: false
cost_est: 15
cost_max: 30
depends_on: ["374"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_cotton_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_cotton_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_cotton_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_melon_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_melon_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_melon_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_watermelon_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_watermelon_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_watermelon_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_cucumber_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_cucumber_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_cucumber_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_hops_1_se.png
  - Assets/Art/Iso/_aday/iso_crop_hops_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_hops_3_se.png
created: 2026-11-20
---
# 541 Yeni ekinler 2/7: pamuk, kavun, karpuz, hıyar, şerbetçiotu (15)

## Amaç
Project belgesi 59 §5: yaz bostanı ve sırık ekinleri. Üç evre: filiz, gelişmiş bitki, **olgun** (olgun görsel yalnızca ekin biçilmeye hazırken çizilir; bu yüzden 3. evre üründen açıkça okunmalı: başak, meyve, çiçek ya da kök başı görünür olsun).

## Tarif
374 ile aynı yöntem: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24, `se`; evre yükseklikleri 10 / 22 / 32 (sırıklı ve uzun ekinlerde 3. evre 40). Referanslar bitki ailesinin referansları; 374'teki açıklama eki aynen eklenir. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| items.json id | Dosya | Açıklama metni (+ ek) |
|---|---|---|
| `item.crop_cotton` (pamuk) | `iso_crop_cotton_1_se.png` | `young cotton seedlings with two broad leaves` |
| `item.crop_cotton` (pamuk) | `iso_crop_cotton_2_se.png` | `bushy cotton plant with lobed leaves and pale yellow flowers` |
| `item.crop_cotton` (pamuk) | `iso_crop_cotton_3_se.png` | `cotton plant with open bolls showing white fluffy cotton` |
| `item.crop_melon` (kavun) | `iso_crop_melon_1_se.png` | `melon seedlings with round lobed leaves` |
| `item.crop_melon` (kavun) | `iso_crop_melon_2_se.png` | `melon vine spreading on the ground with yellow flowers` |
| `item.crop_melon` (kavun) | `iso_crop_melon_3_se.png` | `melon vine with two ripe netted yellow-green melons lying on the ground` |
| `item.crop_watermelon` (karpuz) | `iso_crop_watermelon_1_se.png` | `watermelon seedlings with deeply cut leaves` |
| `item.crop_watermelon` (karpuz) | `iso_crop_watermelon_2_se.png` | `watermelon vine spreading on the ground with small yellow flowers` |
| `item.crop_watermelon` (karpuz) | `iso_crop_watermelon_3_se.png` | `watermelon vine with one large dark green striped watermelon` |
| `item.crop_cucumber` (hıyar) | `iso_crop_cucumber_1_se.png` | `cucumber seedlings beside a thin wooden stake` |
| `item.crop_cucumber` (hıyar) | `iso_crop_cucumber_2_se.png` | `cucumber vine climbing a wooden stake with yellow flowers` |
| `item.crop_cucumber` (hıyar) | `iso_crop_cucumber_3_se.png` | `cucumber vine on a wooden stake with several hanging green cucumbers` |
| `item.crop_hops` (şerbetçiotu) | `iso_crop_hops_1_se.png` | `hop shoots at the foot of a tall wooden pole` |
| `item.crop_hops` (şerbetçiotu) | `iso_crop_hops_2_se.png` | `hop bine twining halfway up a tall wooden pole` |
| `item.crop_hops` (şerbetçiotu) | `iso_crop_hops_3_se.png` | `hop bine covering a tall wooden pole with clusters of pale green hop cones` |

- Maliyet 15 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family bitki <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod evreleri adla bulur (`IsoArt.Crops`: `iso_crop_<ad>_1/_2/_3_se`); gelene kadar renkli şekil çizer.
- Sararmış (mevsimi geçmiş) ekin için ayrı kare yok: kod olgun kareyi sarıya boyar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
