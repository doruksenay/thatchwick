unity-import: pending

# 745 HUD pilotu, yön B: Oyma meşe (KISMİ: yalnızca sayfalar)

Yan yana: art/results/745-yanyana.png

## Araç ve ayarlar
- create_ui_asset 256x192 (sheet1, 10 gen): 128'lik arayüz paleti, max_colors=32, seed=0. Yükseklik alt sınırı 192 olduğundan 256x128 yapılamadı.
- create_ui_asset 256x192 (sheet2, 10 gen): aynı ayarlar.
- create_image_pixflux 128x64 (sheet3, 1 gen): palet verilemedi, seed=0, flat shading.
- Toplam 21 generation. get_balance sonrası: 746,6 (paralel şeritler de harcıyor, bu işin payı 21).

## Dosyalar (hepsi sayfa; kesilmiş parça yok)
| Dosya | Boyut | Not |
|---|---|---|
| ui_hud2_mese_sheet1.png | 256x192 | Zincirli saat levhası, çift şeritli panel, krem şerit, ince çubuk kılıfı, boş çerçeve. Meşe ve sarmaşık dili tutuyor |
| ui_hud2_mese_sheet2.png | 256x192 | Düğme ve rozet YOK: yalnızca tek büyük sarmaşıklı çerçeve |
| ui_hud2_mese_sheet3.png | 128x64 | 16 küçük ikon, ızgarasız; ekmek, hilal, alev, topaç, yaprak, filiz, kar tanesi tanınıyor |

## Doğrulama (style.py check, arayuz)
| Dosya | Palete yakın | Sonuç |
|---|---|---|
| sheet1 | %100 | GEÇTİ |
| sheet2 | %100 | GEÇTİ |
| sheet3 | %95 | GEÇTİ |

## Sorunlar
- Parçalar native boyutta (96x40, 32x32, 16x16 vb.) kesilemedi: sayfadaki parçalar tarif boyutundan çok büyük üretildi ve ölçek düşürme yasak. 9-slice testi ve ikon x3 sayfası yapılamadı.
- Sheet2 istenen düğmeleri ve rozeti üretmedi (P4, P5 eksik). Tek deneme kuralı gereği yeniden denenmedi.
- Sheet3 ikonları ızgaraya oturmadı; P7, P9 (hız), P10 setleri eksik.
- Öneri: parçaları tek tek, küçük boyutta ayrı üretmek (ikonlar için create_image_pixen 16/20/24/28; düğmeler için tek nesne araçları). Sayfa yöntemi bu araçlarla tutmadı. Karar kullanıcıda.
- Masaüstü kopyası: yerel.
