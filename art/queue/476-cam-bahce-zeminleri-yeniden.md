---
id: "476"
title: "Cam bahçe iç zeminleri: kum, tuzlu su, tatlı su (3)"
status: failed
claimed_at: 2026-11-20T07:44Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (cam bahçe: balık), Project belgesi 47
priority: 20
family: zemin
style_gate: false
cost_est: 3
cost_max: 6
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/viv_bg_dry.png
  - Assets/Art/Iso/_aday/viv_bg_salt.png
  - Assets/Art/Iso/_aday/viv_bg_fresh.png
created: 2026-11-20
---
# 476 Cam bahçe iç zeminleri: kum, tuzlu su, tatlı su (3)

## Amaç
Belge 47: yakın görünümün arka planı. 471'de kum ve tuzlu su palet nedeniyle teslim edilmedi; tatlı su zemini (balıklar) yeni.

## Tarif
`create_image_pixflux`, 160x96, `view=side`, `no_background=false`, çerçevesiz, lamba ya da bitki yok. Zemin şeridi alttan %20 (suda %14); su yüzeyi üstten %12'de. Ön planda hiçbir nesne olmasın (süsleri ve canlıları kod koyar). Kabul edilmiş `viv_bg_moist` ile aynı ışık. Aile `zemin`: kum ve çakıl tonları oradan.

| # | Dosya | Boyut | Açıklama metni |
|---|---|---|---|
| 1 | `viv_bg_dry.png` | 160x96 | `inside view of a glass case: pale sand floor with small stones, empty, no plants, no animals, no frame` |
| 2 | `viv_bg_salt.png` | 160x96 | `inside view of a glass case filled with clear sea water: sand floor, light rays, empty, no plants, no animals, no frame` |
| 3 | `viv_bg_fresh.png` | 160x96 | `inside view of a glass case filled with clear green pond water: muddy gravel floor, light rays, empty, no plants, no animals, no frame` |

## Doğrulama
`python tools/art/style.py check --family zemin <dosya> ...`, yan yana `art/results/476-yanyana.png` (üçü ve viv_bg_moist).

## Notlar
Kod `viv_bg_dry`, `viv_bg_salt`, `viv_bg_fresh` adlarını arar; yoksa düz renk çizer.

## Sorular
Üç çıktı da bitki ve çerçeve içeriyor (rapor: art/results/476-cam-bahce-zeminleri.md). Öneri: `viv_bg_moist` init_image (strength ~150) ile yalnızca zemin/su rengini değiştir, ya da "glass case" ifadesini at.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
