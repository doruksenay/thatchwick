unity-import: pending

# 482b Deniz kıyısı toplama düğümleri yeniden (rapor)

Sonuç: **3/3 teslim** (`_aday/`, `_se_v2` ekli). Yan yana (sağdaki üçü reddedilen 482): [482b-yanyana.png](482b-yanyana.png). Ham hâller: `art/results/482b-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme (kılavuz+maske 80x72, 1x1), high top-down, low detail, basic shading. 3 çağrı x 1 gen = 3 generation. İki çağrı hız sınırı hatası verdi; kurala göre bekleyip bir kez yeniden denendi, ikisi de tuttu. Havuz tarifi: doğal ıslak gri-kahve kayalar, sığ su, yosun, midye, deniz yıldızı, aralardan kum. Pivot x=40, y(alttan)=24. get_balance iş sonu: 2731,90 (paralel şeritler de harcıyor; bu işin payı 3).

## Stil denetimi (zemin, kilit kurulmadı)
| Parça | Sonuç | Palete yakın | Doygunluk | Not |
|---|---|---|---|---|
| tidepool | UYARI | %61 | x1.15 | fit x0.89 uygulandı; palet dışı renkler (su mavisi) |
| driftline | GEÇTİ | %100 | x1.15 | fit x0.72 (ham KALDI x1.59) |
| sea_sand | GEÇTİ | %100 | x1.12 | fit x0.53 (ham KALDI x2.16) |

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_forage_tidepool_se_v2.png | 80x72 | 40 | 24 |
| Assets/Art/Iso/_aday/iso_forage_driftline_se_v2.png | 80x72 | 40 | 24 |
| Assets/Art/Iso/_aday/iso_forage_sea_sand_se_v2.png | 80x72 | 40 | 24 |

validate_iso (object): üçü geçti; renk 45/36/25, alt boşluk 7/19/12.

## Sorunlar
- tidepool mor kutu değil, doğal kayalık havuz; mor yok. Ama kaya halkası karoyu geniş dolduruyor ve yosun yeşil çalı gibi okunuyor; kum aralardan az görünüyor.
- driftline ve sea_sand zemin levhası olmadan kum ovali olarak geldi; sea_sand'de iki kabuk seçilir.
- Döşeme testi nesne olduğu için yapılmadı.
