---
id: "305"
title: Dalga 1: hayvan barınakları (kümes 2x2, ahır 3x2, kulübe, kedi sepeti, arı kovanı)
status: pending
approved_by: pending     # Dalga 1 planı (09 belgesi, 2026-11-08); kullanıcı "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["301"]   # yöntem 301 ile aynı; 2026-11-13: öneri bağımlılığa çevrildi (301 stil pilotu kabul edilmeden başlamaz)
max_attempts: 2          # parça başına en çok 2 deneme
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_coop_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_barn_3x2_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_kennel_se.png
  - Assets/Art/Iso/_aday/iso_item_cat_basket_se.png
  - Assets/Art/Iso/_aday/iso_item_beehive_se.png
created: 2026-11-08
---
# 305 Dalga 1: hayvan barınakları (kümes 2x2, ahır 3x2, kulübe, kedi sepeti, arı kovanı)

## Amaç
Hayvanlar 1.0'a girdi (kullanıcı kararı 2026-11-08). Barınaklar `items.json`'da: kümes (tavuk, ördek), ahır (keçi, koyun, inek), köpek kulübesi, kedi sepeti, arı kovanı. Hayvan görselleri 101-106 işlerinde.

## Tarif
Yöntem, ortak ayarlar, ortak açıklama eki, ikinci deneme kuralı, renk sınırı (48), yön ölçümü, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme biçimi **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif). Farklar:
- Masaüstü klasörü `C:\Users\User\OneDrive\Desktop\art\esya\<yyyy-mm-dd>_<ad>\`, toplu önizleme `<yyyy-mm-dd>_{id}_toplu_x4.png`.
- Renk ve ölçek referansı: 300'de kabul edilen masa, sandalye, tabure (301 Notlar). Takım (stil seti) renkleri:
  - **Köylü:** kaba açık-orta kahve çam, saman, ip bağ, kil. Süssüz.
  - **Zanaatkâr:** koyu meşe, demir kulp ve çivi, yünlü örtü (soluk kırmızı ya da hardal). Düzgün işçilik.
- Açıklama metnine takımın tarifi eklenir: Köylü için `rough light pine wood, rope lashing`, Zanaatkâr için `dark oak wood with iron fittings, neat craftsmanship`.
- **2x2 ve 3x2 kılavuz:** `tools/art/footprint_guide.py` yalnızca Nx1 çiziyor. İşçi `--m` (kısa eksendeki karo sayısı, varsayılan 1) ekler; eski çağrıların çıktısı değişmemeli (`--m 1` ile 300/301 kılavuzları bayt bayt aynı). `tools/art` işçinin yazabileceği yol. Ekleyemezse durur ve `## Sorular` yazar.

### Eşyalar
| # | items.json id | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|---|---|
| 1 | `item.coop` (2x2, Köylü) | `iso_item_coop_2x2_se.png` | 2x2 | 144x120 | `--n 2 --m 2` (aşağıda) | 54 | `small wooden chicken coop with a slanted plank roof, a little ramp and a fenced run` |
| 2 | `item.barn` (3x2, Zanaatkâr) | `iso_item_barn_3x2_se_nw.png` | 3x2 | 176x152 | `--n 3 --m 2 --axis x` (aşağıda) | 80 | `small timber animal barn with a thatched roof and a wide open door, hay inside` |
| 3 | `item.kennel` (1x1, Köylü) | `iso_item_kennel_se.png` | 1x1 | 80x80 | `--n 1` | 36 | `small wooden dog kennel with a round door opening, empty` |
| 4 | `item.cat_basket` (1x1, Köylü) | `iso_item_cat_basket_se.png` | 1x1 | 80x56 | `--n 1` | 14 | `round woven wicker basket with a small cushion, empty` |
| 5 | `item.beehive` (1x1, Zanaatkâr) | `iso_item_beehive_se.png` | 1x1 | 80x88 | `--n 1` | 40 | `traditional woven straw skep beehive on a wooden stand` |

## Doğrulama
301 ile aynı (validate_iso, ayak izi kaplaması, maske dışı, kılavuz rengi, ışık sırası, ayna uygunluğu). Ek olarak:
- Aynı takımdaki eşyalar yan yana önizlemede (renk uyumu): baskın ahşap tonlarının ortalama ton/doygunluğu tabloda.
- Yüzeyi olan eşyalarda üst yüzeyin y aralığı (ALTTAN) ve yuvaların üst yüzeye düştüğü önizlemede işaretli.

## Notlar
- Hepsi yalnızca SE (1x1) ya da `se_nw` (Nx1). Diğer yönler ve ayna sonra.
- Pivot değerlerini kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler; işçi o dosyaya dokunmaz. Kodda eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir.
- Arı kovanı için ayrı arı görseli gerekmez (arılar kovanın kendisi).
- Tavuk, ördek, keçi, koyun, inek, köpek, kedi: 101-106. Oyunda şimdilik yer tutucu (renkli küçük küp); koyunda 100'ün `sheep_1` adayı kullanılıyor.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
