---
id: "360"
title: Kazı kaya karoları (alçak kesik blok, 8 tür)
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 28 (18.11.2026, "tek seferde yapalım")
priority: 10
family: zemin
style_gate: false
cost_est: 12
cost_max: 18
depends_on: ["002"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_rock_soil_a.png
  - Assets/Art/Iso/iso_rock_soft_a.png
  - Assets/Art/Iso/iso_rock_stone_a.png
  - Assets/Art/Iso/iso_rock_hard_a.png
  - Assets/Art/Iso/iso_rock_bedrock_a.png
  - Assets/Art/Iso/iso_rock_rubble_a.png
  - Assets/Art/Iso/iso_rock_cracked_a.png
  - Assets/Art/Iso/iso_rock_dug_floor_a.png
created: 2026-11-18
---
# 360 Kazı kaya karoları (alçak kesik blok, 8 tür)

## Amaç
Arsa kazısı (kat -2) ve ocakta kazılmamış karolar (Core.Mining.RockKind). Kod bugün düz renkli elmas çiziyor (`WorldView.RockTint`). 23. belge 13: izometrikte tam boy blok karakteri örtmesin diye kaya **alçak kesik blok** çizilir (yapı modundaki duvar kesiti gibi), üst yüzü dokulu.

## Tarif
Yöntem, tuval ve doğrulama **002 zemin karolarıyla aynı** (`art/queue/002*.md` > Tarif), tek fark: karo 64x32 elmasın üstünde 10 piksel yükseklikte kesik blok (üst yüz + iki ön yan). Renkler toprak altı, kasvetli ve soluk; zemin ailesinin paleti.
| Dosya | Tür | Açıklama metni |
|---|---|---|
| `iso_rock_soil_a.png` | toprak | `low cut block of packed brown earth with small pebbles and roots, isometric tile` |
| `iso_rock_soft_a.png` | yumuşak taş | `low cut block of crumbly grey-brown soft rock, layered` |
| `iso_rock_stone_a.png` | taş | `low cut block of grey stone, chisel marks on top` |
| `iso_rock_hard_a.png` | sert taş | `low cut block of dark dense hard rock, few cracks` |
| `iso_rock_bedrock_a.png` | ana kaya | `low cut block of very dark bedrock, smooth and massive` |
| `iso_rock_rubble_a.png` | moloz | `low pile of broken rock rubble and dust filling the tile` |
| `iso_rock_cracked_a.png` | çatlak (ateşle kırılmış) | `low cut block of dark rock with glowing-free soot stains and many fire cracks` |
| `iso_rock_dug_floor_a.png` | kazılmış zemin | `flat isometric floor tile of dug earth with pick marks, slightly damp` |

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`. Kesik blokların üst yüzü elmasla birebir hizalı; yan yüz yüksekliği 10±2 px.

## Notlar
Kod eşlemesi (IsoArt, RockTint yerine sprite) kod oturumunun işi. Damar karoları 361.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
