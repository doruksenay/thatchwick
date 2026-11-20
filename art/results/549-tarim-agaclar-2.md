unity-import: pending

# 549 Yeni ağaçlar 2/4 (rapor)

Yan yana sayfası: [549-yanyana.png](549-yanyana.png) (4 referans + 8 yeni). Teslim edilmeyenler: `art/results/549-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object` stil eşleme, 96x112, `high top-down`, `low detail`, `basic shading`, `single color outline`. Kılavuz: pivotta (40, 88 üstten) 12x6 koyu kahve elips, maske pivotun üstündeki alan (377 yöntemi; betik `art/tmp/guide549.py`). Açıklama: tablodaki metin + `, the trunk ends directly on the transparent background, no base plate, no turf, no diamond`. Referans görseli ayrı verilmedi (stil eşlemede yalnızca kılavuz tuvali kullanılır, 377 gibi). Her çıktıya `style.py fit --family bitki`.
Çağrılar: 12 x 1 gen = **12 gen** (cost_max 24). Hız sınırı: ilk turda 6 çağrı "rate limit exceeded" verdi, bekledikten sonra her biri bir kez yeniden gönderildi (hepsi başarılı). `get_balance` sonrası: 2024,7 (paralel şeritler de harcıyor).
Yükseklikler (tarifteki 26-64) PixelLab'e ayrıca iletilemedi (tuval sabit 96x112); modelin ürettiği boy geçerli. Ayrı boy ayarı yapılmadı.
Taban parçalı üç görselde (kayısı olgun, fındık meyveli) yeşil/gri taban pikselleri yerel eşikle silindi (y>=71/76 altı, içerik yeniden boyanmadı). Fındık meyveli tabanında koyu yeşil kırıntı kaldı.

## Teslim edilenler (8/12), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Doygunluk (fit) | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_tree_almond_young_se.png | 96x112 | 40 / 24 | x11-73, y7-90 | 40 | x0,57 | GEÇTİ (palet %100, x1,15) | geçti |
| iso_tree_almond_grown_se.png | 96x112 | 40 / 24 | x15-79, y12-90 | 43 | x0,41 | GEÇTİ | geçti |
| iso_tree_almond_fruit_se.png | 96x112 | 40 / 24 | x14-81, y13-87 | 55 | x0,46 | GEÇTİ (palet %92) | geçti |
| iso_tree_apricot_grown_se.png | 96x112 | 40 / 24 | x22-66, y28-79 | 33 | x0,38 | GEÇTİ | geçti |
| iso_tree_apricot_fruit_se.png | 96x112 | 40 / 24 | x19-79, y16-85 | 54 | x0,63 | GEÇTİ | geçti |
| iso_tree_hazel_young_se.png | 96x112 | 40 / 24 | x26-69, y13-82 | 35 | x0,70 | GEÇTİ (x1,18) | geçti |
| iso_tree_hazel_fruit_se.png | 96x112 | 40 / 24 | x14-81, y18-81 | 32 | x0,36 | GEÇTİ | geçti |
| iso_tree_quince_fruit_se.png | 96x112 | 40 / 24 | x13-85, y4-86 | 26 | x0,39 | GEÇTİ | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Zemin parçası olmadığı için döşeme testi yok. (Kilidi kurulu aile: stil denetimi hepsi GEÇTİ.)

## Teslim edilmeyenler (`art/results/549-red/`)
| Dosya | Neden |
|---|---|
| iso_tree_apricot_young_se | Tabanda pembe/toprak elmas plaka |
| iso_tree_hazel_grown_se | Tabanda gri halka plaka; gövde ince, fındık çalısı değil, açık yeşil disklerden ağaç |
| iso_tree_quince_grown_se | Tabanda çimen elması; taç koyu, soluk |
| iso_tree_quince_young_se | Tabanda toprak-pembe elips, temizlenemedi |

Yeniden üretim için ayrı iş gerekir (tek deneme kuralı, ikinci deneme yapılmadı).

## Sorunlar
- Kalite bakımından kullanıcı gözüyle bakılması gerekenler: `almond_young` (yaprak dalı gibi, fidan değil), `almond_fruit` (tüylü kabuk görünmüyor, sarımsı-yeşil top yapraklar), `hazel_young` (çalı yerine koyu ağaç), `hazel_fruit` (çalı değil ağaç, kırmızı meyveler), `apricot_grown` (kalp biçimli turuncu meyveler var; olgun-meyveli ayrımı karışabilir), `quince_fruit` (taban dibinde çimen tutamı).
- Fıstıklar için boy hedefleri (genç 26-32, olgun 52-64) tutmadı; boylar modele bırakıldı.
- Araç önerisi: dört reddedilen için maskeyle tabanı kesen ek `inpaint` adımı ya da ikinci deneme için yeni iş.
