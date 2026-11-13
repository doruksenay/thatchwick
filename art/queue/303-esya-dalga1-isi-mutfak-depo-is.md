---
id: "303"
title: Dalga 1: ısınma, mutfak, depo ve iş (10 eşya, yalnızca SE)
status: pending
approved_by: pending     # Dalga 1 planı (09 belgesi, 2026-11-08); kullanıcı "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 11
cost_max: 22
depends_on: ["301"]   # yöntem 301 ile aynı; 2026-11-13: öneri bağımlılığa çevrildi (301 stil pilotu kabul edilmeden başlamaz)
max_attempts: 2          # parça başına en çok 2 deneme
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_brazier_se.png
  - Assets/Art/Iso/_aday/iso_item_fireplace_large_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_stone_oven_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_water_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_drying_rack_se.png
  - Assets/Art/Iso/_aday/iso_item_barrel_se.png
  - Assets/Art/Iso/_aday/iso_item_woodshed_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_workbench_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_seed_crate_se.png
  - Assets/Art/Iso/_aday/iso_item_whetstone_se.png
created: 2026-11-08
---
# 303 Dalga 1: ısınma, mutfak, depo ve iş (10 eşya, yalnızca SE)

## Amaç
09 belgesindeki Dalga 1 eşyalarından ısınma, mutfak, depo ve iş kategorileri. Marangoz tezgâhı yeni üretim sisteminin istasyonudur.

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
| 1 | `item.brazier` (1x1, Köylü, ısı 40) | `iso_item_brazier_se.png` | 1x1 | 80x64 | `--n 1` | 22 | `small iron brazier bowl on three legs with glowing coals, no flames taller than the bowl` |
| 2 | `item.fireplace_large` (2x1, Zanaatkâr, ısı 140) | `iso_item_fireplace_large_2x1_se_nw.png` | 2x1 | 112x120 | `--n 2 --axis x` | 64 | `large stone fireplace with a wide hearth opening, unlit, grey fieldstone, wooden mantel` |
| 3 | `item.stone_oven` (2x1, Zanaatkâr) | `iso_item_stone_oven_2x1_se_nw.png` | 2x1 | 112x104 | `--n 2 --axis x` | 48 | `dome-shaped clay and stone bread oven with a small arched opening, unlit` |
| 4 | `item.water_barrel` (1x1, Köylü) | `iso_item_water_barrel_se.png` | 1x1 | 80x72 | `--n 1` | 30 | `open wooden water barrel with iron hoops, dark water surface visible at the top` |
| 5 | `item.drying_rack` (1x1, Köylü) | `iso_item_drying_rack_se.png` | 1x1 | 80x88 | `--n 1` | 42 | `simple wooden drying rack with hanging herb bundles and a few dried fish` |
| 6 | `item.barrel` (1x1, Köylü) | `iso_item_barrel_se.png` | 1x1 | 80x72 | `--n 1` | 30 | `closed wooden storage barrel with iron hoops, lid on` |
| 7 | `item.woodshed` (2x1, Köylü, dış) | `iso_item_woodshed_2x1_se_nw.png` | 2x1 | 112x104 | `--n 2 --axis x` | 48 | `small open woodshed with a slanted plank roof, stacked split firewood inside` |
| 8 | `item.workbench` (2x1, Zanaatkâr) | `iso_item_workbench_2x1_se_nw.png` | 2x1 | 112x88 | `--n 2 --axis x` | 32 | `carpenter workbench, thick oak top with a vise, a saw and a mallet lying on it, shavings` |
| 9 | `item.seed_crate` (1x1, Köylü) | `iso_item_seed_crate_se.png` | 1x1 | 80x64 | `--n 1` | 20 | `open wooden crate with small cloth seed sacks inside` |
| 10 | `item.whetstone` (1x1, Zanaatkâr) | `iso_item_whetstone_se.png` | 1x1 | 80x72 | `--n 1` | 30 | `round grinding whetstone wheel on a wooden frame with a hand crank` |

## Doğrulama
301 ile aynı (validate_iso, ayak izi kaplaması, maske dışı, kılavuz rengi, ışık sırası, ayna uygunluğu). Ek olarak:
- Aynı takımdaki eşyalar yan yana önizlemede (renk uyumu): baskın ahşap tonlarının ortalama ton/doygunluğu tabloda.
- Yüzeyi olan eşyalarda üst yüzeyin y aralığı (ALTTAN) ve yuvaların üst yüzeye düştüğü önizlemede işaretli.

## Notlar
- Hepsi yalnızca SE (1x1) ya da `se_nw` (Nx1). Diğer yönler ve ayna sonra.
- Pivot değerlerini kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler; işçi o dosyaya dokunmaz. Kodda eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir.
- Ateşli eşyalar (mangal): yalnızca yanık hâl. Büyük şöminenin yanık/sönük çifti sonra (taş ocağın ikili düzeni gibi).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
