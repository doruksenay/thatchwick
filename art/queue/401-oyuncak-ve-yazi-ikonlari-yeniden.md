---
id: "401"
title: Oyuncak (4) ve ürün ikonu (5) yeniden: 399'un reddedilen ve zayıf parçaları
status: review
claimed_at: 2026-11-18T18:46Z
approved_by: user        # kullanıcı onayı 18.11.2026 ("onaylıyorum"), 399 sonucu üzerine
priority: 20
family: esya
style_gate: false
cost_est: 9
cost_max: 12
depends_on: ["399"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_hoop_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_marbles_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_whittling_knife_se.png
  - Assets/Art/Iso/_aday/iso_item_juggling_balls_se.png
  - Assets/Art/Iso/_aday/icon_tallow.png
  - Assets/Art/Iso/_aday/icon_oak_gall.png
  - Assets/Art/Iso/_aday/icon_ink.png
  - Assets/Art/Iso/_aday/icon_malt.png
  - Assets/Art/Iso/_aday/icon_blank_book_v2.png
created: 2026-11-18
---
# 401 Oyuncak (4) ve ürün ikonu (5) yeniden

## Amaç
399'da kabul edilenler: sapan, atlama ipi, uçurtma, parşömen. Bu iş geri kalanları yeniden üretir (sonuç: `art/results/399-...md`, red klasörü `art/results/399-red/`). Var olan PNG'lerin üzerine yazılmaz: zayıf teslimlerin yenisi `_v2` adıyla.

## Ders (399'dan)
- `create_image_pixen` küçük nesnelerde yüz, göz ve figür üretti (iç yağı göz küresi, mazıda yüz, mürekkep yerine insan). **İkonların hepsi `create_map_object` 32x32 ile** (oyuncaklarla aynı yöntem, `high top-down`, `low detail`, `basic shading`, `single color outline`) üretilecek.
- Her açıklamanın sonuna ekle: `, simple still-life object, no face, no eyes, no creature, no person, no character, plain background`.
- Tek nesne, sade siluet, 2-4 ana renk. Renk paleti 316/399 ile aynı (sıcak toprak tonları, köylü takımı).

## Tarif
### Oyuncaklar (399 ile aynı yöntem ve kılavuz; tuval 32x32, yuva/zemin tabanı 399 sonucundaki gibi)
| # | items.json id | Dosya (`_aday/`) | Yer | Açıklama |
|---|---|---|---|---|
| 1 | `item.hoop` | `iso_item_hoop_se_v2.png` | zemin 1x1 (dış) | `large bent wooden hoop standing upright, a short straight wooden driving stick leaning against it` |
| 2 | `item.marbles` | `iso_item_marbles_se_v2.png` | yuva K | `small open cloth pouch with five round fired clay marbles spilled in front of it, marbles clearly visible` |
| 3 | `item.whittling_knife` | `iso_item_whittling_knife_se.png` | yuva K | `small knife with a wooden handle and a short shiny iron blade, lying next to a half-carved stick with curled wood shavings` |
| 4 | `item.juggling_balls` | `iso_item_juggling_balls_se.png` | yuva K | `three small round stitched leather balls in a little pile, brown, tan and dull red` |

### Ürün ikonları (`create_map_object` 32x32, tek nesne, zemin yok)
| # | Dosya (`_aday/`) | Açıklama |
|---|---|---|
| 5 | `icon_tallow.png` | `a lump of solid off-white animal fat in a small plain brown clay dish` |
| 6 | `icon_oak_gall.png` | `three small round light brown oak galls, smooth woody balls, on a small oak twig with one green leaf` |
| 7 | `icon_ink.png` | `small round dark clay ink pot filled with black ink, a white goose feather quill standing in it` |
| 8 | `icon_malt.png` | `small heap of pale golden barley grains with tiny sprouts, on a plain cloth` |
| 9 | `icon_blank_book_v2.png` | `closed thin book bound in plain brown leather, tied with a thin cord, cream page edges visible on the side` |

## Doğrulama
`python tools/art/validate_iso.py <oyuncaklar>`, `python tools/art/style.py check --family esya <hepsi>`. Yüz, göz, figür içeren sonuç teslim edilmez (red klasörüne). Dönem: kil bilye, deri top, keten/kaz tüyü kalem; cam ve metal hokka yok.

## Notlar
- İkonların `Assets/Art/Icons/` yerine `_aday/` altına gitmesi 316/399 ile aynı (işçi yolu).
- Eşya → dosya eşlemesi kod oturumunun işidir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
