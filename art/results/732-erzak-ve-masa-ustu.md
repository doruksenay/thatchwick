unity-import: pending

# 732 Erzak çuvalları, sepetler ve masa üstü süsler (rapor): 7/9 teslim, review

Yan yana sayfası: [732-yanyana.png](732-yanyana.png) (7 yeni eşya + esya referansları). Teslim edilmeyenler: `art/results/732-red/` (6 dosya, her iki deneme).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- `create_map_object`. 1x1 zemin eşyaları (çuvallar, sepetler) ve duvar eşyası: stil eşleme + kılavuz/maske (`footprint_guide.py --axis x`, yükseltmeler: 80x72 için 28, 80x64 için 24; duvar eşyası için `art/tmp732` içinde elle yapılan "/" paralelkenar kılavuz, duvar rengi (184,160,122), `side` görünüm). `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + ortak ek.
- Masa üstü 4 eşya (32x32): kılavuz elmas 64 px olduğundan 32 px tuvale sığmaz; basic mod (arka plansız), aynı ayarlar ve ek.
- 1. tur 9 çağrı (3'ü ilk denemede hız sınırı hatası verdi, hiçbiri üretilmedi/ücret yok; 30 sn sonra bir kez yeniden gönderildi). 2. tur (max_attempts 2): un çuvalı, tahıl çuvalı, balık sepeti, hasır sepet yeniden = 4 çağrı. Toplam 13 generation (cost_max 24, cost_est 12 içinde).
- get_balance: iş başı 1058,6; iş sonu 1031,6 (paralel şeritler de harcıyor; bu işin payı 13).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | un çuvalı | teslim | 2. deneme; 1. deneme (vazo/kabak biçimi, stil KALDI x1,55) `732-red/flour.png` |
| 2 | tahıl çuvalı | TESLİM EDİLMEDİ | 1. deneme 61 renk, 2. deneme stil KALDI (%56 palet, x1,73 doygun); şekil tanınıyor (`732-red/grain.png`, `grain2.png`) |
| 3 | balık sepeti | teslim (UYARI) | 2. deneme; sepet yerine kare tekne/leğen görünümlü, içinde iki balık (soluk x0,53); 1. deneme 61 renk |
| 4 | hasır sepet | TESLİM EDİLMEDİ | 1. deneme 49 renk + stil UYARI, halka biçimi; 2. deneme 53 renk, stil KALDI (x2,20) |
| 5 | hokka ve kalem | teslim | GEÇTİ |
| 6 | mühür | teslim (UYARI) | mühür çubuğu yatay çubuk gibi okunuyor, palete uzak |
| 7 | mektup kutusu | teslim | GEÇTİ |
| 8 | makas ve tebeşir | teslim (UYARI) | soluk x0,55 |
| 9 | keski rafı (duvar) | teslim | GEÇTİ |

Renk sınırı (48) aşanlara `fit` uygulanmadı (esya kilidi kurulu; doygunluk düzeltmesi kilitsiz aile içindir; 301 kuralı).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_flour_sack_se.png | 80x72 | 40 | 24 |
| iso_item_fish_basket_se.png | 80x64 | 40 | 24 |
| iso_item_ink_quill_se.png | 32x32 | 16 | 7 |
| iso_item_wax_seal_se.png | 32x32 | 16 | 7 |
| iso_item_letter_box_se.png | 32x32 | 16 | 7 |
| iso_item_shears_chalk_se.png | 32x32 | 16 | 7 |
| iso_wall_item_chisel_rack_sw_ne.png | 48x64 | 24 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| un çuvalı | x12-71, y13-61 | ≤48 | 0 | geçti |
| balık sepeti | x8-71, y1-55 | ≤48 | 0 | geçti |
| hokka | x10-21, y5-26 | 8 | 0 | geçti |
| mühür | x1-29, y3-28 | ≤48 | 0 | geçti |
| mektup kutusu | x3-27, y2-29 | 8 | 0 | geçti |
| makas | x3-28, y4-27 | 21 | 0 | geçti |
| keski rafı | x7-41, y12-54 | 27 | 0 | geçti |
| (red) tahıl 1 / balık 1 | — | 61 / 61 | 0 | BAŞARISIZ (renk) |
| (red) hasır 1 / 2 | — | 49 / 53 | 0 | BAŞARISIZ (renk) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| un çuvalı | %100 | 7.6 | x0.86 | GEÇTİ |
| balık sepeti | %100 | 5.6 | x0.53 | UYARI |
| hokka | %100 | 8.8 | x0.70 | GEÇTİ |
| mühür | %95 | 10.3 | x1.36 | UYARI |
| mektup kutusu | %100 | 5.5 | x1.10 | GEÇTİ |
| makas | %87 | 8.7 | x0.55 | UYARI |
| keski rafı | %99 | 6.2 | x1.05 | GEÇTİ |

## Sorunlar
- Tahıl çuvalı ve hasır sepet iki denemede de tutmadı; yeniden iş için kılavuz yüksekliği ya da renk azaltma (`reduce_colors`) önerilir (ayrı iş).
- Masa üstü eşyalar basic modda üretildi; kılavuz ayak izi doğrulaması yok, ölçek gözle. Duvar eşyasında maske dışı piksel sayımı yapılmadı.
- Zemin döşeme testi yok (zemin değil). Gölge pişirilmemiş, ışık sol-üstten (gözle).
