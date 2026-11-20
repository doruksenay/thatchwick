unity-import: pending

# 545 Yeni ekinler 6/7: kimyon, kişniş, anason, çörek otu (rapor)

Yan yana sayfası: [545-yanyana.png](545-yanyana.png). Reddedilenler: `art/results/545-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
374 yöntemi: `create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; maske yükseklikleri 10/22/32), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374 ek metni. Her çıktıya `style.py fit --family bitki` uygulandı (doygunluk x0,40-0,73 düşürüldü, yani ham çıktılar çok doygundu).
Çağrılar: 12 ilk deneme + 9 ikinci deneme = **21 gen** (cost_max 24). İlk denemede 9 görselde toprak/karo tabanı çıktı; ikinci denemede (yalnızca biçim için) metne "floating alone, no soil, no dirt, no ground" eklendi. Hız sınırı hatası yok. `get_balance` sonrası: 2132,7 (paralel şeritler de harcıyor).

## Teslim edilenler (8/12), `Assets/Art/Iso/_aday/`
Tümü 80x72, pivot x=40 / y(alttan)=24, yarı saydam piksel 0, zemin/tepsi yok, gölge pişirilmemiş. validate_iso geçti. Stil denetimi hepsinde UYARI (bitki ailesinin kilidi kurulmadı, palet dışı).
| Dosya | Opak sınır | Yükseklik (hedef) | Palete yakın | Doygunluk | Not |
|---|---|---|---|---|---|
| iso_crop_cumin_3_se.png (2. deneme) | x18-65, y7-47 | 41 (32) | %71 | x1,12 | pembe-beyaz çiçekler; ince çizgili tohum görünmüyor |
| iso_crop_coriander_1_se.png | x18-61, y23-53 | 31 (10) | %32 | x1,15 | yassı maydanoz benzeri yapraklar, tanınıyor ama hedeften büyük |
| iso_crop_coriander_2_se.png (2. deneme) | x15-64, y9-54 | 46 (22) | %33 | x1,15 | dantelli yapraklar + çiçek sapı, iyi |
| iso_crop_coriander_3_se.png (2. deneme) | x18-60, y9-54 | 46 (32) | %51 | x1,11 | beyaz şemsiyeler; yeşil yuvarlak tohum belirsiz |
| iso_crop_anise_1_se.png (2. deneme) | x29-50, y21-48 | 28 (10) | %32 | x1,15 | yuvarlak yapraklı fide |
| iso_crop_anise_3_se.png (2. deneme) | x20-57, y7-48 | 42 (32) | %33 | x1,15 | papatya benzeri beyaz çiçek; gri-yeşil tohum yok, anason olarak zayıf |
| iso_crop_nigella_1_se.png | x23-53, y26-48 | 23 (10) | %80 | x1,15 | fideden çok koyu küçük koza kümesi; zayıf |
| iso_crop_nigella_2_se.png | x12-70, y9-60 | 52 (22) | %33 | x1,16 | soluk mavi çiçekli, iyi; hedeften çok büyük |

## Teslim edilmeyenler (4/12), `545-red/`
- cumin_1: karo parçası üstünde küçük kopuk bitki parçası.
- cumin_2: çim karosu üstünde sırık bitki (zemin parçası).
- anise_2: tüy topu, altında karo izi, anason tanınmıyor.
- nigella_3: papatya çiçekleri, şişkin çizgili kapsül yok (yanlış nesne; ilk denemede de karo vardı).
Bu 4 evre için yeni iş önerilir (kimyon 1-2, anason 2, çörek otu 3; seed yok, kılavuz yöntemi karo üretmeye eğilimli: sadece maske kullanmak ya da `create_1_direction_object` denenebilir).

## Sorunlar
- Model kılavuz yüksekliğine uymadı (374'te de böyle): ölçülen yükseklikler tabloda. Evre dizisi büyümeyi tutarsız gösterebilir (kişniş 1 evresi 31 px, 2. evre 46); kod tarafı ölçek gerekebilir.
- Eksik 4 evre kod tarafında renkli şekille çizilir (iş dosyası notu).
- Zemin olmadığı için döşeme testi yok. Doygunluk fit'i uygulandı.
