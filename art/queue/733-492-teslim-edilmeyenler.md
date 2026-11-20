---
id: "733"
title: "492'de teslim edilmeyen yer eşyaları (7)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20: "bunların hepsini yap. ilk asseti iş dosyasına yaz o başlasın" (belge 64b kalanlar, Unity incelemesi: şehirde kahverengi kutular)
priority: 10
family: esya
style_gate: false
cost_est: 9
cost_max: 18
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_sini_table_se.png
  - Assets/Art/Iso/_aday/iso_item_copper_pots_se.png
  - Assets/Art/Iso/_aday/iso_item_barrel_table_se.png
  - Assets/Art/Iso/_aday/iso_item_water_trough_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_bed_frame_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_drying_rack_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_rope_coil_se.png
created: 2026-11-20
---
# 733 492'de teslim edilmeyen yer eşyaları (7)

## Amaç
Unity'de yeni oyunun şehrine bakıldı (20.11.2026): bu eşyalar dükkânlarda, avlularda ve sokakta kahverengi kutu (yer tutucu) olarak çiziliyor; hiçbirinin görseli ya da işi yok. 492 `review` durumunda ama bu yedi dosya teslim edilmedi (önceki kalma nedeni palet ve doygunluk). Aynı tarif, aynı dosya adları.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.sini_table` | `iso_item_sini_table_se.png` | 1x1 | 80x64 | 40, 24 | `low round wooden tray table on short folding legs, copper tray top` |
| 2 | `item.copper_pots` | `iso_item_copper_pots_se.png` | 1x1 | 80x72 | 40, 24 | `stack of three hammered copper cauldrons of different sizes on the floor` |
| 3 | `item.barrel_table` | `iso_item_barrel_table_se.png` | 1x1 | 80x80 | 40, 24 | `upright barrel used as a small tavern table, a clay mug on top` |
| 4 | `item.water_trough` | `iso_item_water_trough_2x1_se_nw.png` | 2x1 | 112x80 | 56, 32 | `long hollowed log water trough for animals, water inside` |
| 5 | `item.bed_frame` | `iso_item_bed_frame_2x1_se_nw.png` | 2x1 | 112x88 | 56, 32 | `plain wooden bed frame with a straw mattress and a wool blanket` |
| 6 | `item.drying_rack` | `iso_item_drying_rack_2x1_se_nw.png` | 2x1 | 112x96 | 56, 32 | `wide wooden drying rack with herbs and strips of dried meat hanging` |
| 7 | `item.rope_coil` | `iso_item_rope_coil_se.png` | 1x1 | 80x56 | 40, 24 | `coil of thick hemp rope on the ground, beside a fishing pier` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/733-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
