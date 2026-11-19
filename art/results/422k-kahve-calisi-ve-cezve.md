unity-import: pending

# 422k Kahve calisi evreleri ve cezve (rapor)

Yan yana sayfalari: [422k-yanyana.png](422k-yanyana.png) (3 calı, bitki referanslari), [422k-yanyana-cezve.png](422k-yanyana-cezve.png) (cezve, esya referanslari). Teslim edilmeyen: `art/results/422k-red/` (cezve ham ve fit). Masaustu kopyasi: yerel.

## Arac ve ayarlar
`create_map_object`, stil esleme, `footprint_guide.py` kilavuzu + maske (80x72, pivot 40,24; calı yukseklik 34, cezve 14), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki aciklamalar + `, no face, no eyes, no creature, no person`. Cagri basi 1 gen.
Cagrilar: 4 (3 calı + cezve) = **4 generation** (cost_max 12). Cezve ilk cagrida hiz siniri hatasi (rate limit) aldi, 30 sn sonra bir kez yeniden denendi (tek istisna); hatali cagri ucretsiz. `get_balance` sonrasi: 3672,5 (paralel seritler de harciyor).
Calılara `style.py fit` uygulandi. **Aile sapmasi:** is dosyasi `esya` der; `esya` ailesinde fit sonrasi bile palet disi (%32-41) KALDI. 374'teki gibi `bitki` ailesi ile fit (x0,45/0,38/0,42 -> aile hedefi 0,359) ve denetim yapildi: UYARI (bitki kilidi kurulmadi).

## Teslim edilenler (3/4), Assets/Art/Iso/_aday/
| Dosya | Boyut | Pivot x / y (alttan) | Opak sinir | Renk | Doygunluk (fit sonrasi) | Stil (bitki) | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_plant_coffee_young_se.png | 80x72 | 40 / 24 | x11-67, y5-60 | 42 | x1,06 | UYARI (%51, uzaklik 16,7) | gecti |
| iso_plant_coffee_grown_se.png | 80x72 | 40 / 24 | x10-69, y7-55 | 47 | x1,15 | UYARI (%52, uzaklik 21,1) | gecti |
| iso_plant_coffee_fruit_se.png | 80x72 | 40 / 24 | x8-70, y6-63 | 42 | x1,15 | UYARI (%50, uzaklik 20,8) | gecti |

Yari saydam piksel 0, golge pisirilmemis.

## Teslim EDILMEYEN
| Dosya | Stil (esya) | Neden |
|---|---|---|
| iso_item_cezve_se | KALDI (ham %74 palete yakin, uzaklik 13,7, x1,14; fit sonrasi ayni) | palet disi renkler (bakir/altin ton); sekil taninir: kisa saplı demlik gibi, tas altlik uzerinde. Kullanici `422k-red/`e bakabilir |

## Sorunlar
- Cezve: cok kisa sap, cezveden cok demlik/cayday gorunuyor; ikinci deneme yapilmadi (yeniden deneme yok kurali).
- Opak yukseklikler kilavuzdan buyuk: genc 56 px, olgun 49 px, kirazlı 58 px (hedef 34); model kilavuz yuksekligine uymadi. Olgun calı yatay yaprak demeti, govdesiz (dibinde toprak yok); genc ve kirazlı govdeli, kirazlı tanınıyor. Kod tarafi olcek gerektirebilir. Evre buyumesi (genc 56 > olgun 49) tutarsiz okunabilir: kullanici yan yana sayfasina bakmali.
- Zemin parcasi olmadigindan dosemeyi testi yok.
- Oneri: cezve icin yeni is (basit "iron" ya da "tin" kısa tanım, `bitki` degil `esya`).
