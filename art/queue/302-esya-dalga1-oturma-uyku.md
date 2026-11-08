---
id: "302"
title: Dalga 1: oturma, uyku ve masa (8 eşya, yalnızca SE)
status: pending
approved_by: pending     # Dalga 1 planı (09 belgesi, 2026-11-08); kullanıcı "user" yapınca işçi alır
cost_est: 9
cost_max: 18
depends_on: []           # yöntem 301 ile aynı; 301 sonucu görülmeden başlatılmaması önerilir
max_attempts: 2          # parça başına en çok 2 deneme
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_bench_long_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_mattress_straw_se.png
  - Assets/Art/Iso/_aday/iso_item_bed_frame_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_cradle_se.png
  - Assets/Art/Iso/_aday/iso_item_child_bed_se.png
  - Assets/Art/Iso/_aday/iso_item_stump_table_se.png
  - Assets/Art/Iso/_aday/iso_item_prep_counter_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_armchair_cushion_se.png
created: 2026-11-08
---
# 302 Dalga 1: oturma, uyku ve masa (8 eşya, yalnızca SE)

## Amaç
09 belgesindeki Dalga 1 eşyalarından oturma, uyku ve masa kategorileri. Hepsi `items.json`'da var, şimdilik yer tutucu küp olarak görünüyor.

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
| 1 | `item.bench_long` (2x1, Köylü) | `iso_item_bench_long_2x1_se_nw.png` | 2x1 | 112x80 | `--n 2 --axis x` | 22 | `long simple wooden bench without backrest, two people can sit` |
| 2 | `item.mattress_straw` (1x1, Köylü) | `iso_item_mattress_straw_se.png` | 1x1 | 80x56 | `--n 1` | 10 | `flat straw mattress on the floor with a rough linen cover and a small straw pillow` |
| 3 | `item.bed_frame` (2x1, Zanaatkâr) | `iso_item_bed_frame_2x1_se_nw.png` | 2x1 | 112x88 | `--n 2 --axis x` | 34 | `wooden bed frame with headboard and footboard, straw mattress and a muted red wool blanket` |
| 4 | `item.cradle` (1x1, Köylü) | `iso_item_cradle_se.png` | 1x1 | 80x72 | `--n 1` | 26 | `small wooden baby cradle on two curved rockers, empty, small linen blanket inside` |
| 5 | `item.child_bed` (1x1, Zanaatkâr) | `iso_item_child_bed_se.png` | 1x1 | 80x72 | `--n 1` | 26 | `short wooden child bed with low headboard, straw mattress and a small wool blanket` |
| 6 | `item.stump_table` (1x1, Köylü, 1 yuva) | `iso_item_stump_table_se.png` | 1x1 | 80x64 | `--n 1` | 20 | `low side table made from a wide cut tree stump, flat empty top surface clearly visible` |
| 7 | `item.prep_counter` (2x1, Zanaatkâr, 2 yuva) | `iso_item_prep_counter_2x1_se_nw.png` | 2x1 | 112x88 | `--n 2 --axis x` | 30 | `kitchen preparation counter, thick oak worktop, empty top surface clearly visible, open shelf below` |
| 8 | `item.armchair_cushion` (1x1, Zanaatkâr) | `iso_item_armchair_cushion_se.png` | 1x1 | 80x88 | `--n 1` | 40 | `wooden armchair with a padded wool seat cushion and armrests` |

## Doğrulama
301 ile aynı (validate_iso, ayak izi kaplaması, maske dışı, kılavuz rengi, ışık sırası, ayna uygunluğu). Ek olarak:
- Aynı takımdaki eşyalar yan yana önizlemede (renk uyumu): baskın ahşap tonlarının ortalama ton/doygunluğu tabloda.
- Yüzeyi olan eşyalarda üst yüzeyin y aralığı (ALTTAN) ve yuvaların üst yüzeye düştüğü önizlemede işaretli.

## Notlar
- Hepsi yalnızca SE (1x1) ya da `se_nw` (Nx1). Diğer yönler ve ayna sonra.
- Pivot değerlerini kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler; işçi o dosyaya dokunmaz. Kodda eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir.
- Asimetrik 1x1 eşyalar (beşik, koltuk): gerçek yönü ölç, `_se` ya da `_sw` olarak adlandır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
