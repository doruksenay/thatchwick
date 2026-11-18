---
id: "385"
title: Tavan ve kiriş: kiriş kancası, asma fener, demir avize, ot demetleri, çelenk, püskül (6)
status: review
claimed_at: 2026-11-18T12:56Z
approved_by: user        # otomatik akış (art/budget.md)
priority: 10
family: esya
style_gate: false
cost_est: 8
cost_max: 12
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_ceiling_item_beam_hook.png
  - Assets/Art/Iso/_aday/iso_ceiling_item_hanging_lantern.png
  - Assets/Art/Iso/_aday/iso_ceiling_item_iron_chandelier.png
  - Assets/Art/Iso/_aday/iso_ceiling_item_ceiling_herbs.png
  - Assets/Art/Iso/_aday/iso_ceiling_item_ceiling_garland.png
  - Assets/Art/Iso/_aday/iso_ceiling_item_ceiling_tassels.png
created: 2026-11-18
---
# 385 Tavan ve kiriş: kiriş kancası, asma fener, demir avize, ot demetleri, çelenk, püskül (6)

## Amaç
Ev dekorasyonu D1 (Project belgesi `27-ev-dekorasyonu`, kararlar 18.11.2026). Eşyalar kodda ve `items.json`'da var, şimdilik yer tutucu küp. Yuva türü ve boyutu, duvar bandı ve tavan katmanı kodda; görsel yalnızca eşyanın kendisi.

## Tarif
Yöntem, ortak ayarlar, ayak izi kılavuzu, ikinci deneme kuralı, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif).
- Takım renkleri 301/306'daki gibi (Köylü: kaba açık çam, kil, ip; Zanaatkâr: koyu meşe, demir; Tüccar: boyalı ahşap, pirinç, kalay; Asil: ceviz, dövme demir).
- Yeni katman (27 §7.10): eşya karonun ortasında, yerden ~2,5 birim yukarıda çizilir; görselin **üst kenarı** asılma noktasıdır (pivot üst-orta). Yön yok (ayna gerekmez).
- **Işık kaynağı kuralı:** alevli ya da yanan ışık içeren görsellerde `style.py check --light` (TEMPLATE).

### Eşyalar
| # | items.json id (takım) | Dosya (`_aday/`) | Yer | Tuval | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|
| 1 | `item.beam_hook` (Köylü) | `iso_ceiling_item_beam_hook.png` | tavan | 16x24 | `iron hook hanging from a ceiling beam by a short chain` |
| 2 | `item.hanging_lantern` (Zanaatkâr) | `iso_ceiling_item_hanging_lantern.png` | tavan | 24x40 | `iron lantern with horn panes hanging on a chain, glowing` |
| 3 | `item.iron_chandelier` (Asil) | `iso_ceiling_item_iron_chandelier.png` | tavan | 64x40 | `medieval wrought iron ring chandelier with lit candles, hanging on chains` |
| 4 | `item.ceiling_herbs` (Köylü) | `iso_ceiling_item_ceiling_herbs.png` | tavan | 40x32 | `several bundles of dried herbs hanging from a beam` |
| 5 | `item.ceiling_garland` (Köylü) | `iso_ceiling_item_ceiling_garland.png` | tavan | 48x24 | `garland of dried flowers draped from a beam` |
| 6 | `item.ceiling_tassels` (Zanaatkâr) | `iso_ceiling_item_ceiling_tassels.png` | tavan | 32x40 | `colorful wool tassels and a woven charm hanging from the ceiling` |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...` (ışıklılarda `--light`). Takım içi renk uyumu yan yana önizlemede. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

## Notlar
- Eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir; pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
