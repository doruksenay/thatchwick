unity-import: pending

# 397 Çiftlik yeniden (rapor)

Yan yana sayfası: [397-yanyana.png](397-yanyana.png). Ham domuz bölmesi: `art/results/397-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`; harman 208x120 n3 m3 h16, bölme 112x96 n2 axis y h30, sarnıç 144x96 n2 m2 h10, düven 112x64 n2 axis x h12), `high top-down`, `low detail`, `basic shading`, `single color outline`. 4 çağrı x 1 generation = 4 (cost_max 14). Tarif metinleri iş dosyasındaki gibi; harman yerinde ek yok, bölme ve düvende Köylü eki (`rough light pine wood`). `get_balance` iş sonu: 4384,90 (paralel şeritler de harcıyor; bu işin payı 4).
Bölme için 305'teki ahır bölmesi görseli arka plan olarak verilmedi (base64 aktarımında bozulma riski); yalnızca kılavuz kullanıldı, metinde "stall" tarifi var.
Domuz bölmesi: ham çıktı stil KALDI (doygun x1.46) ve 49 renkti. `style.py fit` ile doygunluk x1.17'ye indi, 49 renk en yakın iki rengi birleştirerek (128,86,56 -> 125,82,57, 91 piksel) 48'e indirildi. Ham hâli `397-red/`de.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_threshing_floor_3x3_se.png | 208x120 | 104 | 56 |
| iso_item_pig_pen_1x2_se.png | 112x96 | 56 | 32 |
| iso_item_underground_cistern_2x2_se.png | 144x96 | 72 | 40 |
| iso_item_threshing_sledge_b_2x1_se_nw.png | 112x64 | 56 | 32 |

## Doğrulama (validate_iso --kind object, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| harman | x19-188, y11-101 | 27 | 0 | geçti |
| bölme | x8-103, y9-88 | 48 | 0 | geçti (fit + renk birleştirme sonrası) |
| sarnıç | x8-135, y13-88 | 17 | 0 | geçti |
| düven | x13-98, y9-56 | 41 | 0 | geçti |

Harman yeri 3x3 nesne olarak doğrulandı (zemin 64x32 denetimi uygulanmaz); döşeme testi yapılmadı. Gölge ve ışık yönü sayıyla ölçülmedi.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| harman | %92 | 14.9 | x1.48 | UYARI (palete uzak, doygun) |
| bölme (fit) | %97 | 10.4 | x1.17 | UYARI (palete uzak) |
| sarnıç | %100 | 10.0 | x0.33 | UYARI (soluk; tarifteki koyu su) |
| düven | %95 | 9.0 | x1.29 | UYARI (doygun) |

## Sorunlar / kullanıcı kararı
- Harman yeri: yuvarlak yerine elips; toprak üzerinde sarı eş merkezli halkalar (saman), taş halkası belirgin değil. Kullanıcı bakmalı.
- Bölme: ahır bölmesine benziyor (yan duvarlar, saman zemin, tekne); domuza özgü bir ayrım yok.
- Sarnıç: taş kenar ve koyu su tarife uygun; doygunluk düşük.
- Düven: gri çakmaktaşı dişleri görünür (kullanıcı bakmalı); eğik değil.
- `art/tmp397/` commit edilmez.
