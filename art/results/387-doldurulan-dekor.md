unity-import: pending

# 387 Doldurulan dekor (rapor): 7/12 teslim, review

Yan yana sayfası: [387-yanyana.png](387-yanyana.png) (teslim edilen 7 eşya + 7 referans). Teslim edilmeyenler ve ham sürümler: `art/results/387-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, referanssız kılavuz + maske (Python, `art/tmp387/`, commit edilmez). 12 çağrı x 1 gen = 12 generation (cost_max 18). Tek deneme, hız sınırı olmadı. 12 çağrı paralel gönderildi.
- Yüzey eşyaları (1-5, 10-12): `high top-down`, tuval 32x32 (en az 32 px kuralı), tabanda tam 2:1 elmas (kılavuz #786046), maske = tabanın yükseklik kadar yukarı uzatılmışı + 1 px. Taban elması: sepetler/peynir tahtası 24x12, bal küpü ve sürahi 12x6, tuzluk 10x5, fındık kasesi 16x8, sebze sandığı 28x14; tabanın alt kenarı tuval altından 4 px yukarıda.
- Asılı eşyalar (6-9): `side`, tuval 32x40, üstte 6x3 asılma noktası, maske altındaki alan.
- `low detail`, `basic shading`, `single color outline`; açıklama + ortak ek + Köylü eki (Zanaatkâr peynir tahtası: koyu meşe, demir).
- Tuval boyutu tarifteki 20x24 vb. değerlerden büyük (araç alt sınırı 32).
- Doygunluk: 7 eşyanın ham hâli "ailesinden çok doygun" ya da palet dışıydı; `style.py fit` ile x0,52-0,87 indirildi. Ham hâller `387-red/*_ham.png`.
- get_balance sonrası: 4614,9 / 5000 (paralel şeritler de harcıyor; bu işin payı 12).

## Sonuç
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | meyve sepeti | teslim (fit) | GEÇTİ |
| 2 | ekmek sepeti | teslim (fit) | GEÇTİ |
| 3 | peynir tahtası | teslim | GEÇTİ |
| 4 | bal küpü | teslim | GEÇTİ; küp tanınıyor ama ahşap kepçe ve bal belirgin değil, gözle bakılmalı |
| 5 | tuzluk | teslim | GEÇTİ |
| 6 | soğan örgüsü | TESLİM EDİLMEDİ | çıktı örgü değil, tek dev soğan (yanlış nesne). Ham stil denetimi KALDI, `fit` sonrası geçti |
| 7 | sarımsak örgüsü | TESLİM EDİLMEDİ | çıktı tek dev sarımsak başı, örgü değil |
| 8 | kurutulmuş et | TESLİM EDİLMEDİ | KALDI: palet %51 (fit sonrası), palet dışı renkler |
| 9 | kuru meyve dizisi | TESLİM EDİLMEDİ | çıktı halka/çelenk, dizi değil (tanınmıyor) |
| 10 | mürver sürahisi | teslim (fit) | GEÇTİ |
| 11 | fındık kasesi | teslim (fit) | GEÇTİ |
| 12 | sebze sandığı | TESLİM EDİLMEDİ | validate BAŞARISIZ (51 renk > 48), palet %70, KALDI |

## Dosyalar (Assets/Art/Iso/_aday/; pivot = taban elması merkezi)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_fruit_basket_se.png | 32x32 | 16 | 10 |
| iso_item_bread_basket_se.png | 32x32 | 16 | 10 |
| iso_item_cheese_board_se.png | 32x32 | 16 | 10 |
| iso_item_honey_pot_se.png | 32x32 | 16 | 7 |
| iso_item_salt_cellar_se.png | 32x32 | 16 | 6 |
| iso_item_elderflower_jug_se.png | 32x32 | 16 | 7 |
| iso_item_nut_bowl_se.png | 32x32 | 16 | 8 |

## Doğrulama
| Dosya | Renk | Yarı saydam | Alt boşluk | Palete yakın | Ort. uzaklık | Doygunluk | Stil |
|---|---|---|---|---|---|---|---|
| fruit_basket (fit) | 35 | 0 | 2 | %94 | 7,9 | x1,15 | GEÇTİ |
| bread_basket (fit) | <=48 | 0 | 2 | %100 | 4,4 | x1,15 | GEÇTİ |
| cheese_board | 10 | 0 | 5 | %100 | 7,7 | x1,06 | GEÇTİ |
| honey_pot | 9 | 0 | 6 | %100 | 5,1 | x1,01 | GEÇTİ |
| salt_cellar | 7 | 0 | 6 | %100 | 4,1 | x0,92 | GEÇTİ |
| elderflower_jug (fit) | <=48 | 0 | 2 | %91 | 6,4 | x1,16 | GEÇTİ |
| nut_bowl (fit) | <=48 | 0 | 4 | %87 | 7,9 | x1,16 | GEÇTİ |

`validate_iso.py` yedisi için geçti (`--footprint` verilmedi, tuval 32x32 yüzey dekoru). Döşeme testi zemin olmadığı için uygulanmadı. Işık ve pişmiş gölge yalnızca gözle bakıldı.

## Sorunlar / öneri
- Asılı örgüler (soğan, sarımsak, kuru meyve) model tarafından tek büyük nesneye ya da halkaya dönüştü; `side` kılavuzu yetmedi. Tekrar için tarife "long vertical braid of many small bulbs" vurgusu eklenebilir ya da 32x56 gibi daha uzun tuval denenebilir (kullanıcı kararı). Et ve sebze sandığı da baştan: `387b` önerisi.
- Tüm eşyalar dolu hâl; boş hâl ayrı iş.
- Eşya → dosya eşlemesi ve pivotlar kod oturumu/kullanıcının işi.
