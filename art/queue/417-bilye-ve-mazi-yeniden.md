---
id: "417"
title: Bilye (açık, çok renkli bilyeler) ve mazı (kahverengi, ahşap) yeniden
status: accepted
claimed_at: 2026-11-18T19:35Z
approved_by: user        # kullanıcı 18.11.2026: mazı "kahverengi ve ahşap" vurgusuyla; bilyede kese aynı, bilyeler açık ve farklı renkli
priority: 15
family: esya
style_gate: false
cost_est: 2
cost_max: 4
depends_on: ["406"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_marbles_se_v4.png
  - Assets/Art/Iso/_aday/icon_oak_gall_v3.png
created: 2026-11-18
---
# 417 Bilye ve mazı yeniden

## Amaç
406'dan (sonuç `art/results/406-...md`) iki parça. Yöntem 406 ile aynı (`create_map_object` 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline`, sona `, simple still-life object, no face, no eyes, no creature, no person, plain background`). Eski dosyaların üzerine yazılmaz.

## Tarif
### 1. Bilye: `iso_item_marbles_se_v4.png` (yuva K)
- **Kese aynı kalsın:** `Assets/Art/Iso/_aday/iso_item_marbles_se_v3.png` kesesi beğenildi. Araç destekliyorsa v3'ü referans/başlangıç görseli (init/inpaint) olarak ver ve yalnızca bilyeleri değiştir; desteklemiyorsa aynı kese tarifini koru.
- **Bilyeler:** daha açık renkli ve her biri farklı renk; kesenin ağzında ve önünde 3-4 bilye seçilsin.
- Açıklama: `small open drawstring pouch of warm brown leather, filled with round marbles visible in the opening, three marbles resting in front of it, each marble a different light color: cream, pale ochre yellow, soft sage green, light terracotta, marbles clearly lighter than the pouch`
- Dönem: kil ve taş bilye (açık krem, okra, adaçayı yeşili, açık kiremit); cam bilye ve parlak doygun renk yok.

### 2. Mazı: `icon_oak_gall_v3.png` (ikon)
- 406'daki v2 mor-bordo çıktı, erik gibi. **Kahverengi ve ahşap** görünmeli.
- Açıklama: `three small hard round oak galls, woody brown like dry wood or a hazelnut shell, light tan and medium brown, rough matte bark-like texture with tiny bumps, on a thin brown oak twig, not fruit, not berry, no purple, no red`
- Renk: açık ten-kahve ile orta kahve, mor/bordo/pembe yok. `style.py check` sonucu uyarıysa `style.py fit`.

## Doğrulama
`python tools/art/validate_iso.py <bilye>`, `python tools/art/style.py check --family esya <ikisi>`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
Karar (kullanıcı, 18.11.2026): accepted. Bilye v4 ve mazı v3 kabul; oyuncak/ikon seti (399, 401, 406, 417) tamam.
