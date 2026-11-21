unity-import: pending

# 482c Gelgit havuzu ve deniz kumu yeniden (rapor)

Sonuç: **2/2 teslim** (`_aday/`, `_se_v3`). Yan yana: [482c-yanyana.png](482c-yanyana.png) (zemin referansları + yeni ikisi; 482/482b sıraları `482b-yanyana.png`'de). Ham hâller: `art/results/482c-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, 80x72 kılavuz+maske (`footprint_guide.py`), high top-down, low detail, basic shading, **lineless** (dış çizgi yok). 3 çağrı x 1 = 3 generation (havuz 2 deneme, max_attempts 2).
- Havuz 1. deneme: kahverengi kılavuz yüzünden yine taş çerçeveli kutu çıktı, teslim edilmedi (`482c-red/pool_try1_kutu.png`). 2. deneme: kılavuz kum rengine boyandı; çerçeve yok.
- Kum: ham sonuç KALDI (doygunluk x2.67); `style.py fit` ile x0.43'e indirildi, GEÇTİ.
- get_balance sonrası: 9686 (paralel şeritler de harcıyor).

## Stil denetimi (zemin)
| Parça | Sonuç | Palete yakın | Doygunluk | Not |
|---|---|---|---|---|
| tidepool_v3 | UYARI | %99 | x0.37 | ailesinden soluk |
| sea_sand_v3 | GEÇTİ | %100 | x1.16 | fit uygulandı |

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_forage_tidepool_se_v3.png | 80x72 | 40 | 24 |
| Assets/Art/Iso/_aday/iso_forage_sea_sand_se_v3.png | 80x72 | 40 | 24 |

## Doğrulama
validate_iso (object): ikisi geçti; renk 17/19, alt boşluk 8/14. Yeşil piksel oranı havuz %2,5 (< %8, yalnız kaya üstü yosun kümeleri); kum kenarı koyu piksel oranı %0 (< %5). Döşeme testi nesne olduğu için yapılmadı.

## Sorunlar
- Havuz hâlâ düz bir levha/ova biçiminde (karo sınırını dolduruyor, kum aralardan az görünüyor); yükseklik düşük ama "kayalar arasında kum" tam değil. Yeşil yosun iki yuvarlak küme gibi okunabilir. Kullanıcı kararı gerekir.
- Kumda deniz kabukları küçük, 3 değil 2-3 kırmızımsı işaret.
