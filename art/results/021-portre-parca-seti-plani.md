unity-import: pending

# 021 Portre parça seti planı: sonuç

Plan işi, PixelLab çağrılmadı (0 gen). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Yöntem kararı
020 raporuna göre katman yöntemi (taban `source_image_id` + "only changes" + maske) **tuttu**: maske dışı 0 piksel, dikişsiz birleşim. Bu yüzden parça parça katman planı yazıldı.
İki bilinen sorun işlere gömüldü: (1) 020 tabanı sakallı çıktı; 021a/b tarifinde "clean shaven" ve tek yeniden deneme var. (2) Doygunluk sapması; tabanlar gri tonlu ten çizilir.

## Yazılan işler
| İş | İçerik | Parça | cost_max | Bağımlılık |
|---|---|---|---|---|
| 021a | Yüz tabanı: oval, yuvarlak, köşeli × 4 yaş | 12 | 36 | 020 |
| 021b | Yüz tabanı: uzun, kalp × 4 yaş | 8 | 24 | 020 |
| 021c | Göz şekli (ak + iris) | 8 | 40 | 021a |
| 021d | Kaş 6 + burun 6 | 12 | 60 | 021a |
| 021e | Ağız 6 + kulak 3 | 9 | 45 | 021a |
| 021f | Saç ön katman 1-10 | 10 | 50 | 021a |
| 021g | Saç ön katman 11-20 | 10 | 50 | 021a |
| 021h | Saç arka katman 1-10 | 10 | 50 | 021f |
| 021i | Saç arka katman 11-20 | 10 | 50 | 021g |
| 021j | Kâkül 6 + bıyık 5 | 11 | 55 | 021a |
| 021k | Sakal 6 + küpe 4 + burun halkası + ip kolye | 12 | 60 | 021a |
| 021l-o | Yara izi: yarık, çizik, yanık, ısırık (8 bölge) | 4×8 | 4×40 | 021a |
| 021p | Dövme, yüz (8 desen) | 8 | 40 | 021a |
| 021q | Dövme, boyun (8 desen) | 8 | 40 | 021a |
| 021r | Makyaj 4 + kırışıklık + eksik diş + sargı | 7 | 40 | 021a |

18 iş, 167 parça, toplam `cost_max` 800 gen (bakiye ~1789; rezerv 200 korunur). Frenler (60 gen ve 3 iş/çalışma) nedeniyle işler birkaç turda, gerekirse sonraki dönemde akar.

## Notlar
- 021b, 021a'nın tabanlarına bağlı değil (yalnızca 020); diğer parçalar oval yetişkin tabanı için 021a'ya bağlı.
- Saç stili adları (20 adet, `short_crop` ... `coif_hair`) burada tanımlandı; 017 çocuk işleri henüz yazılmadığından adlar 017 yazılınca eşlenmeli.
- Saç "ön + arka": listedeki 20 parça ön ve arka ayrı sayılınca 40 dosya (200 gen). Arka katman işleri (021h, 021i) kullanıcı isterse silinebilir.
- Göz ak/iris tek PNG; iris sabit `#808080` tonla ayrılır.
- Parça başına 5 gen varsayımı 020 ölçümüne dayanır.

## Doğrulama
`queue_ready.py --all` 18 yeni işi hatasız ayrıştırıyor; hepsi `cost_max <= 60`; numara çakışması yok.
