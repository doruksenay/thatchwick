unity-import: pending

# 797 Servis kapları, katmanlı (rapor): 3 kap + 10 dolu katman (`_4`) teslim; `_3…_1` yapılmadı

Yan yana sayfası: [797-yanyana.png](797-yanyana.png). Kap + `_4` şeridi (üst sıra boş kap, alt sıra kap+yemek; sıra: pilav, çorba, kızartma, balık, börek, ekmek, tatlı, salata, kahvaltı, kızartma): [797-serit.png](797-serit.png). Ham hâller ve pilav `_3` denemesi: `art/results/797-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, `high top-down`, `basic shading`, `low detail`, `single color outline`, tuval 32x32 (578 ile aynı). Kap: 24x12 elmas kılavuz + dikdörtgen maske. Yemek: arka plan = ilgili kap (tepsi/çömlek var olan `_0`, yeni kaplar bu işten), maske = kabın iç yüzeyi (elips ya da kabın 2 px aşındırılmış silueti), açıklama `only the food, …`.
- Hepsi tek denemeyle: 3 kap + 10 `_4` + 1 pilav `_3` = 14 çağrı. İki çağrı (balık, salata) base64 aktarım hatasıyla reddedildi (ücret yok), görsel yeniden kodlanıp bir kez yeniden gönderildi.
- Doygunluk: ham çıktılar aileden çok doygun (x1.3-2.1, KALDI). `style.py fit` ile x0.55-0.86'ya indirildi; şekiller değişmedi. Teslim edilen hâl fit'li; ham hâller `797-red/*_ham.png`. Tahta kap ve tatlı katmanı fit gerektirmedi.
- Katman çıkarma tuttu: çıktı yalnızca yemek (kap pikseli yok; araç maske dışını vermiyor). Kap üstüne (0,0)'da bindirilince kaba oturuyor.

## Sonuç ve durdurma nedeni
| Parça | Durum |
|---|---|
| 3 yeni kap (tahta, oval tabak, tahta kase) | teslim |
| 10 yemek `_4` | teslim (zayıf olanlar aşağıda) |
| `_3`, `_2`, `_1` (30 katman) | **YAPILMADI** |

Pilav `_3` denemesi (arka plan: tepsi + `_4`, aynı maske, "üçte dört kadar, daha alçak yığın"): çıktı `_4` ile aynı sınır kutusunda (7,9,25,22), maskeyi baştan sona dolduruyor; yalnızca doku değişti (`797-red/iso_serve_food_pilaf_3_ayni-alan.png`). Model maskeyi dolduruyor, doluluk azalmıyor; tarifteki "önce tek birleşim, tutmazsa dur" gereği kalan 29 çağrı yapılmadı.

## Dosyalar (Assets/Art/Iso/_aday/), pivot x 16, pivot y 10 (alttan; masa üstü eşyası, 578 ile aynı)
| Yol | Piksel |
|---|---|
| iso_serve_vessel_board.png | 32x32 |
| iso_serve_vessel_platter.png | 32x32 |
| iso_serve_vessel_bowl_wood.png | 32x32 |
| iso_serve_food_{pilaf,soup_red,roast,fish,pie,bread,sweet,salad,breakfast,fry}_4.png | 32x32 |

Katmanlar kapla aynı tuvalde, kap (0,0) konumunda.

## Doğrulama (teslim edilen fit'li hâller)
| Dosya | Palete yakın | Uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| vessel_board | %100 | 7.1 | x0.97 | GEÇTİ |
| vessel_platter | %100 | 8.0 | x1.14 | GEÇTİ |
| vessel_bowl_wood | %100 | 6.8 | x1.14 | GEÇTİ |
| food_pilaf_4 | %100 | 9.3 | x1.15 | GEÇTİ |
| food_soup_red_4 | %93 | 9.8 | x1.16 | GEÇTİ |
| food_roast_4 | %100 | 7.8 | x1.14 | GEÇTİ |
| food_fish_4 | %100 | 7.1 | x1.15 | GEÇTİ |
| food_pie_4 | %100 | 8.4 | x1.14 | GEÇTİ |
| food_bread_4 | %100 | 7.8 | x1.15 | GEÇTİ |
| food_sweet_4 | %100 | 7.0 | x0.96 | GEÇTİ |
| food_salad_4 | %90 | 11.8 | x1.15 | UYARI (palete uzak) |
| food_breakfast_4 | %100 | 6.5 | x1.24 | GEÇTİ |
| food_fry_4 | %100 | 7.2 | x1.14 | GEÇTİ |

`validate_iso.py --kind object` örneklendi (tahta, balık: geçti, 8-9 renk). Opak sınırlar kap içinde; gölge, buhar, el yok. Zemin değil, döşeme testi gerekmiyor.

## Sorunlar
- ⚠ Doluluk kademesi (`_3…_1`) bu yöntemle üretilemiyor (yukarıda).
- ⚠ Zayıf `_4`: balık tek bir sosis gibi uzun nesne ("yan yana balıklar" yok); tatlı tahta rengine çok yakın (kontrast düşük); ekmek ve börek düz dikdörtgen bloklar; kızartma iki üç iri parça (yığın değil). Çorba yüzeyinde beyazımsı parçalar var. Kullanıcı `797-serit.png`'ye bakmalı.
- Yeni kaplardan oval tabak kırmızımsı (kiremit gibi), tahta kase derin bir kova gibi; ikisi de boş.

## Sorular
- `_3…_1` için seçenek: (a) maskeyi doluluğa göre küçültmek (ör. `_3` %75, `_2` %50, `_1` %25 alanlı elips) ve `_4` yerine boş kabı arka plan almak; (b) katmanı oyunda `_4` katmanının ölçeklenmesi/kırpılması ile doluluk göstermek (kod tarafı); (c) bu birleşimleri tek kare yapmak. Karar kullanıcıda; (a) için yeni iş dosyası yazılabilir (~30 çağrı).
