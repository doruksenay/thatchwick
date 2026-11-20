unity-import: pending

# 548 Yeni ağaçlar 1/4: dut, incir, nar, ceviz (rapor)

Yan yana sayfası: [548-yanyana.png](548-yanyana.png). Teslim edilmeyen ham: `art/results/548-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object` stil eşleme (377 yöntemi): 96x112 kılavuz (pivotta 12x6 koyu kahve elips) + her ağacın yüksekliğine göre dikdörtgen maske, `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + `muted greens` + `, the trunk ends directly on the transparent background, no base plate, no turf, no diamond`. Betikler `art/tmp548/` (commit edilmedi). Bitki referansı görseli verilmedi (kılavuz tek bağlam, 377 gibi).
Çağrılar: 12 çağrı + 1 hatam: ilk dut meyveli çağrısı yanlışlıkla incir genç maskesiyle gönderildi (sonuç kullanılmadı) = **13 gen** (cost_max 24). Hız sınırı: ceviz meyveli çağrısı "rate limit exceeded" verdi, ~30 sn sonra bir kez yeniden denendi. `get_balance` sonrası: 2035,7 (paralel şeritler de harcıyor).
Taban elması/çim parçası çoğu çıktıda yine geldi (incir olgun/meyveli, nar, dut meyveli vb.): yeni PixelLab çağrısı yapmadan yerel maske ile y>=79 altında gövde sütunu dışı ve yeşil pikseller silindi, en büyük bağlı bileşen dışı atıldı (içerik yeniden boyanmadı). Ardından `style.py fit --family bitki` (doygunluk x0,42-x1,00 → hedef).

## Teslim edilenler (11/12), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Stil denetimi | validate_iso |
|---|---|---|---|---|---|
| iso_tree_mulberry_young_se.png (dut genç) | 96x112 | 40 / 24 | y46-79 (yük. 34) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_mulberry_grown_se.png (dut olgun) | 96x112 | 40 / 24 | y18-86 (yük. 69) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_mulberry_fruit_se.png (dut meyveli) | 96x112 | 40 / 24 | y18-87 (yük. 70) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_fig_young_se.png (incir genç) | 96x112 | 40 / 24 | y51-89 (yük. 39) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_fig_grown_se.png (incir olgun) | 96x112 | 40 / 24 | y19-88 (yük. 70) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_fig_fruit_se.png (incir meyveli) | 96x112 | 40 / 24 | y21-86 (yük. 66) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_pomegranate_young_se.png (nar genç) | 96x112 | 40 / 24 | y52-87 (yük. 36) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_pomegranate_grown_se.png (nar olgun) | 96x112 | 40 / 24 | y24-87 (yük. 64) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_pomegranate_fruit_se.png (nar meyveli) | 96x112 | 40 / 24 | y22-85 (yük. 64) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_walnut_grown_se.png (ceviz olgun) | 96x112 | 40 / 24 | y12-92 (yük. 81) | UYARI yok, GEÇTİ (fit sonrası) | geçti |
| iso_tree_walnut_fruit_se.png (ceviz meyveli) | 96x112 | 40 / 24 | y18-85 (yük. 68) | UYARI yok, GEÇTİ (fit sonrası) | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Zemin parçası olmadığı için döşeme testi yok.

## Teslim edilmeyen
| Dosya | Neden |
|---|---|
| iso_tree_walnut_young_se.png | Fidan değil, havada yüzen yapraklı dal (y37-72, pivottan 16 px yukarıda). Ham: `548-red/walnut_young_ham.png`. Yeniden üretim gerekir |

## Sorunlar
- Yükseklikler tarifle birebir değil: genç dut 34, olgun dut 69, olgun incir 70, olgun ceviz 81 (tarif 36/72/64/92). Olgun ceviz tarifin altında, incir üstünde.
- Olgun incirde gövde neredeyse beyaz (pale grey bark isteği abartılmış), kök tabanı beyaz.
- Meyveli nar ve incirde meyve renkleri `fit` sonrası soluk (nar kırmızısı pembeye yakın).
- Genç dut ve genç nar çok küçük/seyrek; genç incir açık yapraklı, gövdesi ince.
- Genç/olgun/meyveli üçlülerinin "aynı tür" tutarlılığı sayfadan kullanıcı tarafından değerlendirilmeli (dut, incir, nar üçlüleri farklı taç formunda).
- Öneri: ceviz genç için yeni iş (gövdeli fidan tarifiyle).
