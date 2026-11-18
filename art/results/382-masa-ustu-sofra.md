unity-import: pending

# 382 Masa ve raf üstü: sofra takımı ve kaplar (rapor)

Yan yana sayfası: [382-yanyana.png](382-yanyana.png) (teslim edilen 9 eşya + 7 referans). Teslim edilmeyen: `art/results/382-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (301 yöntemi; kılavuz ve maske `art/tmp382/` altında Python ile üretildi, `footprint_guide.py` yalnızca tam 64x32 karo ayak izi yaptığı için küçük eşyalara uymuyor: öneri aşağıda). Taban elması alt kenardan 4 px yukarıda, maske eşya yüksekliği kadar yukarı + 1 px. `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 10 çağrı x 1 gen = 10 generation (cost_max 16). Tuval, araç alt sınırı (32 px) ve taban genişliği yüzünden tarifteki tuvalden büyük (tablo aşağıda). Hiçbir çağrıda ikinci deneme yapılmadı (yağ testisi paletten KALDI, ikinci deneme kuralının maddelerine girmiyor). `get_balance` iş sonu: 4799,90 (paralel şeritler de harcıyor; bu işin payı 10).
Depo küçük küp (#10) ilk çağrıda base64 kesilmesiyle reddedildi (üretim başlamadı, harcama yok), kılavuz yeniden kodlanıp tekrar gönderildi.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | tahta tabak | teslim | GEÇTİ |
| 2 | kil tabak | teslim | UYARI: ailesinden doygun (x1.35) |
| 3 | kalay tabak | teslim | UYARI: palete uzak (%88, 13.4), açık pembemsi ton |
| 4 | servis tepsisi | teslim | GEÇTİ |
| 5 | tahta kase | teslim | GEÇTİ |
| 6 | kepçe | teslim | UYARI: soluk (x0.49), çok ince, küçük pencerede zor okunur |
| 7 | havan ve tokmak | teslim | GEÇTİ |
| 8 | yağ testisi | TESLİM EDİLMEDİ | ham: palet %55 (KALDI), `fit` sonrası %70 (hâlâ KALDI): turuncu/terakota palet dışı |
| 9 | bakır kazan | teslim (doygunluk x0.70 `fit`) | ham KALDI (palet %60, x1.64); `fit` sonrası UYARI (palete uzak). Ham hâli `382-red/`de |
| 10 | küçük küp | teslim | UYARI: palete uzak (11.0), x0.68 |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_wooden_plate_se.png | 32x32 | 16 | 9 |
| iso_item_clay_plate_se.png | 32x32 | 16 | 9 |
| iso_item_tin_plate_se.png | 32x32 | 16 | 9 |
| iso_item_serving_tray_se.png | 40x32 | 20 | 11 |
| iso_item_wooden_bowl_se.png | 32x32 | 16 | 8 |
| iso_item_ladle_se.png | 32x40 | 16 | 6 (asılı eşya; kanca noktası görselin üstü) |
| iso_item_mortar_pestle_se.png | 32x36 | 16 | 8 |
| iso_item_copper_pot_se.png | 40x40 | 20 | 10 |
| iso_item_storage_pot_small_se.png | 40x48 | 20 | 11 |

Pivotlar kılavuzdaki küçük taban elmasının merkezidir.

## Doğrulama (validate_iso, `--max-colors 48`, ayak izi kutusu verilmedi)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| tahta tabak | x6-25, y12-27 | 7 | 0 | yok | geçti |
| kil tabak | x6-25, y12-25 | 11 | 0 | yok | geçti |
| kalay tabak | x6-25, y12-24 | 15 | 0 | yok | geçti |
| servis tepsisi | x6-33, y6-26 | 11 | 0 | yok | geçti |
| tahta kase | x7-24, y9-22 | 7 | 0 | yok | geçti |
| kepçe | x12-19, y5-34 | 6 | 0 | yok | geçti |
| havan | x7-24, y8-29 | 15 | 0 | yok | geçti |
| bakır kazan (fit) | x7-32, y5-33 | 7 | 0 | yok | geçti |
| küçük küp | x6-33, y7-43 | 13 | 0 | yok | geçti |
| yağ testisi (teslim değil) | x8-23, y5-35 | 9 | 0 | yok | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| tahta tabak | %100 | 3.4 | x1.20 | GEÇTİ |
| kil tabak | %91 | 9.9 | x1.35 | UYARI |
| kalay tabak | %88 | 13.4 | x0.73 | UYARI |
| servis tepsisi | %100 | 6.6 | x0.82 | GEÇTİ |
| tahta kase | %100 | 6.2 | x0.78 | GEÇTİ |
| kepçe | %100 | 6.4 | x0.49 | UYARI |
| havan | %100 | 6.8 | x1.20 | GEÇTİ |
| bakır kazan (fit x0.70) | %100 | 11.7 | x1.15 | UYARI |
| küçük küp | %100 | 11.0 | x0.68 | UYARI |
| yağ testisi (ham) | %55 | 15.8 | x1.22 | KALDI |
| yağ testisi (fit x0.94) | %70 | 14.8 | x1.15 | KALDI |

## Gözlem (görsel)
- Kalay ve bakır: bakır kazan boş, koyu bordo iç, iki kulp; parlak turuncu-bakır. Kalay tabak pembemsi, kalaya değil etle benzer okunabilir.
- Kepçe çok ince, 6 renk; görünür ama zayıf.
- Işık: sol-üst, gölge pişirilmemiş (gözle). Ölçüm yapılmadı.
- Zemin döşeme testi: eşya olduğu için uygulanmadı.

## Sorunlar ve öneriler
- Yağ testisi teslim edilmedi: paleti terakota/turuncu. Yeniden deneme için "dull brownish grey-brown earthenware, no orange" gibi ton ipucu önerilir.
- `footprint_guide.py` yalnızca 64x32 karo ayak izi üretiyor; küçük masa üstü eşyalar için yapılandırılabilir taban genişliği (ör. `--base-w`) eklenmesi önerilir. Bu işte betik `art/tmp382/` altında yazıldı.
- Tuvaller tarifteki boyutlardan büyük (alt sınır 32 px ve taban payı); oyunda yuva ölçeğine göre kod oturumu kırpmayı/ölçeği değerlendirmeli.
