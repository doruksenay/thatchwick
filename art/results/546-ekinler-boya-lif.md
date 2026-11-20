unity-import: pending

# 546 Yeni ekinler 7/7: kendir, çivit otu, kökboya, sarı boya otu, havuç 3 (rapor)

Yan yana sayfası: [546-yanyana.png](546-yanyana.png). Reddedilenler: `art/results/546-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
374 yöntemi: `create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; maske yükseklikleri 10 / 22 / 32, kendir 3 için 40), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374 ek metni. Her çıktıya `style.py fit --family bitki` uygulandı (doygunluk x0,39-1,00).
Çağrılar: 13 ilk deneme + 7 ikinci deneme (yalnızca biçim için; metne "floating alone, no soil, no dirt, no ground" eklendi) = **20 gen** (cost_max 26). Hız sınırı: kökboya 2 ilk çağrısı 429 verdi, 30 sn sonra bir kez yeniden denendi (gen harcamadı). `get_balance` sonrası: 2068,7 (paralel şeritler de harcıyor).

## Teslim edilenler (11/13), `Assets/Art/Iso/_aday/`
Tümü 80x72, pivot x=40 / y(alttan)=24, yarı saydam piksel 0, gölge pişirilmemiş. validate_iso geçti. Stil denetimi hepsinde GEÇTİ (palete yakın %99-100, doygunluk x1,12-1,15).
| Dosya | Deneme | Opak yükseklik (hedef) | Not |
|---|---|---|---|
| iso_crop_hemp_1_se.png | 1 | 27 (10) | tırtıklı yapraklı fide, iyi |
| iso_crop_hemp_2_se.png | 1 | 43 (22) | ince, avuç yapraklı sap, iyi |
| iso_crop_hemp_3_se.png | 2 | 47 (40) | yıldız yapraklı, tepede tohum; sık gövde değil |
| iso_crop_woad_1_se.png | 1 | 27 (10) | yuvarlak, kapak gibi; rozet olarak zayıf |
| iso_crop_woad_3_se.png | 1 | 51 (32) | uzun sapta sarı çiçek, iyi |
| iso_crop_madder_1_se.png | 1 | 30 (10) | yuvarlak yapraklı filiz; kökboya olarak genel |
| iso_crop_madder_2_se.png | 2 | 34 (22) | yayılan yuvarlak yapraklar, uygun |
| iso_crop_weld_1_se.png | 1 | 29 (10) | yassı dar yapraklı rozet, iyi |
| iso_crop_weld_2_se.png | 2 | 45 (22) | geniş yapraklı sivri filiz, yaprak sapı belirgin |
| iso_crop_weld_3_se.png | 2 | 50 (32) | ince sap üzerinde sarı çiçek başağı, iyi |
| iso_crop_carrot_3_se.png | 2 | 46 (32) | geniş yapraklı üst, ten rengi kök başı; havuç tüyü ve turuncu-mor ton zayıf |

## Teslim edilmeyenler (2/13), `546-red/`
- madder_3: iki denemede de çimen/toprak karosu üstünde (zemin parçası).
- woad_2: iki denemede de palet dışı (ilk: %60, ikinci: %41, KALDI), turkuaz yıldız yapraklar.
Bu iki evre için yeni iş önerilir (kökboya 3, çivit otu 2); kod tarafında renkli şekille çizilir.

## Sorunlar
- Model kılavuz yüksekliğine uymadı (374 ve 545'te de böyle); evre dizisi büyümeyi tutarsız gösterebilir.
- Bitki zemin parçası olmadığından döşeme testi yok.
- Çivit otu 1 ve kökboya 1 tür olarak ayırt edilmiyor (genel yeşil yapraklı fide).
