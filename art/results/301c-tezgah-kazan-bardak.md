unity-import: pending

# 301c Pazar tezgâhı, kazan, toprak bardak (rapor)

Yan yana sayfası: [301c-yanyana.png](301c-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`; bardak için 32x32 dekor kılavuzu, taban 12x6 elmas, maske 1 px genişletilmiş), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 3 çağrı x 1 gen = 3 generation, hepsi ilk denemede, paralel. `get_balance` sonrası: 3765,5 (paralel şeritler de harcıyor).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_market_stall_2x1_se_nw_v3.png | 112x136 | 56 | 32 |
| iso_item_cooking_pot_se_v3.png | 80x80 | 40 | 24 |
| iso_item_clay_cup_se_v2.png | 32x32 | 16 | 4 (taban elması merkezi) |

## Doğrulama
| Dosya | Opak kutu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| tezgâh | x10-101, y7-124 | 35 | 0 | geçti |
| kazan | x16-63, y8-58 | 59 | 0 | renk 59 > 48 (301 kuralı: reduce_colors yapılmaz, yalnızca raporlandı) |
| bardak (3 px aşağı kaydırıldı) | x9-22, y13-25 | 5 | 0 | geçti; ham en alt satır y=22 tabana değmiyordu, taşıma sonrası y=25 (taban alt yarısı 25-27) |

## Stil denetimi (esya)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| tezgâh | %95 | 9.8 | x1.32 | UYARI (ailesinden doygun) |
| kazan | %98 | 10.8 | x0.90 | UYARI (palete uzak) |
| bardak | %100 | 5.2 | x1.29 | UYARI (ailesinden doygun) |

## Gözlem
- Tezgâh: ochre/bej çizgili tente, sepet elma, ekmek, testi, peynir; canlı ve pazar yeri gibi okunuyor. Tente 301b'ye göre daha açık.
- Kazan: üçayak üzerinde asılı, geniş mavi-siyah gövde, kepçe görünür; 301b'ye göre belirgin büyük. Yüzey mavimsi koyu demir.
- Bardak: kahverengi kulplu bardak okunuyor, ancak hâlâ küçük (14x13 px).
- Masaüstü kopyası: yerel. Ham/ara dosyalar art/tmp301c (commit edilmez).

## Sorunlar
- Kazan renk sayısı 59 (sınır 48), UYARI'lar kullanıcı kararına bırakıldı.
