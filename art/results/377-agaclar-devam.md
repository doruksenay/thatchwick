unity-import: pending

# 377 Ağaçlar devam (rapor)

Yan yana sayfası: [377-yanyana.png](377-yanyana.png) (4 referans + 4 yeni + genç meşe). Tabanlı ham meşe/huş: `art/results/377-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object` stil eşleme, 96x112, `high top-down`, `low detail`, `basic shading`, `single color outline`. Kılavuz: pivotta (40,88 üstten) 12x6 koyu kahve elips, maske eskisi gibi (betik `art/tmp/guide377.py`). 372 açıklamaları + 372 eki + tarifin eki (`the trunk ends directly on the transparent background, no base plate, no turf, no diamond`). Her çıktıya `style.py fit --family bitki`.
Çağrılar: 4 ilk (1 gen) + söğüt ikinci denemesi = **5 gen** (cost_max 14). Hız sınırı: söğüt ilk çağrısı "rate limit exceeded" verdi, bir süre sonra bir kez yeniden denendi (başarılı). `get_balance` sonrası: 3745,5 (paralel şeritler de harcıyor).
Meşe ve huş ilk denemede yine elmas çim tabanlı geldi. Yeni PixelLab çağrısı yapmadan taban parçası yerel maskeyle silindi (yalnız yeşil tonlu pikseller y>=80/78 altında saydam yapıldı, en büyük bağlı bileşen dışı kırpıldı; içerik yeniden boyanmadı). Tarifin inpaint adımı yerine bu yapıldı.

## Teslim edilenler (4/4), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Doygunluk (fit) | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_tree_oak_grown_se.png | 96x112 | 40 / 24 | x12-70, y13-94 | 35 | x0,69 | UYARI (palet dışı, kilit yok) | geçti |
| iso_tree_birch_grown_se.png | 96x112 | 40 / 24 | x25-71, y12-94 | 41 | x0,77 | UYARI | geçti |
| iso_tree_willow_young_se.png (2. deneme) | 96x112 | 40 / 24 | x22-71, y36-84 | 32 | x1,00 | UYARI | geçti |
| iso_tree_plum_young_se.png | 96x112 | 40 / 24 | x19-71, y31-96 | 32 | x0,48 | UYARI | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Zemin parçası olmadığı için döşeme testi yok.

## Sorunlar
- **Olgun meşe boyu:** opak yükseklik 82 px; genç meşe 67 px (372). Oran ~1,2, tarifin ~2 katı değil. Köylünün 1,5 katı kontrolü kullanıcıya bırakıldı.
- **Meşe tabanında** küçük yeşil kök/yaprak kalıntıları kaldı; huşta birkaç piksel kök kırıntısı.
- **Söğüt genç:** 1. deneme çıplak ağaç + çim tabanlı (teslim edilmedi). 2. deneme söğüt perdesi olarak tanınıyor ama gövde görünmüyor ve ağaç yerden ~4 px havada (en alt opak y=84, pivot y=88). Kabul kararında dikkat.
- **Erik genç:** tabansız temiz çıktı ama tacı genç ağaç için büyük, 372 erik olgununa yakın boyut.
- Genç/olgun meşenin "aynı türün iki evresi" kontrolü yan yana sayfasından görsel olarak yapılmalı.
