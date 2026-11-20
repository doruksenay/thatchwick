unity-import: pending

# 482 Deniz kıyısı toplama düğümleri (rapor)

Sonuç: **3/3 teslim** (`_aday/`). Yan yana: [482-yanyana.png](482-yanyana.png). Ham hâller (doygunluk düzeltmesi öncesi): `art/results/482-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, `footprint_guide.py` kılavuz+maske (80x72, 1x1), high top-down, low detail, basic shading, single color outline, ortak ek (sol-üst ışık, muted natural colours, no ground tile, no shadow). 3 çağrı x 1 gen = 3 generation (paralel, ilk denemede). Pivot x=40, y(alttan)=24. get_balance iş sonu: 2896,90 (paralel şeritler de harcıyor; bu işin payı 3).

## Stil denetimi (zemin, kilit kurulmadı: palet uyarı)
| Parça | Sonuç | Palete yakın | Doygunluk | Not |
|---|---|---|---|---|
| tidepool | UYARI | %64 | x1.08 | taş havuz bloğu, su ve yosun tanınır |
| driftline | GEÇTİ (fit sonrası; ham KALDI x1.90) | %100 | x1.15 | doygunluk x0.60 |
| sea_sand | GEÇTİ (fit sonrası; ham KALDI x1.87) | %100 | x1.15 | doygunluk x0.61 |

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_forage_tidepool_se.png | 80x72 | 40 | 24 |
| Assets/Art/Iso/_aday/iso_forage_driftline_se.png | 80x72 | 40 | 24 |
| Assets/Art/Iso/_aday/iso_forage_sea_sand_se.png | 80x72 | 40 | 24 |

validate_iso (object): üçü geçti; renk 42/26/25, yarı saydam 0; opak alt boşluk 7/12/7 px.

## Sorunlar
- Üçünün altında kalın elmas zemin bloğu (kum/taş levha) var; "no ground tile" tutmadı. Kıyıda levha kuma karıştığı için kabul edilebilir olabilir; kullanıcı yan yana sayfasına bakıp karar versin. Döşeme testi nesne olduğu için yapılmadı.
- sea_sand yalnızca ince daireli kum tepesi; kabuklar seçilmiyor. driftline yosun kahverengi-kırmızı, odun parçası belirsiz.
- Doygunluk düzeltmesi (`style.py fit`) iki parçada uygulandı.
