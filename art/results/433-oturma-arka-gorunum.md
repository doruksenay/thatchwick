unity-import: pending

# 433 Yerleşim: oturma eşyalarının arkası dönük görünümü (rapor)

Sonuç: **10/12 teslim**, 2 teslim edilmedi (boyalı sandalye `_se` ve `_ne`: stil KALDI). Yan yana: [433-yanyana.png](433-yanyana.png). Ham hâller: `art/results/433-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`, `--axis x`; ön görselin tuval boyutu aynen: sandalye 80x84/80x88, bank 112x80/112x88/112x104; yükseklik 44-68). `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 12 çağrı x 1 gen = **12 generation** (cost_est 12). Settle bankı ilk iki gönderimde base64 kopyalama hatasıyla reddedildi (üretim başlamadı, ücret yok); kılavuz paletli PNG'ye küçültülüp üçüncüde gitti. `get_balance` sonrası: **2372,90** (paralel şeritler de harcıyor).
Doygunluk düzeltmesi (`style.py fit`): koltuk (ham x1,61) ve ağartılmış bank (ham x1,58, palete uzak); ham hâller `433-red/raw_*`.

## Teslim edilenler (Assets/Art/Iso/_aday/)
| Dosya | Boyut | Pivot x / y (alttan) | Stil | validate_iso |
|---|---|---|---|---|
| iso_item_chair_wood_ne.png | 80x84 | 40 / 24 | GEÇTİ | geçti |
| iso_item_chair_carved_ne.png | 80x88 | 40 / 24 | GEÇTİ | geçti |
| iso_item_armchair_cushion_ne.png | 80x88 | 40 / 24 | GEÇTİ (fit x0,72) | geçti |
| iso_item_x_chair_ne.png | 80x88 | 40 / 24 | UYARI (doygun x1,48) | geçti |
| iso_item_bench_long_2x1_ne.png | 112x80 | 56 / 32 | GEÇTİ | geçti |
| iso_item_han_bench_2x1_ne.png | 112x80 | 56 / 32 | GEÇTİ | geçti |
| iso_item_garden_bench_2x1_ne.png | 112x88 | 56 / 32 | UYARI (palete uzak %89) | geçti |
| iso_item_settle_bench_2x1_ne.png | 112x104 | 56 / 32 | GEÇTİ | geçti |
| iso_item_chest_bench_2x1_ne.png | 112x80 | 56 / 32 | GEÇTİ | geçti |
| iso_item_bleached_bench_2x1_ne.png | 112x80 | 56 / 32 | GEÇTİ (fit x0,73) | geçti |

Pivotlar kılavuzdan (taban elmasının merkezi). Yarı saydam piksel 0.

## Teslim EDİLMEYEN (`433-red/`)
| Dosya | Neden |
|---|---|
| iso_item_chair_painted_se | stil KALDI (palete %69, palet dışı mavi-yeşil boya). `fit` palet sorununu çözmez |
| iso_item_chair_painted_ne | stil KALDI (palete %27) ve 55 renk. Ön görsel de çıkmadığından arka görsel anlamsız |

## Sorunlar (kullanıcı yan yana sayfasında karar versin)
- **Yön güvenilir değil.** Stil eşleme modu "arkadan" tarifini tutarlı uygulamıyor: `chair_wood_ne`, `chair_carved_ne`, `armchair_cushion_ne` ve `x_chair_ne` oturağı hâlâ görünür (yarı ön/yan) çıktı; gerçek arka görünüm sayılmayabilir. `han_bench`, `garden_bench`, `chest_bench`, `settle_bench` sırtlığı öne gelmiş gibi görünüyor. `bench_long` ve `bleached_bench` sırtlıksız düz tahta, yön farkı yok.
- Sırtlıksız bank (`bench_long`, `bleached`) ön görselin neredeyse aynısı; ayrı `_ne` dosyası gerekmeyebilir.
- Boyalı sandalye için palet kararı kullanıcıda (mavi-yeşil boya aile paleti dışı). Öneri: ayrı iş, ahşap tonlu boya.
- `items.json` eşlemesi yapılmadı.
