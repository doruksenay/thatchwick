---
id: "432"
title: "Yerleşim: dış mekân, bahçe ve bitki örtüsü (13 eşya)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 ("hepsinin üretimini başlat")
family: esya
style_gate: false
cost_est: 14
cost_max: 23
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_shop_sign_se.png
  - Assets/Art/Iso/_aday/iso_item_signpost_se.png
  - Assets/Art/Iso/_aday/iso_item_notice_post_se.png
  - Assets/Art/Iso/_aday/iso_item_clothes_line_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_hitching_post_se.png
  - Assets/Art/Iso/_aday/iso_item_cloth_drying_line_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_net_drying_rack_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_pottery_display_se.png
  - Assets/Art/Iso/_aday/iso_item_smith_display_se.png
  - Assets/Art/Iso/_aday/iso_item_flower_clump_se.png
  - Assets/Art/Iso/_aday/iso_item_shrub_se.png
  - Assets/Art/Iso/_aday/iso_item_reeds_se.png
  - Assets/Art/Iso/_aday/iso_item_grass_tuft_se.png
created: 2026-11-19
---
# 432 Yerleşim: dış mekân, bahçe ve bitki örtüsü

## Amaç
Belge 44 Aşama D: tabela, yol tabelası, ilan tahtası, çamaşır ipi, at bağlama direği, meslek avlusu eşyaları (boyalı kumaş,
ağ kurutma, çömlek ve demirci sergisi) ve doğal bitki örtüsü (çiçek öbeği, çalı, saz, çim öbeği). Hepsi dış mekân.

## Tarif
301 yöntemi; çiçek, çalı, saz ve çim öbeği 371 toplama düğümü ölçeğinde (çim öbeği yere yassı, gölgesiz; yürünebilir halı katmanı).

| # | Dosya | Ayak izi | Açıklama |
|---|---|---|---|
| 1 | `iso_item_shop_sign_se.png` | 1x1 | `wooden post with a hanging painted shop sign board on an iron bracket, blank symbol` |
| 2 | `iso_item_signpost_se.png` | 1x1 | `rustic wooden crossroads signpost with two arrow boards, no readable text` |
| 3 | `iso_item_notice_post_se.png` | 1x1 | `freestanding wooden notice board on two posts with pinned parchment notes and a small roof` |
| 4 | `iso_item_clothes_line_2x1_se.png` | 2x1 | `rope clothesline between two wooden posts with linen sheets and a shirt hanging` |
| 5 | `iso_item_hitching_post_se.png` | 1x1 | `wooden horse hitching rail with an iron ring` |
| 6 | `iso_item_cloth_drying_line_2x1_se.png` | 2x1 | `wooden poles with long lengths of freshly dyed red, blue and yellow wool cloth drying` |
| 7 | `iso_item_net_drying_rack_2x1_se.png` | 2x1 | `wooden frame with a fishing net hung out to dry, cork floats` |
| 8 | `iso_item_pottery_display_se.png` | 1x1 | `low wooden bench with clay jugs, bowls and pots for sale` |
| 9 | `iso_item_smith_display_se.png` | 1x1 | `small wooden rack with horseshoes, sickles and iron tools for sale` |
| 10 | `iso_item_flower_clump_se.png` | 1x1 | `small clump of wild meadow flowers, poppies, cornflowers and daisies` |
| 11 | `iso_item_shrub_se.png` | 1x1 | `small round green bush` |
| 12 | `iso_item_reeds_se.png` | 1x1 | `clump of tall green riverbank reeds and rushes` |
| 13 | `iso_item_grass_tuft_se.png` | 1x1 | `low tuft of wild grass, flat on the ground` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`.
Ayak izi kılavuzu: 1x1 80 px tuval pivot (40, 24); 2x1 112 px pivot (56, 32); dekor 32 px pivot (16, 7).

## Notlar
- Yön: tek görsel `_se` (ekranda sağ-aşağı = mantıksal N, `docs/gorsel-yonerge.md` "Yön kuralı"). Çok karolu eşyada uzun eksen "\" (se-nw).
  Eşya sol/sağ simetrik çizilsin (ayna W yönünü verir).
- Kod eşlemesi görseller gelince: `items.json` → `art` (`python tools/layout/item_meta.py` eşlenmemiş görselleri bulur).
- Dönem: ortaçağ; makine, cam vitrin, matbaa yok. Her açıklamanın sonuna `, no face, no eyes, no creature, no person, no text`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
