---
id: "736"
title: "730-734 teslim edilmeyenler: erzak (2)"
status: review
claimed_at: 2026-11-20T17:44Z
approved_by: user        # kullanıcı 20.11 21:46 "dekorasyonları düzelt": 730-734'te teslim edilmeyenler yeniden
priority: 10
family: esya
style_gate: false
cost_est: 3
cost_max: 6
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_grain_sack_se.png
  - Assets/Art/Iso/_aday/iso_item_wicker_basket_se.png
created: 2026-11-20
---
# 736 730-734 teslim edilmeyenler: erzak (2)

## Amaç
732'de renk sayısı ve palet kilidi yüzünden teslim edilmeyen eşyalar; Unity'de hâlâ kutu çiziliyorlar.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.grain_sack` | `iso_item_grain_sack_se.png` | 1x1 | 80x72 | 40, 24 | `open hemp sack full of wheat grain, a wooden scoop in it` |
| 2 | `item.wicker_basket` | `iso_item_wicker_basket_se.png` | 1x1 | 80x64 | 40, 24 | `empty round wicker basket with two handles` |

**Bu yeniden denemenin farkı (730-734 raporları):** çoğu çıktı renk sayısından (49-63 > 48) ve palet/doygunluk kilidinden kaldı.
- Açıklamaya ek: `, simple shapes, few colors, flat muted earthy tones, no bright colors` (boyalı kumaş satırında renkler yine soluk: kök boya kırmızısı, çivit, muhabbet sarısı, hepsi gri katkılı).
- Renk sayısı 48'i aşarsa PixelLab `reduce_colors` (40 renk) bir kez uygulanır (011'deki gibi; 1 çağrı), sonra doğrulama tekrarlanır. Stil kilidi yine bağlayıcı: KALDI teslim edilmez.
- Kılavuz elmasını oda/kutu olarak çizdiren eşyalarda (demir yığını) kılavuzsuz basit mod denenir.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/736-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
