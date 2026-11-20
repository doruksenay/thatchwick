unity-import: pending

# 498 Kılıçtan Saban masa üstü dekoru (rapor)

**Sonuç: FAILED, teslim edilen dosya yok.** Masaüstü kopyası: yerel. Reddedilen denemeler: `art/results/498-red/`.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (383 yöntemi: 32x32 tuval, taban elması 16x8 alt kenardan 4 px yukarıda, maske eşya yüksekliği + 1 px). `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 2 çağrı x 1 gen = 2 generation (cost_max 3, max_attempts 2). Aracın seed parametresi yok; ikinci deneme aynı girdiyle. `get_balance` sonrası: 2352,70 (paralel şeritler dahil). Tuval 32x24 yerine 32x32 (araç alt sınırı 32).

## Denemeler
| # | Opak kutu | Renk | validate_iso | Değerlendirme |
|---|---|---|---|---|
| 1 | x12-19 y14-23 (8x10) | 5 | geçti | Tanınmıyor: kaya/mantar benzeri yuvarlak leke, tabana değmiyor (alt boşluk 8), ayak izini doldurmuyor |
| 2 | x13-19 y12-28 (7x17) | 3 | geçti | Dikey duran kılıç silüeti (kabza + siper); yatay saban demiri değil, silah gibi; 16 px taban genişliğinin yarısından azını dolduruyor |

İkisi de iş dosyasının dönem ölçütüne (tehditkâr silah gibi görünmemeli, yatık saban demiri) uymuyor; ikinci deneme hakkı da kullanıldı. Teslim edilmedi. Stil denetimi ve yan yana sayfası üretilmedi (teslim edilen dosya yok).

## Öneri
Yeni tarif: "triangular iron plough blade lying flat" (kılıç ima etmeden), geniş yatay taban için 40x32 tuval ve 20x10 taban elması. Kılıç kabzası ipucu bu küçük boyutta silah siluetine kayıyor.
