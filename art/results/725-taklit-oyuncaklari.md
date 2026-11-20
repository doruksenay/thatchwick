unity-import: pending

# 725 Taklit oyuncakları (4): 4/4 teslim

Yan yana sayfası: [725-yanyana.png](725-yanyana.png). Ham koyun: `art/results/725-red/sheep_ham.png`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Tuval 32x32 (araç alt sınırı), kılavuz: tabanda 24x12 elmas (#786046), alt kenardan 4 px yukarıda; maske = taban ve üstünü kapsayan dikdörtgen (betikle `art/tmp725`'te üretildi; `footprint_guide.py` bu ölçüye uygun değil). Dört çağrı paralel, her biri 1 gen = 4 gen (cost_est 4). `get_balance` sonrası: 1814,7 (paralel şeritler dahil).
- Koyun: doygunluk ham x1,73 (KALDI, palet %84), `style.py fit` ile x0,66'ya indirildi, yeniden denetimde GEÇTİ.

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/iso_item_toy_pot_set_se.png | 32x32 | 16 | 10 |
| Assets/Art/Iso/iso_item_toy_hammer_se.png | 32x32 | 16 | 10 |
| Assets/Art/Iso/iso_item_toy_hoe_se.png | 32x32 | 16 | 10 |
| Assets/Art/Iso/iso_item_toy_sheep_se.png | 32x32 | 16 | 10 |

Pivot = taban elmasının merkezi (fruit_basket kuralı).

## Doğrulama
| Dosya | validate_iso | Opak sınır | Renk | Stil (palete yakın / uzaklık / doygunluk) | Sonuç |
|---|---|---|---|---|---|
| pot_set | geçti | x3-28, y4-25 | 9 | %100 / 8,2 / x1,29 | UYARI (doygun) |
| hammer | geçti | x10-21, y4-27 | 13 | %100 / 7,8 / x0,67 | GEÇTİ |
| hoe | geçti | x3-28, y6-25 | 7 | %100 / 5,7 / x0,29 | UYARI (soluk) |
| sheep (fit) | geçti | x4-27, y4-27 | 19 | %100 / 9,7 / x1,14 | GEÇTİ |

Zemin parçası yok, döşeme testi gerekmedi. Gölge pişirilmemiş.

## Sorunlar
- Kullanıcı yan yana sayfasına bakmalı: 32x32 tuval bez bebek ölçüsünden büyük olabilir; hoe çok soluk (x0,29), pot_set biraz doygun; koyunda "yüz detayı yok" şartı bu boyutta doğrulanamadı.
- Tek deneme, yeniden üretim yapılmadı.
