unity-import: pending

# 374 Ekin evreleri yeniden (rapor)

Yan yana sayfası: [374-yanyana.png](374-yanyana.png). Evre dizisi (satırlar pırasa, bezelye, şalgam; sütunlar evre 1-2-3; 371'den kurtarılanlar `_aday/`daki dosyalardır, eksik hücre = evre yok): [374-evre-dizisi.png](374-evre-dizisi.png). Reddedilen deneme: `art/results/374-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ek metin. Her çıktıya `style.py fit --family bitki` uygulandı.
Çağrılar: 4 ilk deneme + 1 ikinci deneme (pırasa 3: ilk deneme toprak tepsili; metne "floating alone, no soil, no dirt" eklendi) = **5 gen** (cost_max 8). Hız sınırı hatası yok. `get_balance` sonrası: 4653,9 (paralel şeritler de harcıyor).

## Teslim edilenler (4/4), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Doygunluk (fit sonrası) | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_crop_leek_1_se.png | 80x72 | 40 / 24 | x37-46, y22-47 | 21 | x1,16 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_leek_3_se.png (2. deneme) | 80x72 | 40 / 24 | x15-65, y5-64 | 23 | x1,15 | UYARI (palete uzak) | geçti |
| iso_crop_pea_2_se.png | 80x72 | 40 / 24 | x26-55, y10-48 | 51 | x1,15 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_turnip_2_se.png | 80x72 | 40 / 24 | x24-55, y11-61 | 31 | x1,15 | UYARI (palet dışı, kilit yok) | geçti |

Yarı saydam piksel 0, zemin/tepsi yok (pırasa 3 ikinci denemede giderildi). Gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok.

## Sorunlar (kullanıcı yan yana sayfasına bakmalı)
- **Yükseklikler tarifin dışında:** model kılavuz maskesinin yüksekliğine uymadı. Ölçülen opak yükseklik: pırasa 1 = 26 px (hedef 10), pırasa 3 = 60 px (hedef 32), bezelye 2 = 39 px (hedef 30), şalgam 2 = 51 px (hedef 22; tam şalgam gövdesi görünüyor, "yalnızca üstü" değil). Evre dizisinde büyüme tutarsız okunabilir; kod tarafı için ölçek gerekebilir.
- Pırasa 1: tek, kalın, açık yeşil sap (üç ince filiz değil), tepede küçük yaprak; tanınırlığı zayıf. Bezelye 2: çubuksuz, yuvarlak yapraklı sarmaşık.
- Pırasa 3 ve şalgam 2 biçim olarak tanınıyor. Boyut düzeltmeli yeni iş önerilir.
- İkinci deneme yalnızca pırasa 3 için kullanıldı (biçim: tepsi).
