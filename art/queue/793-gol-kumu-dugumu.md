---
id: "793"
title: "Göl kumu toplama noktası (1)"
status: review
claimed_at: 2026-11-20T17:44Z
approved_by: user        # kullanıcı 21.11 00:51 "bunları yap": gölde kutu çizilen kum toplama noktası (399 bekliyor)
priority: 10
family: esya
style_gate: false
cost_est: 2
cost_max: 4
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_forage_sand_se.png
created: 2026-11-20
---
# 793 Göl kumu toplama noktası

## Amaç
Göl kıyısındaki kum toplama düğümü (`item.forage_sand`, cam zincirinin kumu) Unity'de kahverengi kutu çiziliyor; görseli 399'da ama o iş başka bağımlılıklar yüzünden bekliyor. Bu iş yalnızca bu görseli üretir; 399 aynı dosyayı bulursa üstüne yazmaz.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.forage_sand` | `iso_item_forage_sand_se.png` | 1x1 | 80x64 | 40, 24 | `small heap of clean pale lake sand with a wooden scoop stuck in it, a few pebbles` |

Az renk: açıklamaya `, few colors, flat muted earthy tones` eki; 48 rengi aşarsa `reduce_colors` (40) bir kez.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/793-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
