---
id: "750"
title: "Gönül yolu eşyaları: mendil, nişan yüzükleri, kurdele, kına tası, davul, zurna, testi, kahve tepsisi (11)"
status: running
claimed_at: 2026-11-21T00:42Z
approved_by: user        # kullanıcı 20.11.2026; Project belgeleri 67b §3, 67c
priority: 40
family: esya
style_gate: false
cost_est: 22
cost_max: 40
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_handkerchief_red.png
  - Assets/Art/Iso/_aday/icon_item_handkerchief_blue.png
  - Assets/Art/Iso/_aday/icon_item_handkerchief_white.png
  - Assets/Art/Iso/_aday/icon_item_ring_silver.png
  - Assets/Art/Iso/_aday/icon_item_ring_gold.png
  - Assets/Art/Iso/_aday/icon_item_ribbon_red.png
  - Assets/Art/Iso/_aday/iso_item_henna_bowl_se.png
  - Assets/Art/Iso/_aday/iso_item_davul_se.png
  - Assets/Art/Iso/_aday/iso_item_zurna_se.png
  - Assets/Art/Iso/_aday/iso_item_clay_jug_small_se.png
  - Assets/Art/Iso/_aday/iso_item_coffee_tray_se.png
created: 2026-11-20
---
# 750 Gönül yolu eşyaları (11)

## Amaç
Sevgili olmada mendil, nişanda yüzük ve kurdele, kına gecesinde kına tası, düğünde davul ve zurna, eşikte kırılan testi, istemede kahve tepsisi (Project belgesi 67b §3). `items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları buradaki gibi).

## Tarif
**İkonlar (envanter, 1-6):** 591t ile aynı yöntem ve boyut (arayüz/envanter ikonu, saydam zemin).

| Dosya | Eşya | Açıklama metni |
|---|---|---|
| `icon_item_handkerchief_red.png` | İşlemeli mendil (kırmızı) | `small folded linen handkerchief with red cross-stitch flower embroidery on one corner and a lace edge` |
| `icon_item_handkerchief_blue.png` | İşlemeli mendil (mavi) | `small folded linen handkerchief with blue cross-stitch vine embroidery and a lace edge` |
| `icon_item_handkerchief_white.png` | İşlemeli mendil (beyaz) | `small folded white linen handkerchief with white-on-white embroidery of a small bird` |
| `icon_item_ring_silver.png` | Nişan yüzüğü (gümüş) | `plain thin silver ring, a pair tied together with a short red ribbon` |
| `icon_item_ring_gold.png` | Nişan yüzüğü (altın) | `plain thin gold ring, a pair tied together with a short red ribbon` |
| `icon_item_ribbon_red.png` | Nişan kurdelesi | `short red satin ribbon with a small bow` |

**Dünya eşyaları (7-11):** 732 ile aynı yöntem (`create_map_object`, stil eşleme, kılavuz + maske, `high top-down`, `low detail`, `basic shading`, `single color outline`, 732'deki ortak ek ve palet kuralı).

| # | Dosya | Ayak izi | Tuval | Pivot | Açıklama metni |
|---|---|---|---|---|---|
| 7 | `iso_item_henna_bowl_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `tiny copper bowl of dark orange henna paste with a small candle stuck in it` |
| 8 | `iso_item_davul_se.png` | 1x1 | 64x64 | 32, 20 | `large wooden folk drum with rope tension lacing and a curved beater resting on top` |
| 9 | `iso_item_zurna_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `small wooden folk shawm with a flared bell lying on its side` |
| 10 | `iso_item_clay_jug_small_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `small unglazed terracotta water jug with one handle` |
| 11 | `iso_item_coffee_tray_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `small round copper tray with two tiny coffee cups and a small copper coffee pot` |

Dönem: kahve 32'deki onaylı istisna; davul ve zurna dönemle uyumlu halk çalgıları.

## Doğrulama
İkonlar: `python tools/art/style.py check --family arayuz <dosya>`. Dünya eşyaları: `python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`; masa üstü eşyalar masa yuvasına sığar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
