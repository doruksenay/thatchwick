unity-import: pending

# 390 Yörük Odası mobilyası ve halılar (rapor)

Yan yana sayfası: [390-yanyana.png](390-yanyana.png). Teslim edilmeyenler: `art/results/390-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuz + maske (referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`; 301'deki ortak ek + takım eki (Zanaatkâr: dark oak wood with iron fittings; Köylü: rough light pine wood, rope lashing). Halı, minder ve hasırda ahşap ibaresi çıkarıldı, "flat thin rug lying on the floor" eklendi. 11 çağrı x 1 gen = 11 (cost_max 20). Yeniden deneme yok; tek istisna: kilim_large'ın ilk çağrısı base64 aktarım hatasıyla üretime girmedi (ücret yok), aynı girdiyle yeniden gönderildi.
Kılavuz tuvalleri: sedir 112x88 h28, sedir_long 144x96 h28, minder 80x56 h12, sini 80x64 h22, yüklük 112x136 h70, yer yatağı 112x80 h12, sedir yatak 112x88 h30, küçük kilim 112x72 h2, büyük kilim 176x96 h2, cicim ve hasır 144x88 h2.
`get_balance` sonrası: 4596,9 (paralel şeritler de harcıyor).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | sedir | teslim | |
| 2 | sedir_long | teslim | |
| 3 | floor_cushion | teslim (UYARI) | palete uzak (10,6) |
| 4 | sini_table | TESLİM EDİLMEDİ | ham: stil KALDI (doygunluk x1,39, %72 palet); `fit` sonrası stil UYARI ama 57 renk > 48. Tanınıyor. `390-red/` |
| 5 | yukluk | teslim | |
| 6 | floor_bed | teslim | |
| 7 | sedir_bed | teslim | |
| 8 | kilim_small | teslim (`fit`, UYARI) | ham doygunluk x2,03 KALDI; fit sonrası x1,15 |
| 9 | kilim_large | teslim (`fit`) | ham x1,56 KALDI |
| 10 | cicim | teslim (`fit`, UYARI) | ham palete %14 yakın KALDI |
| 11 | herb_rush_mat | TESLİM EDİLMEDİ | palet dışı %22; `fit` ile düzelmedi (renk sorunu). `390-red/` |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_sedir_2x1_se.png | 112x88 | 56 | 32 |
| iso_item_sedir_long_3x1_se.png | 144x96 | 72 | 40 |
| iso_item_floor_cushion_se.png | 80x56 | 40 | 24 |
| iso_item_yukluk_2x1_se.png | 112x136 | 56 | 32 |
| iso_item_floor_bed_2x1_se.png | 112x80 | 56 | 32 |
| iso_item_sedir_bed_2x1_se.png | 112x88 | 56 | 32 |
| iso_item_kilim_small_2x1_se.png | 112x72 | 56 | 32 |
| iso_item_kilim_large_3x2_se.png | 176x96 | 88 | 48 |
| iso_item_cicim_2x2_se.png | 144x88 | 72 | 40 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır (x, y) | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| sedir | 8-103, 3-80 | 13 | 0 | yok | geçti |
| sedir_long | 12-135, 4-88 | 37 | 0 | yok | geçti |
| floor_cushion | 10-69, 13-48 | 37 | 0 | yok | geçti |
| yukluk | 10-101, 9-128 | 22 | 0 | yok | geçti |
| floor_bed | 8-103, 11-72 | 23 | 0 | yok | geçti |
| sedir_bed | 9-102, 1-80 | 23 | 0 | yok | geçti |
| kilim_small | 8-103, 13-63 | 35 | 0 | yok | geçti |
| kilim_large | 8-167, 5-88 | 29 | 0 | yok | geçti |
| cicim | 8-135, 13-80 | 19 | 0 | yok | geçti |
| (red) sini_table fit | 14-65, 4-56 | 57 | 0 | yok | BAŞARISIZ (renk) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| sedir | %100 | 5,1 | x0,69 | GEÇTİ |
| sedir_long | %100 | 6,4 | x0,88 | GEÇTİ |
| floor_cushion | %86 | 10,6 | x1,19 | UYARI |
| yukluk | %100 | 6,8 | x0,77 | GEÇTİ |
| floor_bed | %100 | 6,7 | x0,91 | GEÇTİ |
| sedir_bed | %100 | 8,0 | x0,69 | GEÇTİ |
| kilim_small (fit) | %84 | 10,4 | x1,15 | UYARI |
| kilim_large (fit) | %100 | 6,2 | x1,17 | GEÇTİ |
| cicim (fit) | %100 | 15,4 | x1,15 | UYARI |
| (red) herb_rush_mat | %78 | 13,2 | x1,04 | KALDI |

## Sorunlar
- Halılar için döşeme testi, ışık sırası ve ayna ölçümü yapılmadı; yan yana sayfasına bakılmalı.
- sini_table 48 renk sınırını aştı (57); kullanıcı `reduce_colors` ile kurtarmayı onaylayabilir (yaklaşık 0,1 gen).
- Kilim ve cicim renkleri `fit` ile soluklaştırıldı (kök boya varyantları motorda).
- Hepsi SE yönünde; sedir ve yatak simetrisi ölçülmedi.
