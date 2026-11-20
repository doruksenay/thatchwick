unity-import: pending

# 553 Yaban bitkileri 1/3: orman (rapor)

Sonuç: **6/9 teslim** (`Assets/Art/Iso/_aday/`); chanterelle, sloe ve wild_pear teslim edilmedi (biçim). Yan yana sayfası: [553-yanyana.png](553-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
373 yöntemi: `create_map_object` stil eşleme, referanssız 1x1 kılavuz + özel maske (`footprint_guide.py`, tuval 80x72, pivot x=40 / y alttan 24), `high top-down`, `low detail`, `basic shading`, `single color outline`, 373'teki açıklama eki. Çağrılar: 9 ilk deneme + 4 ikinci deneme (morel, chanterelle, sloe, wild_pear; yalnızca biçim) = 13 generation. İkinci denemede maske zemin elmasının alt yarısını (y>=50) kapsamıyordu; sloe ve pear tarifine "no trunk, no tree" eklendi. Üretimden sonra `style.py fit --family bitki` (doygunluk: bilberry x0.85, raspberry x0.74, morel x0.52 (ikinci deneme), porcini x1.00, hawthorn x0.63, acorn x0.45). `get_balance` sonrası: 1966,7 (paralel şeritler de harcıyor).

## Dosyalar ve doğrulama
| Dosya | Boyut | Pivot x / y(alttan) | Renk | validate_iso | Stil denetimi |
|---|---|---|---|---|---|
| iso_forage_bilberry_se | 80x72 | 40 / 24 | 42 | geçti | UYARI (palete uzak, %97) |
| iso_forage_wild_raspberry_se | 80x72 | 40 / 24 | 34 | geçti | GEÇTİ |
| iso_forage_morel_se | 80x72 | 40 / 24 | 9 | geçti | GEÇTİ |
| iso_forage_porcini_se | 80x72 | 40 / 24 | 11 | geçti | GEÇTİ |
| iso_forage_hawthorn_se | 80x72 | 40 / 24 | 51 | geçti | GEÇTİ |
| iso_forage_acorn_se | 80x72 | 40 / 24 | 23 | geçti | GEÇTİ |

## Sorunlar
- **chanterelle teslim yok:** iki denemede de çim/toprak bloğu üstünde çıktı (ham: `553-red/chanterelle_raw1.png`, `chanterelle_raw.png`).
- **sloe teslim yok:** ilk deneme ağaç + toprak bloğu, ikinci deneme çıplak ağaç (`553-red/sloe_raw1.png`, `sloe_raw.png`).
- **wild_pear teslim yok:** ilk deneme tam ağaç, ikinci deneme ince fidan (`553-red/wild_pear_raw1.png`, `wild_pear_raw.png`).
- morel ikinci deneme: zemin yok, ama başlıklar turuncu ve petek dokusu belli değil, küçük (yükseklik ~23 px, hedef 10 px aşıldı). Kullanıcı karar versin. İlk deneme (`553-red/morel_raw1.png`) blok üstündeydi.
- hawthorn: kırmızı alıç salkımları seçilmiyor, açık yeşil çalı gibi okunuyor. Raspberry: kırmızı meyve az.
- Döşeme testi ve çimen üstü sayfa: zemin işi değil, yapılmadı.
- Öneri: chanterelle, sloe ve wild_pear için ayrı tarif (kılavuzsuz ya da mantar kümesi için mantar+yaprak çöpü, çalılar için "dense round shrub, wide leafy mass, no trunk visible").
