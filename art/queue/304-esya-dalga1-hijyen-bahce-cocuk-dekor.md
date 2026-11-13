---
id: "304"
title: Dalga 1: hijyen, bahçe, çocuk ve yüzey dekoru (15 eşya)
status: pending
approved_by: pending     # Dalga 1 planı (09 belgesi, 2026-11-08); kullanıcı "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 15
cost_max: 25
depends_on: ["301"]   # yöntem 301 ile aynı; 2026-11-13: öneri bağımlılığa çevrildi (301 stil pilotu kabul edilmeden başlamaz)
max_attempts: 2          # parça başına en çok 2 deneme
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_basin_ewer_se.png
  - Assets/Art/Iso/_aday/iso_item_water_bucket_se.png
  - Assets/Art/Iso/_aday/iso_item_barrel_tub_se.png
  - Assets/Art/Iso/_aday/iso_item_outhouse_se.png
  - Assets/Art/Iso/_aday/iso_item_well_se.png
  - Assets/Art/Iso/_aday/iso_item_scarecrow_se.png
  - Assets/Art/Iso/_aday/iso_item_garden_bench_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_toy_horse_se.png
  - Assets/Art/Iso/_aday/iso_item_child_chair_se.png
  - Assets/Art/Iso/_aday/iso_item_toy_chest_se.png
  - Assets/Art/Iso/_aday/iso_item_bread_bowl_se.png
  - Assets/Art/Iso/_aday/iso_item_jug_se.png
  - Assets/Art/Iso/_aday/iso_item_morris_board_se.png
  - Assets/Art/Iso/_aday/iso_item_spinning_top_se.png
  - Assets/Art/Iso/_aday/iso_item_story_book_se.png
created: 2026-11-08
---
# 304 Dalga 1: hijyen, bahçe, çocuk ve yüzey dekoru (15 eşya)

## Amaç
09 belgesindeki Dalga 1 eşyalarından hijyen (keyif ve hijyen ihtiyaçları 1.0'a girdi), bahçe, çocuk ve masa üstü dekor. Değirmen oyunu ve leğen yeni oyunun başlangıç eşyalarıdır, önce bunlar.

## Tarif
Yöntem, ortak ayarlar, ortak açıklama eki, ikinci deneme kuralı, renk sınırı (48), yön ölçümü, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme biçimi **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif). Farklar:
- Masaüstü klasörü `C:\Users\User\OneDrive\Desktop\art\esya\<yyyy-mm-dd>_<ad>\`, toplu önizleme `<yyyy-mm-dd>_{id}_toplu_x4.png`.
- Renk ve ölçek referansı: 300'de kabul edilen masa, sandalye, tabure (301 Notlar). Takım (stil seti) renkleri:
  - **Köylü:** kaba açık-orta kahve çam, saman, ip bağ, kil. Süssüz.
  - **Zanaatkâr:** koyu meşe, demir kulp ve çivi, yünlü örtü (soluk kırmızı ya da hardal). Düzgün işçilik.
- Açıklama metnine takımın tarifi eklenir: Köylü için `rough light pine wood, rope lashing`, Zanaatkâr için `dark oak wood with iron fittings, neat craftsmanship`.

### Eşyalar
| # | items.json id | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|---|---|
| 1 | `item.basin_ewer` (1x1, Köylü) | `iso_item_basin_ewer_se.png` | 1x1 | 80x72 | `--n 1` | 30 | `wooden washstand with a clay washing basin and a clay water jug` |
| 2 | `item.water_bucket` (1x1, Köylü) | `iso_item_water_bucket_se.png` | 1x1 | 80x56 | `--n 1` | 16 | `wooden water bucket with a rope handle, water inside` |
| 3 | `item.barrel_tub` (1x1, Zanaatkâr) | `iso_item_barrel_tub_se.png` | 1x1 | 80x72 | `--n 1` | 28 | `bathtub made from a large cut wooden barrel, iron hoops, water inside, linen cloth over the rim` |
| 4 | `item.outhouse` (1x1, Köylü, dış) | `iso_item_outhouse_se.png` | 1x1 | 80x120 | `--n 1` | 76 | `tiny wooden outhouse with a plank door and a small roof` |
| 5 | `item.well` (1x1, Köylü, dış) | `iso_item_well_se.png` | 1x1 | 80x112 | `--n 1` | 68 | `round fieldstone village well with a wooden roof frame, rope and bucket` |
| 6 | `item.scarecrow` (1x1, Köylü, dış) | `iso_item_scarecrow_se.png` | 1x1 | 80x112 | `--n 1` | 70 | `straw scarecrow on a wooden pole with a ragged tunic and a straw hat` |
| 7 | `item.garden_bench` (2x1, Zanaatkâr, dış) | `iso_item_garden_bench_2x1_se_nw.png` | 2x1 | 112x88 | `--n 2 --axis x` | 34 | `oak garden bench with a backrest` |
| 8 | `item.toy_horse` (1x1, Köylü) | `iso_item_toy_horse_se.png` | 1x1 | 80x72 | `--n 1` | 28 | `small wooden rocking horse toy for children` |
| 9 | `item.child_chair` (1x1, Köylü) | `iso_item_child_chair_se.png` | 1x1 | 80x64 | `--n 1` | 24 | `small low wooden child chair` |
| 10 | `item.toy_chest` (1x1, Zanaatkâr) | `iso_item_toy_chest_se.png` | 1x1 | 80x72 | `--n 1` | 26 | `small painted wooden toy chest with the lid open, a wooden ball and a doll visible` |
| 11 | `item.bread_bowl` (yüzey) | `iso_item_bread_bowl_se.png` | dekor | 32x32 | dekor (301) | 12 | `small wooden bowl with a round loaf of bread` |
| 12 | `item.jug` (yüzey) | `iso_item_jug_se.png` | dekor | 32x32 | dekor (301) | 14 | `small clay water jug with a handle` |
| 13 | `item.morris_board` (yüzey) | `iso_item_morris_board_se.png` | dekor | 32x32 | dekor (301) | 6 | `small wooden nine men's morris game board with a few black and white pieces` |
| 14 | `item.spinning_top` (yüzey) | `iso_item_spinning_top_se.png` | dekor | 32x32 | dekor (301) | 10 | `small wooden spinning top toy` |
| 15 | `item.story_book` (yüzey) | `iso_item_story_book_se.png` | dekor | 32x32 | dekor (301) | 8 | `small closed leather-bound book` |

## Doğrulama
301 ile aynı (validate_iso, ayak izi kaplaması, maske dışı, kılavuz rengi, ışık sırası, ayna uygunluğu). Ek olarak:
- Aynı takımdaki eşyalar yan yana önizlemede (renk uyumu): baskın ahşap tonlarının ortalama ton/doygunluğu tabloda.
- Yüzeyi olan eşyalarda üst yüzeyin y aralığı (ALTTAN) ve yuvaların üst yüzeye düştüğü önizlemede işaretli.

## Notlar
- Hepsi yalnızca SE (1x1) ya da `se_nw` (Nx1). Diğer yönler ve ayna sonra.
- Pivot değerlerini kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler; işçi o dosyaya dokunmaz. Kodda eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir.
- Yüzey dekoru (5 eşya) 32x32 tuval, 301'deki bardak kılavuzu; en alt opak satır taban elmasının alt yarısında olmalı.
- Bütçe sınırında (25): sırayla üret, 25'e yaklaşırsan kalanları teslim etmeden dur ve raporla.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
