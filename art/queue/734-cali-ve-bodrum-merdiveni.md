---
id: "734"
title: "Çalı ve ahşap bodrum merdiveni (2)"
status: running
claimed_at: 2026-11-20T17:44Z
approved_by: user        # kullanıcı kararı 2026-11-20: "bunların hepsini yap. ilk asseti iş dosyasına yaz o başlasın" (belge 64b kalanlar, Unity incelemesi: şehirde kahverengi kutular)
priority: 10
family: esya
style_gate: false
cost_est: 4
cost_max: 8
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_shrub_v2_se.png
  - Assets/Art/Iso/_aday/iso_item_stairs_down_wood_1x2_se.png
created: 2026-11-20
---
# 734 Çalı ve ahşap bodrum merdiveni (2)

## Amaç
Unity'de yeni oyuna bakıldı (20.11.2026): yol ve şehir kenarındaki çalılar ile evlerdeki ahşap bodrum merdivenleri kutu çiziliyor. Var olan `iso_item_shrub_se` 32x32 (masa üstü boyu), dünyada görünmüyor; ahşap bodrum merdiveninin (`item.stairs_down`) hiç görseli yok, taş olanın (`iso_item_stairs_down_stone_1x2_se`) var.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.shrub` | `iso_item_shrub_v2_se.png` | 1x1 | 80x72 | 40, 24 | `low round green leafy shrub, dense hawthorn-like bush, no flowers, no fruit` |
| 2 | `item.stairs_down` | `iso_item_stairs_down_wood_1x2_se.png` | 1x2 | 112x96 | 56, 32 | `wooden steps going down into a dark square cellar opening in the floor, plank frame around the hole, a rope handrail` |

- Merdiven: `iso_item_stairs_down_stone_1x2_se` ile aynı ölçek ve yön (referans olarak ver): basamaklar ekranda sol-yukarıdan sağ-aşağıya iner, uzun kenar sağ-aşağı → sol-yukarı (ızgara y). Taş olanın ahşap kardeşi gibi görünmeli.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/734-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
