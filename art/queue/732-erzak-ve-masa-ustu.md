---
id: "732"
title: "Erzak çuvalları, sepetler ve masa üstü süsler (9)"
status: running
claimed_at: 2026-11-20T17:37Z
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
  - Assets/Art/Iso/_aday/iso_item_flour_sack_se.png
  - Assets/Art/Iso/_aday/iso_item_grain_sack_se.png
  - Assets/Art/Iso/_aday/iso_item_fish_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_wicker_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_ink_quill_se.png
  - Assets/Art/Iso/_aday/iso_item_wax_seal_se.png
  - Assets/Art/Iso/_aday/iso_item_letter_box_se.png
  - Assets/Art/Iso/_aday/iso_item_shears_chalk_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_chisel_rack_sw_ne.png
created: 2026-11-20
---
# 732 Erzak çuvalları, sepetler ve masa üstü süsler (9)

## Amaç
Unity'de yeni oyunun şehrine bakıldı (20.11.2026): bu eşyalar dükkânlarda, avlularda ve sokakta kahverengi kutu (yer tutucu) olarak çiziliyor; hiçbirinin görseli ya da işi yok. Fırın, değirmen, balıkçı ve yazıcının küçük eşyaları; masa üstü olanlar (hokka, mühür, mektup kutusu, makas) masa ve tezgâh yuvasında durur.
`items.json` eşlemesini kod oturumu ekler (`pending: true`, dosya adları aşağıdaki tabloyla aynı); dosya gelince oyun kendiliğinden çizer.

## Tarif
**492 ile aynı yöntem:** `create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Referanslar: ailenin `art/style/references.json` görselleri, ayrıca `iso_item_barrel_se`, `iso_item_workbench_2x1_se_nw`.
- 1x1: `_se`, 2x1: 492 ile aynı (`--n 2 --axis x`, `_2x1_se_nw`): uzun kenar ekranda **sağ-aşağıdan sol-yukarıya** uzanır (`iso_item_workbench_2x1_se_nw` gibi); ayna öbür ekseni verir.
- Ortak ek: `, medieval European, earthy muted medieval palette, desaturated, high top-down isometric view, light from top-left, single dark outline, transparent background`
- Palet ≤ 48 renk (`fit` sonrası). Yazı, harf, rakam yok (tabela ve panolar boş ya da resimli). Sol/sağ simetrik ya da yönsüz çizim: ayna geçerli olmalı.
- Dönem kuralı (CLAUDE.md): hayali ortaçağ yerleşimi; Yeni Dünya bitkisi ve ortaçağ sonrası icat yok.

| # | items.json id | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.flour_sack` | `iso_item_flour_sack_se.png` | 1x1 | 80x72 | 40, 24 | `tied hemp sack of flour, slightly dusted white, slumped on the floor` |
| 2 | `item.grain_sack` | `iso_item_grain_sack_se.png` | 1x1 | 80x72 | 40, 24 | `open hemp sack full of golden wheat grain, a wooden scoop in it` |
| 3 | `item.fish_basket` | `iso_item_fish_basket_se.png` | 1x1 | 80x64 | 40, 24 | `wicker creel basket with a few silver fish and a bit of seaweed` |
| 4 | `item.wicker_basket` | `iso_item_wicker_basket_se.png` | 1x1 | 80x64 | 40, 24 | `empty round wicker basket with two handles` |
| 5 | `item.ink_quill` | `iso_item_ink_quill_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `tiny clay inkpot with a goose feather quill standing in it` |
| 6 | `item.wax_seal` | `iso_item_wax_seal_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `tiny brass seal stamp beside a stick of red sealing wax` |
| 7 | `item.letter_box` | `iso_item_letter_box_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `tiny wooden box with folded parchment letters inside` |
| 8 | `item.shears_chalk` | `iso_item_shears_chalk_se.png` | 1x1 (masa üstü) | 32x32 | 16, 7 | `tiny iron tailor shears, a coiled measuring cord and a piece of chalk` |
| 9 | `item.chisel_rack` | `iso_wall_item_chisel_rack_sw_ne.png` | duvar | 48x64 | 24, 32 | `wall-mounted wooden rack holding chisels, a mallet and a hand plane, seen on a wall that runs from lower-left to upper-right` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ... --max-colors 48`, `python tools/art/style.py check --family esya <dosya> ...`. Yan yana sayfası `art/results/732-yanyana.png`: aynı dükkândaki komşu eşyalarla (fıçı, tezgâh, raf) aynı satırda; ölçek ve renk uyumu göze batmamalı.

## Notlar
- Teslimde `cp -n`, `.meta` yok; var olan PNG'nin üstüne yazılmaz.
- Teslim edilen eşyanın `pending` alanını kod oturumu kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
