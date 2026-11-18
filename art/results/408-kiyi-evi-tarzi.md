unity-import: pending

# 408 Kıyı Evi tarzı (8 eşya): 7/8 teslim, review

Yan yana sayfası: [408-yanyana.png](408-yanyana.png). Teslim edilmeyen: `art/results/408-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, özel maske; `low detail`, `basic shading`, `single color outline`. Yer eşyaları `high top-down`, duvar eşyaları `side`.
- Kılavuz: yer eşyalarında `footprint_guide.py` (ip 80x56 h12, bank 112x80 `--n 2 --axis x` h24), yüzey dekorunda (kabuk kasesi, balık sepeti) 301'deki 32x32 12x6 elmas (h10 ve h16), duvar eşyalarında "/" eğimli paralelkenar, duvar rengi (184,160,122) (ağ perdesi 72x72, kürek 40x56, kurutulmuş balık 40x60, raf 40x40). Betikler `art/tmp408/` (commit edilmez).
- Açıklama: tarifteki metin + 301 ortak eki (kahverengi ahşap yerine "sun-bleached grey-beige" ve kum tonları) + duvar eki.
- Çağrı: 9 çağrı x 1 gen = ~9 (cost_max 16). Ağ perdesinin ilk çağrısı 72x70 tuvalde sunucuda "Height mismatch: 280 != 272" ile üretime girmeden düştü (tuval 8'in katı olmalı); aynı kılavuz 72x72 yapılıp bir kez yeniden gönderildi.
- Doygunluk düzeltmesi (`style.py fit`): bank (ham x1.59 KALDI → x0.72 ölçek, denetim GEÇTİ). İp makarası da `fit` edildi (UYARI) ama renk sayısı 51 > 48 oldu: teslim edilmedi.
- `get_balance` sonrası: 4117,9 (paralel şeritler de harcıyor).

## Dosyalar (`Assets/Art/Iso/_aday/`; pivot ALTTAN)
| Dosya | Boyut | Pivot x | Pivot y |
|---|---|---|---|
| iso_wall_item_net_curtain_2w_sw_ne.png | 72x72 | 36 | 36 |
| iso_wall_item_oar_rack_sw_ne.png | 40x56 | 20 | 28 |
| iso_item_fish_basket_decor_se.png | 32x32 | 16 | 7 |
| iso_wall_item_dried_fish_string_sw_ne.png | 40x60 | 20 | 30 |
| iso_item_shell_bowl_se.png | 32x32 | 16 | 7 |
| iso_item_bleached_bench_2x1_se.png | 112x80 | 56 | 32 |
| iso_wall_item_green_bottle_shelf_sw_ne.png | 40x40 | 20 | 20 |
| iso_item_rope_coil_se.png | TESLİM EDİLMEDİ | | |

## Doğrulama (`validate_iso.py --kind object --max-colors 48`)
| Dosya | Opak sınır | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| net_curtain | x6-66 y7-63 | 31 | 0 | geçti |
| oar_rack | x4-35 y4-50 | 11 | 0 | geçti |
| fish_basket | x10-21 y7-23 | 9 | 0 | geçti |
| dried_fish_string | x4-35 y7-51 | 33 | 0 | geçti |
| shell_bowl | x10-21 y11-21 | 15 | 0 | geçti |
| bleached_bench (fit) | x16-96 y13-64 | 43 | 0 | geçti |
| green_bottle_shelf | x6-35 y8-30 | 19 | 0 | geçti |
| rope_coil (fit, red) | x14-65 y6-42 | 51 | 0 | BAŞARISIZ (renk) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| net_curtain | %100 | 6.0 | x0.75 | GEÇTİ |
| oar_rack | %100 | 6.9 | x0.55 | UYARI (soluk) |
| fish_basket | %100 | 5.8 | x0.68 | GEÇTİ |
| dried_fish_string | %87 | 11.4 | x0.92 | UYARI (palete uzak) |
| shell_bowl | %100 | 9.2 | x1.04 | GEÇTİ |
| bleached_bench (fit) | %100 | 9.3 | x1.16 | GEÇTİ |
| green_bottle_shelf | %100 | 6.5 | x1.13 | GEÇTİ |
| rope_coil ham | %79 | 15.6 | x1.33 | KALDI |
| rope_coil fit | %86 | 13.6 | x1.15 | UYARI ama 51 renk |

## Sorunlar
- İp makarası teslim edilmedi: ham stil KALDI, `fit` sonrası 51 renk (>48). Tanınıyor; kullanıcı `reduce_colors` (~0,1 gen) ile kurtarmayı onaylayabilir. `408-red/rope_coil_fit.png`.
- Ağ perdesi: üstte kalın bir kiriş çıktı ("perde" yerine çerçeveli ağ); göz kararı bakılmalı. 2 kenar genişliği 64 px kılavuzdan geldi (380'deki 96x56 yerine 72x72).
- Kürek askısı soluk ve küçük; kurutulmuş balıklar turuncu-kırmızı (palete uzak UYARI).
- Bank ham hâlde pembeydi, `fit` ile soluklaştırıldı ("ağartılmış gri" için yine sıcak kalmış olabilir).
- Kabuk kasesi ve balık sepeti 32x32'de çok küçük; kabuklar seçilemiyor olabilir.
- Döşeme testi, ışık ve ayna ölçümü yapılmadı; yan yana sayfasına bakılmalı. Ayna: duvar eşyalarında öbür yüz aynayla.
