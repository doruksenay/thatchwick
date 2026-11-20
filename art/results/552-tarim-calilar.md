unity-import: pending

# 552 Yeni çalılar: ahududu, frenk üzümü, kına (rapor)

Yan yana sayfası: [552-yanyana.png](552-yanyana.png) (referanslar + 9 yeni, hepsi fit sonrası). Teslim edilmeyenler: `art/results/552-red/` (fit ve ham). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (80x72, pivot 40,24; yükseklikler 15/30/14/28/17/34), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarif metni + `, no face, no eyes, no creature, no person`. Çağrı başı 1 gen = **9 generation** (cost_max 18). İki frenk üzümü çağrısı base64 aktarım hatasıyla başlamadan reddedildi (ücret yok); maske yeniden kodlanıp gönderildi (deneme sayılmadı). `get_balance` sonrası: 1997,7 (paralel şeritler de harcıyor). Hepsine `style.py fit --family bitki` uygulandı (x0,40-0,75: ham çıktılar çok doygundu).

## Teslim edilenler (4/9), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Stil (bitki) | validate_iso |
|---|---|---|---|---|---|---|
| iso_bush_currant_grown_se.png | 80x72 | 40 / 24 | x8-71, y7-59 | 55 | GEÇTİ (%99) | geçti |
| iso_bush_currant_fruit_se.png | 80x72 | 40 / 24 | x8-71, y4-63 | 35 | GEÇTİ (%92) | geçti |
| iso_bush_henna_grown_se.png | 80x72 | 40 / 24 | x15-64, y11-54 | 41 | GEÇTİ (%99) | geçti |
| iso_bush_henna_fruit_se.png | 80x72 | 40 / 24 | x15-64, y9-54 | 49 | GEÇTİ (%99) | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok.

## Teslim EDİLMEYEN (5/9)
| Dosya | Stil | Neden |
|---|---|---|
| iso_bush_raspberry_young_se | UYARI (%81) | çalı değil: pembe meyveli yeşil yaprak topakları, ahududu sapı yok |
| iso_bush_raspberry_grown_se | KALDI (%78) | **yüz/göz var** (pembe turp gibi yaratıklar), "no face" ihlali |
| iso_bush_raspberry_fruit_se | KALDI (%75) | uzun saplarda pembe toplar, ahududu çalısı gibi değil |
| iso_bush_currant_young_se | KALDI (%78) | mavi-mor böğürtlen kümesi; frenk üzümü genç çalısı değil |
| iso_bush_henna_young_se | GEÇTİ (%100) | dibinde kahverengi saksı/tepsi gibi taban |

## Sorunlar
- Ahududu üç evresi de yok; tek deneme kuralı gereği ikinci deneme yapılmadı. Öneri: ahududu için sade tanım ve taban uyarısıyla ("floating alone, no soil, no pot") yeni iş.
- Genç evreler (frenk üzümü, kına) yok; yeni iş gerekir.
- Opak yükseklikler tarif dışı (olgun 44-56 px, hedef 28-34); kod tarafı ölçek gerektirebilir. Frenk üzümü meyveli, ağaç gibi gövdeli okunuyor.
