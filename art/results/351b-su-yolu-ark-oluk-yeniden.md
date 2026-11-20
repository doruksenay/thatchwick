unity-import: pending

# 351b Su yolu karoları yeniden: 12/18 teslim (6 parça eksik)

Yan yana sayfası (3 referans + 12 yeni): [351b-yanyana.png](351b-yanyana.png). 351'in reddedilen çıktısı için: [351-yanyana.png](351-yanyana.png).
Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_tiles_pro`, stil modu, 64x32. `style_images`: kuru çağrıda yalnızca `iso_ground_dirt_a.png` (002 toprağı), sulu çağrıda dirt_a + `iso_ground_water_a.png` (gölet suyu). Neden: 351'de yalnızca çimen verilmişti ve zemin turuncu çıkmıştı. Stil: palet kapalı, outline/detail/shading açık.
- Tarife "kazılmış hendek, kütük değil", "bacaksız U oluk", "yuvarlak kapak değil" ifadeleri eklendi. Her çağrıda 12 numaralı tarif (3 tür x düz NE-SW, düz SE-NW, kol NE, kol SE), 16 karo döndü.
- 2 çağrı, her biri **10 gen**, toplam **20** (cost_est 20, cost_max 40). Sulu çağrı ilk denemede hız sınırı (10/10 iş) aldı, bekleyip bir kez yeniden denendi, geçti.
- `get_balance` (çağrılardan önce): 2762,9. Sonrası paralel şeritler yüzünden güvenilir değil.

## Teslim edilenler (Assets/Art/Iso/_aday/, hepsi 64x32, pivot x 32, pivot y 16 alttan)
| Parça | Kuru | Sulu |
|---|---|---|
| ditch center | iso_conduit_ditch_dry_center_v2 | iso_conduit_ditch_wet_center_v2 |
| ditch arm_ne | iso_conduit_ditch_dry_arm_ne_v2 | iso_conduit_ditch_wet_arm_ne_v2 |
| ditch arm_se | iso_conduit_ditch_dry_arm_se_v2 | iso_conduit_ditch_wet_arm_se_v2 |
| stone center | iso_conduit_stone_dry_center_v2 | iso_conduit_stone_wet_center_v2 |
| stone arm_se | iso_conduit_stone_dry_arm_se_v2 | iso_conduit_stone_wet_arm_se_v2 |
| flume center | iso_conduit_flume_dry_center_v2 | iso_conduit_flume_wet_center_v2 |

`center` = karonun ortasından NE-SW doğrultusunda düz geçen parça (SE-NW düz parça üretilemedi: iki düz karo aynı yönde çıktı).

## Teslim EDİLMEYENLER (tek deneme, boşluk doldurulmadı)
- `stone_*_arm_ne`: üretilen karo kol değil, tam düz hat çıktı.
- `flume_*_arm_ne`, `flume_*_arm_se`: ikisi de tam düz oluk çıktı, kol yok.
- Öneri (devam işi): bu 3 kol x 2 durum için ayrı çağrı, teslim edilen arm_se parçaları referans eklenerek. Dikey çevirme ışığı bozar, kullanılmadı.

## Doğrulama
| Dosya | validate_iso | Stil denetimi | Doygunluk | Renk | Dikiş oranı |
|---|---|---|---|---|---|
| ditch dry center/arm_ne/arm_se | geçti | GEÇTİ (fit x0.63/0.81/0.81 sonrası) | x1.16/1.15/1.15 | 22/28/27 | 1.10/1.06/1.07 |
| ditch wet center/arm_ne/arm_se | geçti | GEÇTİ | x0.93 | 25/25/23 | 1.09/1.02/1.08 |
| stone dry center | geçti | UYARI (palete uzak) | x1.16 (fit x0.86) | 29 | 1.06 |
| stone dry arm_se | geçti | UYARI (palete uzak) | x1.16 (fit x0.95) | 29 | 1.04 |
| stone wet center | geçti | UYARI (ailesinden soluk) | x0.53 | 25 | 1.02 |
| stone wet arm_se | geçti | GEÇTİ | x0.69 | 24 | 1.00 |
| flume dry center | geçti | GEÇTİ (fit x0.89) | x1.14 | 20 | 1.01 |
| flume wet center | geçti | GEÇTİ | x1.09 | 28 | 1.00 |

Aile kilidi kurulu değil (palet uyumu uyarı). Ham kuru karolar KALDI (x1.81) olduğundan `fit` uygulandı. Dikiş oranı = kenar bandı / iç parlaklık. Döşeme testi (8x8, (32,16) adımı) `art/tmp351b/` altında yapıldı. Gölge pişirilmemiş, ışık sol-üstten. Maske dışı piksel yok (kırpma gerekmedi).

## Sorunlar
- **Kuru karoların zemini hâlâ 002 toprağından açık, turuncuya yakın** (ana renk fit sonrası ~(188,143,92); 002: (138,104,68)). Sulu karoların zemini 002 ile birebir aynı. Yan yana konunca fark görülür; kuru parçalar için yeniden üretim önerilir. Kabul kararında değerlendirilmeli.
- Ditch kolları karo kenarına tam ulaşıyor mu, yan yana sayfasından gözle bakılmalı.
