unity-import: pending

# 453 Ev bitkileri: büyük (rapor): 0/6 teslim, failed

Yan yana sayfası: [453-yanyana.png](453-yanyana.png). Teslim edilmeyenler: `art/results/453-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme (302/306 ile aynı yöntem): yer bitkileri `high top-down`, duvar bitkileri `side`; `low detail`, `basic shading`, `single color outline`. Kılavuz: `footprint_guide.py` (saksı 80x80 yükseklik 44, havuz 80x56 yükseklik 20), duvar için "/" paralelkenar kılavuz (ivy 40x56, pencere çiçekliği 64x32). Paralel 6 çağrı, 1 gen = 6 generation (cost_max 14 içinde). Tek deneme.
`get_balance` iş sonu: 3086,9 (paralel şeritler de harcıyor).

## Sonuç
Altısı da stil denetiminde KALDI (palet dışı renkler: bitki yeşili/pembe/beyaz çiçek). Doygunluk dışında palet sorunu olduğu için `fit` uygulanmadı; teslim yok.

| Dosya | Piksel | validate_iso | Palete yakın | Ort. uzaklık | Doygunluk | Stil |
|---|---|---|---|---|---|---|
| pot_bay_se | 80x80 | geçti (22 renk) | %48 | 18,6 | x1,20 | KALDI |
| pot_myrtle_se | 80x80 | BAŞARISIZ (55 renk > 48) | %34 | 31,7 | x1,51 | KALDI |
| pot_rose_se | 80x80 | BAŞARISIZ (54 renk > 48) | %52 | 28,2 | x1,53 | KALDI |
| water_garden_se | 80x56 | geçti (37) | %67 | 16,9 | x0,69 | KALDI |
| ivy_wall_sw_ne | 40x56 | geçti (15) | %59 | 19,3 | x1,61 | KALDI |
| window_flower_box_sw_ne | 64x32 | geçti (26) | %65 | 18,5 | x0,94 | KALDI |

Hepsi şekil olarak tanınıyor görünüyor (kullanıcı yan yana sayfasından bakabilir); kabul edilirse `453-red/` içinden elle alınabilir.

## Sorular
- Bitki ürünleri için esya paleti yetersiz (yeşil, pembe, beyaz çiçek). Öneri: bitkiler için palete yeşil/çiçek tonları eklemek ya da bitki alt-ailesi kilidi; ardından 453 yeniden kuyruğa alınabilir.
