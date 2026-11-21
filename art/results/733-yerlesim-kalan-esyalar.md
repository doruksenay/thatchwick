unity-import: pending

# 733 492'de teslim edilmeyen yer eşyaları (rapor)

Yan yana sayfası: [733-yanyana.png](733-yanyana.png). Teslim edilmeyenler: `art/results/733-red/` (3 dosya).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py --axis x`; yükseklikler: sini 20, bakır kazanlar 32, fıçı masa 40, yalak 24, yatak 28, kurutma rafı 64, ip yumağı 10). `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + ortak ek. 7 çağrı = 7 generation (cost_est 9, cost_max 18), hepsi paralel, tek deneme. `get_balance`: iş öncesi 9787, iş sonu 9746 (paralel şeritler de harcıyor; bu işin payı 7).
Ham çıktılar `art/tmp733/` içinde (commit edilmez).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | sini masa | teslim | GEÇTİ |
| 2 | bakır kazanlar | TESLİM EDİLMEDİ | üç fıçı gibi okunuyor (kazan değil), altında koyu zemin plakası; 13 renk |
| 3 | fıçı masa | teslim | GEÇTİ; üstte kupa görünüyor |
| 4 | su yalağı | TESLİM EDİLMEDİ | stil KALDI (%74 palet, mavi su palet dışı) |
| 5 | yatak çerçevesi | teslim (UYARI) | doygun x1,32 |
| 6 | kurutma rafı | teslim | GEÇTİ; zemin yok, otlar ve et şeritleri asılı |
| 7 | ip yumağı | TESLİM EDİLMEDİ | stil KALDI (%61 palet, x1,45 doygun; pembe-mor şeritli halat) |

esya ailesinin kilidi kurulu olduğu için `fit` uygulanmadı (yalnızca kilitsiz aileler için). Yeniden üretim yapılmadı (max_attempts içinde tek deneme).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_sini_table_se.png | 80x64 | 40 | 24 |
| iso_item_barrel_table_se.png | 80x80 | 40 | 24 |
| iso_item_bed_frame_2x1_se_nw.png | 112x88 | 56 | 32 |
| iso_item_drying_rack_2x1_se_nw.png | 112x96 | 56 | 32 |

## Doğrulama (validate_iso --kind object --max-colors 48)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| sini masa | x14-65, y8-56 | 39 | 0 | geçti |
| fıçı masa | x11-67, y7-72 | 47 | 0 | geçti |
| yatak | x13-98, y11-79 | 35 | 0 | geçti |
| kurutma rafı | x10-101, y6-88 | 33 | 0 | geçti |
| (red) bakır kazanlar | x8-71, y9-64 | 13 | 0 | geçti (şekil/zemin nedeniyle reddedildi) |
| (red) yalak | x8-103, y13-72 | 37 | 0 | geçti (stil KALDI) |
| (red) ip yumağı | x10-69, y9-44 | 44 | 0 | geçti (stil KALDI) |

## Stil denetimi (style.py check --family esya)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| sini masa | %100 | 7.1 | x1.07 | GEÇTİ |
| fıçı masa | %89 | 9.1 | x1.01 | GEÇTİ |
| yatak | %95 | 8.1 | x1.32 | UYARI |
| kurutma rafı | %91 | 8.9 | x1.08 | GEÇTİ |
| (red) bakır kazanlar | %100 | 4.9 | x0.84 | GEÇTİ |
| (red) ip yumağı | %61 | 18.0 | x1.45 | KALDI |
| (red) yalak | %74 | 13.8 | x0.77 | KALDI |

## Sorunlar
- Ölçülmedi: ayna uygunluğu, ayak izi kaplaması sayısal değeri; zemin değil, döşeme testi yok. Gölge pişirilmemiş, ışık sol-üstten (gözle).
- Kalan 3 eşya (bakır kazanlar, su yalağı, ip yumağı) için öneri: farklı açıklamayla yeni iş (kazanlar için "cauldron pots" yerine tek büyük kazan; yalakta suyu gri-yeşil; ip için "undyed beige hemp rope"). Dosyalar `733-red/`.
