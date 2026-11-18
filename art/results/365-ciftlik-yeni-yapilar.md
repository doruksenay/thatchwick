unity-import: pending

# 365 Çiftlik yeni yapılar (rapor)

Yan yana sayfası: [365-yanyana.png](365-yanyana.png). Teslim edilmeyenler: `art/results/365-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`, 2x2/3x3 için `--m`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Barınaklar (düven, domuz bölmesi, domuz ağılı, öküz arabası) Köylü takım eki (`rough light pine wood, rope lashing`); sandık Zanaatkâr eki (`dark oak wood with iron fittings`). Yükseklikler tarifte yok, tahmin edildi: harman 16, düven 12, ağıl 36, bölme 30, araba 44, sandık 26, sarnıç 10 px. 7 çağrı x 1 generation = 7 (cost_max 22). Sandık ve düven dışında altı çağrının ilk base64 aktarımı bir parçada bozulduğu için (öküz arabası, ücret yok) kılavuzlar küçük 1 bit PNG ile yeniden kodlandı.
`get_balance` iş sonu: 4576,90 (paralel şeritler de harcıyor; bu işin payı 7).
**Hata (benim):** harman yeri açıklamasına yanlışlıkla Köylü eki (`rough light pine wood, rope lashing`) girdi; tek deneme kuralı gereği tekrar edilmedi. Teslim edilmediği için sonucu etkilemedi.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | harman yeri (3x3) | TESLİM EDİLMEDİ | 53 renk > 48; yuvarlak yerine yelpaze/üçgen şekil, elmas ayak izini doldurmuyor |
| 2 | düven (2x1) | teslim | tahta kızak + sırık; çakmak taşı görünmüyor |
| 3 | domuz ağılı (2x2) | teslim (UYARI) | çatılı barınak + çamurlu zemin + tekne + çim tutamı (tarifteki "muddy yard"); palete biraz uzak |
| 4 | domuz bölmesi (1x2) | TESLİM EDİLMEDİ | saman dolu uzun yem teknesi çıktı, bölme/ahır gözü değil |
| 5 | öküz arabası (2x1) | teslim | iki tekerlekli, boş, uzun oyuk; tekerlekler dolu değil de kollu |
| 6 | hazine sandığı | teslim (UYARI) | ağır demir bantlı kapalı sandık; ailesinden soluk (x0.56); kilit okunmuyor |
| 7 | yer altı sarnıcı (2x2) | TESLİM EDİLMEDİ | stil KALDI (palete yakın %70, soluk x0.53; `fit` düzeltmedi) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_threshing_sledge_2x1_se_nw.png | 112x64 | 56 | 32 |
| iso_item_pigsty_2x2_se.png | 144x112 | 72 | 40 |
| iso_item_ox_cart_2x1_se_nw.png | 128x96 | 56 | 32 |
| iso_item_treasure_chest_se.png | 80x72 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| düven | x12-99, y8-56 | 17 | 0 | yok | geçti |
| ağıl | x8-135, y10-104 | 31 | 0 | yok | geçti |
| araba | x19-102, y16-83 | 33 | 0 | yok | geçti |
| sandık | x8-71, y5-64 | 25 | 0 | yok | geçti |
| bölme (red) | x8-103, y9-88 | 45 | 0 | yok | geçti (şekil yanlış) |
| sarnıç (red) | x8-135, y13-88 | 22 | 0 | yok | geçti |
| harman (red) | x11-194, y11-112 | 53 | 0 | yok | BAŞARISIZ (renk) |

Maske dışı piksel, kılavuz kalıntısı, ayak izi kaplaması ve ışık yönü (sol/sağ parlaklık) ölçülmedi; kullanıcı yan yana sayfasına bakmalı. Zemin döşeme testi yalnızca harman yeri için gerekirdi, teslim edilmediği için yapılmadı. Düzeltilmemiş çıktılar kırpılmadı.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| düven | %95 | 9.6 | x0.87 | GEÇTİ |
| ağıl | %85 | 11.0 | x0.98 | UYARI (palete uzak) |
| araba | %100 | 6.6 | x1.09 | GEÇTİ |
| sandık | %100 | 6.6 | x0.56 | UYARI (soluk) |
| bölme (red) | %92 | 7.4 | x1.03 | GEÇTİ |
| sarnıç (red) | %70 | 11.3 | x0.53 | KALDI (`fit` sonrası aynı) |
| harman (red) | %97 | 8.9 | x1.09 | GEÇTİ |

## Sorunlar / kullanıcı kararı
- Üç parça teslim edilmedi (harman yeri, domuz bölmesi, sarnıç); `max_attempts: 1` olduğu için ikinci deneme yapılmadı. Yeniden denemek için 365b işi yazılabilir (harman yeri için "round" yerine "full diamond-shaped ground patch" vurgusu, bölme için "four posts with a gate", sarnıç için açık taş renkleri).
- Ağılda zemin (çamur) pişmiş: tarif "muddy yard" istiyordu, zemin parçası olarak kabul edilebilir mi kullanıcı bakmalı.
- Ayak izi doluluğu ve ışık yönü sayıyla ölçülmedi.
- Araçta `seed` yok.
