---
id: "731"
title: "Zanaat dükkânı eşyaları (9)"
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
  - Assets/Art/Iso/_aday/iso_item_quench_tub_se.png
  - Assets/Art/Iso/_aday/iso_item_iron_pile_se.png
  - Assets/Art/Iso/_aday/iso_item_smith_display_se.png
  - Assets/Art/Iso/_aday/iso_item_unfinished_furniture_se.png
  - Assets/Art/Iso/_aday/iso_item_pottery_display_se.png
  - Assets/Art/Iso/_aday/iso_item_garment_stand_se.png
  - Assets/Art/Iso/_aday/iso_item_yarn_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_shop_counter_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_clay_trough_se.png
created: 2026-11-20
---
# 731 Zanaat dükkânı eşyaları (9)

## Amaç
Unity'de yeni oyunun şehrine bakıldı (20.11.2026): bu eşyalar dükkânlarda, avlularda ve sokakta kahverengi kutu (yer tutucu) olarak çiziliyor; hiçbirinin görseli ya da işi yok. Demirci, marangoz, çömlekçi ve terzinin "ne dükkânı olduğu" bu eşyalardan anlaşılıyor; şu an kutu.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.quench_tub` | `iso_item_quench_tub_se.png` | 1x1 | 80x72 | 40, 24 | `half barrel quenching tub full of dark water, a pair of blacksmith tongs resting on the rim` |
| 2 | `item.iron_pile` | `iso_item_iron_pile_se.png` | 1x1 | 80x64 | 40, 24 | `small heap of iron bars, horseshoes and scrap iron on the floor` |
| 3 | `item.smith_display` | `iso_item_smith_display_se.png` | 1x1 | 80x96 | 40, 24 | `wooden display stand with horseshoes, sickles and iron tools hanging on pegs` |
| 4 | `item.unfinished_furniture` | `iso_item_unfinished_furniture_se.png` | 1x1 | 80x96 | 40, 24 | `half-built wooden cupboard without doors, raw pale wood, a few loose boards leaning against it` |
| 5 | `item.pottery_display` | `iso_item_pottery_display_se.png` | 1x1 | 80x96 | 40, 24 | `wooden stepped stand with glazed jugs, bowls and cups for sale` |
| 6 | `item.garment_stand` | `iso_item_garment_stand_se.png` | 1x1 | 64x104 | 32, 20 | `wooden garment stand on a cross base with a linen tunic and a wool cloak hanging` |
| 7 | `item.yarn_basket` | `iso_item_yarn_basket_se.png` | 1x1 | 80x64 | 40, 24 | `round wicker basket full of wool yarn balls and skeins in natural colours` |
| 8 | `item.shop_counter` | `iso_item_shop_counter_2x1_se_nw.png` | 2x1 | 112x88 | 56, 32 | `plain wooden shop counter with a hinged lid, a small set of balance scales on top` |
| 9 | `item.clay_trough` | `iso_item_clay_trough_se.png` | 1x1 | 80x64 | 40, 24 | `wooden trough of wet grey potter clay with a wooden paddle in it` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/731-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
