---
id: "730"
title: "Sokak ve avlu eşyaları (9)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20: "bunların hepsini yap. ilk asseti iş dosyasına yaz o başlasın" (belge 64b kalanlar, Unity incelemesi: şehirde kahverengi kutular)
priority: 10
family: esya
style_gate: false
cost_est: 12
cost_max: 24
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_shop_sign_se.png
  - Assets/Art/Iso/_aday/iso_item_notice_post_se.png
  - Assets/Art/Iso/_aday/iso_item_hitching_post_se.png
  - Assets/Art/Iso/_aday/iso_item_clothes_line_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_cloth_drying_line_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_net_drying_rack_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_lumber_pile_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_stone_blocks_se.png
  - Assets/Art/Iso/_aday/iso_item_wash_tub_se.png
created: 2026-11-20
---
# 730 Sokak ve avlu eşyaları (9)

## Amaç
Unity'de yeni oyunun şehrine bakıldı (20.11.2026): bu eşyalar dükkânlarda, avlularda ve sokakta kahverengi kutu (yer tutucu) olarak çiziliyor; hiçbirinin görseli ya da işi yok. Sokakta ve dükkân avlularında en çok görünen kutular bunlar: 13 dükkân tabelası, ilan panosu, ip ve askılar.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.shop_sign` | `iso_item_shop_sign_se.png` | 1x1 | 64x112 | 32, 16 | `tall wooden post with an iron bracket and a blank painted wooden hanging sign board, no lettering` |
| 2 | `item.notice_post` | `iso_item_notice_post_se.png` | 1x1 | 64x112 | 32, 16 | `wooden notice board on two posts with a small shingled roof, parchment sheets pinned on it, no readable text` |
| 3 | `item.hitching_post` | `iso_item_hitching_post_se.png` | 1x1 | 80x72 | 40, 24 | `wooden hitching rail on two short posts with an iron ring and a rope` |
| 4 | `item.clothes_line` | `iso_item_clothes_line_2x1_se_nw.png` | 2x1 | 112x96 | 56, 32 | `rope clothes line strung between two wooden poles with linen shirts and sheets hanging` |
| 5 | `item.cloth_drying_line` | `iso_item_cloth_drying_line_2x1_se_nw.png` | 2x1 | 112x96 | 56, 32 | `two wooden poles with a line of freshly dyed wool cloth lengths hanging, madder red, woad blue and weld yellow` |
| 6 | `item.net_drying_rack` | `iso_item_net_drying_rack_2x1_se_nw.png` | 2x1 | 112x96 | 56, 32 | `wooden frame with fishing nets hung up to dry, a few cork floats` |
| 7 | `item.lumber_pile` | `iso_item_lumber_pile_2x1_se_nw.png` | 2x1 | 112x80 | 56, 32 | `neat stack of sawn oak planks and beams on wooden sleepers` |
| 8 | `item.stone_blocks` | `iso_item_stone_blocks_se.png` | 1x1 | 80x80 | 40, 24 | `stack of rough-cut grey limestone building blocks, stone chips on the ground` |
| 9 | `item.wash_tub` | `iso_item_wash_tub_se.png` | 1x1 | 80x64 | 40, 24 | `wide wooden wash tub with iron hoops, soapy water and a washboard leaning on it` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/730-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
