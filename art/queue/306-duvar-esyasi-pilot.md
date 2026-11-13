---
id: "306"
title: Duvar eşyası pilotu (meşale, gaz lambası, duvar rafı, ot demeti, alet askısı)
status: pending
approved_by: pending     # Dalga 1 planı (09 belgesi, 2026-11-08); kullanıcı "user" yapınca işçi alır
family: esya
style_gate: false
cost_est: 5
cost_max: 10
depends_on: ["301"]   # yöntem 301 ile aynı; 2026-11-13: öneri bağımlılığa çevrildi (301 stil pilotu kabul edilmeden başlamaz)
max_attempts: 2          # parça başına en çok 2 deneme
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_torch_wall_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_oil_lamp_wall_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_wall_shelf_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_herb_bundle_wall_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_tool_rack_wall_sw_ne.png
created: 2026-11-08
---
# 306 Duvar eşyası pilotu (meşale, gaz lambası, duvar rafı, ot demeti, alet askısı)

## Amaç
Duvar eşyaları 1.0'a girdi (kullanıcı kararı: duvar rafları, meşale, gaz lambası). Kodda yeni yerleşim türü `Wall`: eşya duvarın iç yüzüne asılır, karo kaplamaz. Bu iş, duvar yüzüne oturan eşyanın nasıl üretileceğini dener.

## Tarif
Yöntem, ortak ayarlar, ortak açıklama eki, ikinci deneme kuralı, renk sınırı (48), yön ölçümü, teslim (`cp -n`, `.meta` yok), masaüstü kopyası ve önizleme biçimi **301 ile aynıdır** (`art/queue/301-esya-grup2.md` > Tarif). Farklar:
- Masaüstü klasörü `C:\Users\User\OneDrive\Desktop\art\esya\<yyyy-mm-dd>_<ad>\`, toplu önizleme `<yyyy-mm-dd>_{id}_toplu_x4.png`.
- Renk ve ölçek referansı: 300'de kabul edilen masa, sandalye, tabure (301 Notlar). Takım (stil seti) renkleri:
  - **Köylü:** kaba açık-orta kahve çam, saman, ip bağ, kil. Süssüz.
  - **Zanaatkâr:** koyu meşe, demir kulp ve çivi, yünlü örtü (soluk kırmızı ya da hardal). Düzgün işçilik.
- Açıklama metnine takımın tarifi eklenir: Köylü için `rough light pine wood, rope lashing`, Zanaatkâr için `dark oak wood with iron fittings, neat craftsmanship`.
- **Duvar kılavuzu (yeni):** eşya, kabul edilmiş duvar görselinin (`iso_wall_wall_sw_ne.png`, 102x115, pivot (32,19)) iç yüzüne asılı durur. Yüz "/" eğiminde (sw_ne): kılavuz, tuvalde 2:1 eğimli bir paralelkenar (duvar yüzü parçası), maske bu paralelkenarın eşyanın sığacağı kısmı. İşçi kılavuzu `tools/art/` altında küçük bir betikle üretir.
- Açıklama ekine `mounted flat against a wall that slopes up to the right (isometric), seen from the room side` eklenir.
- Pilot olduğu için önce **yalnızca meşale** denenir; tutarsa diğer dördü. Tutmazsa durur ve `## Sorular` yazar (alternatif: `create_image_pixflux` ile önden çizip kodda eğmek).
- Ayna: "\" yüzü (se_nw) aynayla çıkar; ışık sağ-üstten gelir, not et. Kodda: karonun +y kenarındaki duvar "/" (sw_ne), +x kenarındaki "\" (se_nw).

### Eşyalar
| # | items.json id | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni (+ ortak ek + takım eki) |
|---|---|---|---|---|---|---|---|
| 1 | `item.torch_wall` (duvar, Köylü) | `iso_wall_item_torch_wall_sw_ne.png` | duvar | 48x64 | duvar (aşağıda) | 40 | `burning wall torch in an iron bracket` |
| 2 | `item.oil_lamp_wall` (duvar, Zanaatkâr) | `iso_wall_item_oil_lamp_wall_sw_ne.png` | duvar | 48x64 | duvar (aşağıda) | 36 | `small iron oil lamp with a glass chimney hanging from a wall hook, lit` |
| 3 | `item.wall_shelf` (duvar, Köylü, 2 yuva) | `iso_wall_item_wall_shelf_sw_ne.png` | duvar | 64x48 | duvar (aşağıda) | 20 | `single rough plank wall shelf on two wooden brackets, empty top` |
| 4 | `item.herb_bundle_wall` (duvar, Köylü) | `iso_wall_item_herb_bundle_wall_sw_ne.png` | duvar | 40x56 | duvar (aşağıda) | 36 | `bundle of dried herbs hanging upside down from a wall peg` |
| 5 | `item.tool_rack_wall` (duvar, Zanaatkâr) | `iso_wall_item_tool_rack_wall_sw_ne.png` | duvar | 64x64 | duvar (aşağıda) | 40 | `wooden wall rack holding an axe, a hoe and a saw` |

## Doğrulama
301 ile aynı (validate_iso, ayak izi kaplaması, maske dışı, kılavuz rengi, ışık sırası, ayna uygunluğu). Ek olarak:
- Aynı takımdaki eşyalar yan yana önizlemede (renk uyumu): baskın ahşap tonlarının ortalama ton/doygunluğu tabloda.
- Yüzeyi olan eşyalarda üst yüzeyin y aralığı (ALTTAN) ve yuvaların üst yüzeye düştüğü önizlemede işaretli.

## Notlar
- Hepsi yalnızca `sw_ne` ("/" duvar yüzü). Diğer yüz aynayla.
- Pivot değerlerini kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler; işçi o dosyaya dokunmaz. Kodda eşya → dosya eşlemesi (`Assets/Scripts/Game/IsoArt.cs`) kod oturumunun işidir.
- Pivot: görselin **merkezi** (kod, eşyanın merkezini duvarda asılma noktasına koyar: kenar ortası, yerden ~1,3 birim yukarıda).
- Önizleme: duvar görselinin üstüne asılı hâli, yanında köylü (ölçek).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
