unity-import: pending

# 312 Saklama eşyaları (rapor)

Yan yana sayfası: [312-yanyana.png](312-yanyana.png). Teslim edilmeyen denemeler: `art/results/312-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Araçta seed yok; ikinci deneme aynı kılavuz ve maskeyle. Çağrılar (1 gen each): fıçı 1, kök mahzeni 2 (ikinci deneme: ilk deneme stil KALDI) = 3 generation (cost_max 5). `get_balance` iş sonu: 885,70 (paralel şeritler de harcıyor).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | tuzlama fıçısı | teslim | 29 renk, 80x72 |
| 2 | kök mahzeni 2x2 | TESLİM EDİLMEDİ | 1. deneme: stil KALDI (palet %79, palet dışı renk), tabana değmiyor (en alt y=88, taban 103'e kadar), yalnızca x24-90 (ayak izi dolmuyor), toprak höyük yerine ağaç kütüğü/koni. 2. deneme: stil UYARI (soluk x0,59), ama şekil höyük değil duvar köşesi gibi, sola kaymış (x41-103), kapı açık/sarı çerçeveli (tarif: kapalı), tanınmıyor. |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_salting_barrel_se.png | 80x72 | 40 | 24 |

## Doğrulama
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| fıçı | x15-64, y10-61 | 29 | 0 | yok | geçti |
| mahzen d1 (red) | x24-90, y27-88 | 15 | 0 | yok | geçti (şekil kötü) |
| mahzen d2 (red) | x41-103, y29-95 | 29 | 0 | yok | geçti (şekil kötü) |

Kılavuz rengi kalıntısı otomatik ölçülmedi, görselde yok. Fıçıda sıvı/yiyecek yok, kapak kapalı.

## Stil denetimi
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| fıçı | %100 | 5.5 | x0.71 | GEÇTİ |
| mahzen d1 | %79 | 15.3 | x0.89 | KALDI |
| mahzen d2 | %88 | 9.8 | x0.59 | UYARI (soluk) |

## Işık / ayna
Fıçı: üst 76, sol 70, sağ 70 (yüzler eşit; sol-sağ ayrımı zayıf ⚠). Simetrik, tek sprite her yöne gider.

## Sorunlar
- Kök mahzeni için iki deneme tükendi; yeni iş (farklı tarif, ör. "grassy earth mound with wooden door", ya da `create_1_direction_object`) gerekir.
