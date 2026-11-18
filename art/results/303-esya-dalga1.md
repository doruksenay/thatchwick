unity-import: pending

# 303 Dalga 1: ısınma, mutfak, depo ve iş (rapor)

Yan yana sayfası: [303-yanyana.png](303-yanyana.png) (yalnızca teslim edilen 7 eşya). Teslim edilmeyenler: `art/results/303-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`, 1 bit palet PNG), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek ve takım eki. Araçta `seed` yok. Çağrılar paralel gönderildi. 10 ilk deneme + 2 ikinci deneme (bileme taşı: kırmızı çerçeve; şömine: yanık çıktı) = 12 generation (cost_max 22 içinde). `get_balance` iş sonu: 983,7 (paralel şeritler de harcıyor; bu işin payı 12).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | mangal | teslim (UYARI) | alevler kâse üstüne çıkıyor (yanık hâl, tarif yalnızca yanık hâl istiyor); doygun x1,37 |
| 2 | büyük şömine | TESLİM EDİLMEDİ | iki denemede de içinde ateş var ("unlit" ihlali); 2. denemede 49 renk (>48) |
| 3 | taş ocak | teslim (UYARI) | palete uzak (%94) |
| 4 | su fıçısı | TESLİM EDİLMEDİ | stil denetimi KALDI (%70 palet dışı); yeniden deneme koşulu yok |
| 5 | kurutma rafı | teslim | GEÇTİ |
| 6 | fıçı | teslim | GEÇTİ |
| 7 | odunluk | teslim (UYARI) | soluk x0,51 |
| 8 | çalışma tezgâhı | TESLİM EDİLMEDİ | 57 renk (>48), stil KALDI (çok doygun x1,54) |
| 9 | tohum sandığı | teslim (UYARI) | doygun x1,40 |
| 10 | bileme taşı | teslim (2. deneme) | 1. deneme kırmızı çerçeve + KALDI; 2. deneme GEÇTİ, 47 renk |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_brazier_se.png | 80x64 | 40 | 24 |
| iso_item_stone_oven_2x1_se_nw.png | 112x104 | 56 | 32 |
| iso_item_drying_rack_se.png | 80x88 | 40 | 24 |
| iso_item_barrel_se.png | 80x72 | 40 | 24 |
| iso_item_woodshed_2x1_se_nw.png | 112x104 | 56 | 32 |
| iso_item_seed_crate_se.png | 80x64 | 40 | 24 |
| iso_item_whetstone_se.png | 80x72 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`, ayak izi kutusu verildi)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| mangal | x21-58, y5-50 | 31 | 0 | yok | geçti |
| taş ocak | x8-103, y16-96 | 33 | 0 | yok | geçti |
| kurutma rafı | x10-69, y5-80 | 36 | 0 | yok | geçti |
| fıçı | x20-59, y10-62 | 25 | 0 | yok | geçti |
| odunluk | x9-103, y0-96 | 33 | 0 | yok | geçti |
| tohum sandığı | x8-71, y6-56 | 37 | 0 | yok | geçti |
| bileme taşı | x12-67, y7-64 | 47 | 0 | yok | geçti |
| (red) şömine 2. | x9-103, y0-112 | 49 | 0 | yok | BAŞARISIZ (renk) |
| (red) tezgâh | x8-103, y12-80 | 57 | 0 | yok | BAŞARISIZ (renk) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| mangal | %81 | 14.7 | x1.37 | UYARI |
| taş ocak | %94 | 13.6 | x0.85 | UYARI |
| kurutma rafı | %92 | 8.0 | x0.85 | GEÇTİ |
| fıçı | %98 | 5.8 | x0.70 | GEÇTİ |
| odunluk | %95 | 9.6 | x0.51 | UYARI |
| tohum sandığı | %87 | 6.5 | x1.40 | UYARI |
| bileme taşı | %94 | 9.3 | x1.08 | GEÇTİ |
| su fıçısı (red) | %70 | 14.2 | x1.08 | KALDI |
| tezgâh (red) | %90 | 13.5 | x1.54 | KALDI |
| şömine 2. (red) | %98 | 8.0 | x0.47 | UYARI |

## Yapılmayanlar / sorunlar
- Ölçülmedi: takım ton/doygunluk tablosu, ışık sırası (üst/sol/sağ parlaklık), ayna uygunluğu, üst yüzey y aralığı. Kullanıcı yan yana sayfasına bakmalı.
- Büyük şömine iki denemede de yanık çıktı; sönük hâl için ayrı iş ya da tarif değişikliği önerilir. Tezgâh ve su fıçısı için yeniden deneme koşulları (ayak izi/tanınma) tutmadığından ikinci deneme yapılmadı; yalnızca renk sayısı ve stil kaldı.
- Mangalda alevler kâse yüksekliğinin üstüne çıkıyor.
