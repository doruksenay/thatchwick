unity-import: pending

# 550 Yeni ağaçlar 3/4 (rapor)

Yan yana sayfası: [550-yanyana.png](550-yanyana.png) (4 referans + 12 yeni). Teslim edilmeyen ilk denemeler: `art/results/550-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object` stil eşleme, tuval 96x112, `high top-down`, `low detail`, `basic shading`, `single color outline`. Kılavuz: pivotta (40, y=88 üstten) 12x6 koyu kahve elips; maske: pivotun üstündeki alan (ağaç yüksekliği + 4 px). Açıklama: tablodaki metin + 372 eki + `the trunk ends directly on the transparent background, no base plate, no turf, no diamond`. Her çıktıya `style.py fit --family bitki`.
Çağrılar: 12 ilk deneme + 2 ikinci deneme (peach_fruit, sour_cherry_grown) = **14 gen** (cost_max 24). Hız sınırı: 4 çağrı "rate limit exceeded" verdi (muşmula olgun/meyveli, kızılcık genç/olgun); 30+ sn sonra her biri bir kez yeniden gönderildi, başarılı. Hız sınırı hatalı çağrılar iş oluşturmadı (sayıldı: bakiye farkı ~37, paralel şeritler de harcıyor). `get_balance` sonrası: **2022,7**.

Referanslar (stil eşleme) yalnızca kılavuz görseliyle verildi (372/377 yöntemi); `style.py fit` aileye indirgedi.

## Yerel temizlik (yeniden boyama yok)
Tabanda zemin parçası gelen 5 çıktıda (cornel_grown, cornel_young, medlar_grown, medlar_fruit, cornel_fruit) maskeyle yalnızca taban parçası pikselleri (yeşil/açık/gri, y>=74-84) saydam yapıldı, en büyük bağlı bileşen dışı silindi. İkinci deneme çıktılarından peach_fruit (yeşil taban tüyü) ve sour_cherry_grown (toprak bloğu, x dışı + yeşil) aynı yöntemle temizlendi.

## Teslim edilenler (12/12), `Assets/Art/Iso/_aday/`
Hepsi 96x112, pivot x 40 / y 24 (alttan), yarı saydam piksel 0.

| Dosya | Stil denetimi | validate_iso | Not |
|---|---|---|---|
| iso_tree_peach_young_se.png | GEÇTİ | geçti | küçük fidan |
| iso_tree_peach_grown_se.png | GEÇTİ | geçti | uzun sarkık yapraklar |
| iso_tree_peach_fruit_se.png (2. deneme) | GEÇTİ | geçti | pembe-turuncu şeftaliler; 1. deneme krem toplu (şeftaliye benzemiyor), `550-red/` |
| iso_tree_sour_cherry_young_se.png | GEÇTİ | geçti | |
| iso_tree_sour_cherry_grown_se.png (2. deneme) | GEÇTİ | geçti | 1. deneme palet dışı (KALDI, `550-red/`); temizlenmiş hâli küçük taçlı, gövde ucu y=73 (pivot 88'in ~15 px üstü: havada) |
| iso_tree_sour_cherry_fruit_se.png | GEÇTİ | geçti | |
| iso_tree_medlar_young_se.png | GEÇTİ | geçti | |
| iso_tree_medlar_grown_se.png | UYARI (palete uzak %89) | geçti | sarı-yeşil yapraklar, tabanı temizlendi |
| iso_tree_medlar_fruit_se.png | GEÇTİ | geçti | çıplak, kahverengi meyveli; taş halka temizlendi |
| iso_tree_cornel_young_se.png | GEÇTİ | geçti | gövdesiz alçak çalı kümesi; toprak elması silindi |
| iso_tree_cornel_grown_se.png | GEÇTİ | geçti | taban kökleri kırıntılı |
| iso_tree_cornel_fruit_se.png | GEÇTİ | geçti | kırmızı meyveli |

Zemin parçası olmadığı için döşeme testi yok. Gölge pişirilmemiş.

## Sorunlar
- Tabanda zemin parçası beş çıktıda geldi (tarifin eki yine bastıramadı); yerel maskeyle temizlendi, taban/kök kırıntıları kalmış olabilir.
- **sour_cherry_grown** ve **cornel_young**: gövde kısa ya da yok, tacın altı pivottan yüksekte (havada görünebilir). Kabul kararında dikkat.
- Hedef yükseklikler (30/60/56) tam tutmadı; boyutlar opak sınırdan okunabilir (genç 18-30 px, olgun ~50-60 px).
- Araç önerisi: tabanlı çıktıyı temizleyen betik `tools/art/` altına eklenebilir (bulutta eklenmedi).
