unity-import: pending

# 551 Yeni ağaçlar 4/4: turunç, limon, keçiboynuzu (rapor)

Yan yana sayfası: [551-yanyana.png](551-yanyana.png) (4 referans + 9 yeni). Teslim edilmeyen ilk denemeler: `art/results/551-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object` stil eşleme (372/377/550 yöntemi), tuval 96x112, `high top-down`, `low detail`, `basic shading`, `single color outline`. Kılavuz: pivotta (40, y=88 üstten) 12x6 koyu kahve elips, geri kalanı saydam. Maske: pivotun üstü, x 6-89, y (88 - yükseklik - 4)..88. Açıklama: tablodaki metin + 372 eki + 377 eki. Her çıktıya `style.py fit --family bitki`.
Çağrılar: 9 ilk deneme + 2 ikinci deneme (carob_grown, lemon_young) = **11 gen** (cost_max 18). Hız sınırı hatası yok. `get_balance` sonrası: **1997,7** (paralel şeritler de harcıyor).

İkinci deneme gerekçeleri: carob_grown ilk denemesi fit sonrası neredeyse siyah (doygunluk x0.39, silüet okunmuyor); lemon_young ilk denemesi fidan değil, yapraklı tek limon gibiydi. İkincisinde açıklamaya "lighter green highlights" ve "thin ... sapling ..., no fruit" eklendi.

## Teslim edilenler (9/9), `Assets/Art/Iso/_aday/`
Hepsi 96x112, pivot x 40 / y 24 (alttan), yarı saydam piksel 0. Doygunluk fit: turunç x0.47-0.56, limon x0.39-0.58, keçiboynuzu x0.39-0.90.

| Dosya | Stil denetimi | validate_iso | Not |
|---|---|---|---|
| iso_tree_bitter_orange_young_se.png | GEÇTİ | geçti | küçük fidan, y 56-80 |
| iso_tree_bitter_orange_grown_se.png | GEÇTİ | geçti | açık yeşil yuvarlak taç, ince gövde, gövde ucu y=83 (pivottan ~5 px yukarıda) |
| iso_tree_bitter_orange_fruit_se.png | GEÇTİ | geçti | turuncu meyveli, geniş taç |
| iso_tree_lemon_young_se.png (2. deneme) | GEÇTİ | geçti | fidan; fit sonrası soluk gri-yeşil |
| iso_tree_lemon_grown_se.png | GEÇTİ | geçti | seyrek yapraklı, fidana yakın görünüm |
| iso_tree_lemon_fruit_se.png | UYARI (palete uzak, ort. 10.0) | geçti | soluk sarı limonlar, tanınıyor |
| iso_tree_carob_young_se.png | GEÇTİ | geçti | |
| iso_tree_carob_grown_se.png (2. deneme) | UYARI (palete uzak %88) | geçti | koyu yeşil geniş taç; tabanda küçük toprak/kök lekesi kaldı |
| iso_tree_carob_fruit_se.png | GEÇTİ | geçti | koyu kahverengi sarkık baklalar |

Zemin parçası olmadığı için döşeme testi yok. Gölge pişirilmemiş, ışık sol-üstten.

## Sorunlar
- carob_grown: tabanda küçük kahverengi toprak lekesi (yeniden boyama yapılmadı, maskeyle de temizlenmedi). Kabul kararında dikkat.
- lemon_grown fidandan çok ayrışmıyor, ağaç taçlı değil; lemon_fruit en iyi limon çıktısı.
- Yükseklikler hedefle tam tutmadı (genç 25-30, olgun 56-73 px civarı).
