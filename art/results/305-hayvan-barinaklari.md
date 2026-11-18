unity-import: pending

# 305 Dalga 1: hayvan barınakları (rapor)

Yan yana sayfası: [305-yanyana.png](305-yanyana.png). Teslim edilmeyen ilk kümes denemesi: `art/results/305-red/iso_item_coop_2x2_se_d1.png`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py --m` zaten vardı; araç değiştirilmedi), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki + takım eki (Köylü: kümes, kulübe, kedi sepeti; Zanaatkâr: ahır, arı kovanı). 5 paralel çağrı + kümes ikinci deneme = 6 generation (cost_max 12). `get_balance` iş sonu: 775,1 (paralel şeritler de harcıyor; bu işin payı 6).
Kümes 1. deneme: zemin çimeni (yeşil piksel 2217), 52 renk, palet dışı (stil KALDI), köpek kulübesi gibi. 2. deneme açıklaması: `chicken coop: ... on short stilts ... no grass, no green, no ground`.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_coop_2x2_se.png | 144x120 | 72 | 40 |
| iso_item_barn_3x2_se_nw.png | 176x152 | 88 | 48 |
| iso_item_kennel_se.png | 80x80 | 40 | 24 |
| iso_item_cat_basket_se.png | 80x56 | 40 | 24 |
| iso_item_beehive_se.png | 80x88 | 40 | 24 |

## Doğrulama
| Dosya | Renk | Yarı saydam | Alta değme | Maske dışı | Kılavuz kalıntısı | Ayak izi kaplaması sol/sağ | Parlaklık sol/sağ | validate_iso |
|---|---|---|---|---|---|---|---|---|
| kümes | 37 | 0 | yok (en alt y=98, taban ucu 111) | 0 | 0 | %71/%61 ⚠ | 62/89 ⚠ | geçti |
| ahır | 27 | 0 | yok | 0 | 0 | %84/%96 | 78/58 | geçti |
| kulübe | 36 | 0 | yok | 0 | 0 | %100/%100 | 63/87 ⚠ | geçti |
| kedi sepeti | 34 | 0 | yok | 0 | 0 | %92/%92 | 91/94 | geçti |
| arı kovanı | 31 | 0 | yok | 0 | 0 | %52/%52 (ince ayaklı sehpa) | 59/57 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| kümes (2. deneme) | %100 | 8.7 | x0.86 | GEÇTİ |
| kümes (1. deneme, teslim değil) | %68 | 18.7 | x1.08 | KALDI |
| ahır | %92 | 7.0 | x0.84 | GEÇTİ |
| kulübe | %100 | 6.6 | x0.86 | GEÇTİ |
| kedi sepeti | %95 | 11.7 | x0.99 | UYARI (palete uzak) |
| arı kovanı | %86 | 9.2 | x1.15 | GEÇTİ |

## Sorunlar (kullanıcı kararı)
- Ahırda zemin parçası (toprak ve çim, 693 yeşil piksel) pişmiş; "no ground" çiğnendi, ikinci deneme koşulu olmadığı için teslim edildi.
- Kümes bir ambar/kulübe gibi okunuyor; kümes ayak izini %71/%61 dolduruyor, tabanın alt ucuna değmiyor. Rampa var, çit yok. İkinci deneme hakkı bitti.
- Kümes ve kulübede ışık sağ yüzü daha parlak gösteriyor (sol-üst kuralına ters).
- Arı kovanı sarı kubbe + sehpa olarak çıktı, hasır kovan olarak zayıf okunuyor.
- Kedi sepeti yassı hasır tepsi, minder yok.
- Açık ayak izi taşması, yarı saydam piksel, kılavuz kalıntısı yok. Kırpma yapılmadı.
