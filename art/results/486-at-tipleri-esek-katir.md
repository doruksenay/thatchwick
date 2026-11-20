unity-import: pending

# 486 At tipleri, eşek, katır, bardo: teslim yok (`failed`)

Sonuç: 7 pixflux tek karesi üretildi; hiçbiri güney kontrolünü geçmedi (hepsi yandan ya da 3/4 duruşta). Tek deneme kuralı gereği döndürme yapılmadı, Assets'e dosya konmadı. Masaüstü kopyası: yerel (bulut modu).

Yan yana sayfası (tek kareler, 5 kat): `art/results/486-yanyana.png` (sırayla light, medium, heavy, pony, donkey, mule, hinny). Stil denetimi yapılmadı (teslim edilen dosya yok).

## Araç ve ayarlar
- 375/484 tarifi: `create_image_pixflux`, `view=high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading; palet zorlaması `iso_animal_sheep_1_cand_se.png`. Açıklama eki: 375 eki + `neutral grey-brown coat, plain coat without markings or spots` + `standing directly on transparent background, no ground disc, no floor shadow, no grey patch under the paws`. Tuvaller: tarife göre (56/56/56/44/44/56/48).
- Hız sınırı hatası olmadı. Her çağrı 1 gen; toplam 7. Bakiye sonrası: 2798,9.

## Tek kare kontrolü
| Tip | Tuval | Opak bbox (x0,y0,x1,y1) | Renk | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|---|---|
| horse_light | 56 | 9,13,49,47 | 9 | yandan, yürür pozda | yok | döndürülmedi |
| horse_medium | 56 | 6,7,49,52 | 8 | 3/4 | yok | döndürülmedi |
| horse_heavy | 56 | 8,1,46,55 | 10 | 3/4 (beyaz alev ve çorap: desen çıktı) | yok | döndürülmedi |
| pony | 44 | 6,1,39,41 | 9 | 3/4 yandan | yok | döndürülmedi |
| donkey | 44 | 6,2,40,42 | 9 | 3/4 | var (koyu gri disk) | döndürülmedi |
| mule | 56 | 7,6,48,48 | 10 | 3/4 yandan (beyaz burun) | yok | döndürülmedi |
| hinny | 48 | 6,3,43,45 | 10 | 3/4 yandan | var (koyu gri disk) | döndürülmedi |

Tek kareler: `art/results/486-red/tek_<tip>.png`. Biçim açısından tipler iyi ayrışıyor (hafif at ince ve uzun bacaklı, ağır at iri, midilli kısa, eşek uzun dik kulaklı, katır uzun kulaklı ve at boyunda, bardo daha küçük ve dolgun yeleli); asıl sorun duruş.

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x7 | 7 |
| Toplam | 7 |

`get_balance` sonrası: 2798,9.

## Sorunlar ve öneri (tahmin)
- 7/7 tek kare yandan ya da 3/4 çıktı: pixflux, at gövdesini "önden" tarifine rağmen yan çiziyor. 484'teki köpek sonucu ile aynı (6/7 yandan). v3 güney girdisi gerektirdiğinden döndürülemedi.
- Öneri: `create_character mode=pro` ile bu kareleri `reference_image_url` olarak döndürmek (pro her yöne bakan sprite'ı döndürür, 10 gen/tip), ya da `template=horse` ile `standard`/pro doğrudan 8 yön üretimi. Ayrı iş olarak yazılmalı.
- Desen çıktısı: heavy (beyaz alev ve çorap), mule (beyaz burun), light (karında açık leke) düz tüy kuralına uymadı; palet değişimi için düz gövde gerekir.
