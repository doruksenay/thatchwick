---
id: "735"
title: "730-734 teslim edilmeyenler: zanaat ve avlu (6)"
status: review
claimed_at: 2026-11-20T17:44Z
approved_by: user        # kullanıcı 20.11 21:46 "dekorasyonları düzelt": 730-734'te teslim edilmeyenler yeniden
priority: 10
family: esya
style_gate: false
cost_est: 8
cost_max: 16
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_quench_tub_se.png
  - Assets/Art/Iso/_aday/iso_item_iron_pile_se.png
  - Assets/Art/Iso/_aday/iso_item_garment_stand_se.png
  - Assets/Art/Iso/_aday/iso_item_yarn_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_cloth_drying_line_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_shrub_v2_se.png
created: 2026-11-20
---
# 735 730-734 teslim edilmeyenler: zanaat ve avlu (6)

## Amaç
730, 731 ve 734'te renk sayısı ve palet kilidi yüzünden teslim edilmeyen eşyalar; Unity'de hâlâ kutu çiziliyorlar.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.quench_tub` | `iso_item_quench_tub_se.png` | 1x1 | 80x72 | 40, 24 | `half barrel quenching tub full of dark water, blacksmith tongs resting on the rim` |
| 2 | `item.iron_pile` | `iso_item_iron_pile_se.png` | 1x1 | 80x64 | 40, 24 | `small loose heap of iron bars and horseshoes lying directly on the ground, no box, no walls` |
| 3 | `item.garment_stand` | `iso_item_garment_stand_se.png` | 1x1 | 64x104 | 32, 20 | `wooden garment stand on a cross base with a plain linen tunic and a brown wool cloak hanging` |
| 4 | `item.yarn_basket` | `iso_item_yarn_basket_se.png` | 1x1 | 80x64 | 40, 24 | `round wicker basket holding a few balls of undyed cream and brown wool yarn` |
| 5 | `item.cloth_drying_line` | `iso_item_cloth_drying_line_2x1_se_nw.png` | 2x1 | 112x96 | 56, 32 | `two wooden poles with a line of hanging dyed wool cloth lengths in muted dull red, dull blue and dull yellow` |
| 6 | `item.shrub` | `iso_item_shrub_v2_se.png` | 1x1 | 80x72 | 40, 24 | `low round green leafy shrub, dense bush, no flowers, no fruit` |

**Bu yeniden denemenin farkı (730-734 raporları):** çoğu çıktı renk sayısından (49-63 > 48) ve palet/doygunluk kilidinden kaldı.
- Açıklamaya ek: `, simple shapes, few colors, flat muted earthy tones, no bright colors` (boyalı kumaş satırında renkler yine soluk: kök boya kırmızısı, çivit, muhabbet sarısı, hepsi gri katkılı).
- Renk sayısı 48'i aşarsa PixelLab `reduce_colors` (40 renk) bir kez uygulanır (011'deki gibi; 1 çağrı), sonra doğrulama tekrarlanır. Stil kilidi yine bağlayıcı: KALDI teslim edilmez.
- Kılavuz elmasını oda/kutu olarak çizdiren eşyalarda (demir yığını) kılavuzsuz basit mod denenir.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/735-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
