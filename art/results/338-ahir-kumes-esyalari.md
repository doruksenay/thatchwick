unity-import: pending

# 338 Ahır ve kümes eşyaları (rapor)

Yan yana sayfası: [338-yanyana.png](338-yanyana.png). Teslim edilmeyen/ham: `art/results/338-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`tools/art/footprint_guide.py`; 301 yöntemi), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 12 başarılı çağrı x 1 gen = **12 generation** (base64 aktarım hataları sunucuda doğrulamada reddedildi, üretim başlamadı). Yeniden deneme yok. Ahır ve at bölmesi `--axis y`, 2x1'ler `--axis x`. Toplama sandığında aktarım sorunu yüzünden maske olarak eyer sehpasının maskesi kullanıldı (aynı tuval ve ayak izi; yükseklik 30 yerine 26 olacaktı). `get_balance` iş sonu: 4999 / 5000 (hesap bu sırada Tier 2'ye yükselmiş, dönem yenilenmiş: 2026-12-18; eski bakiyeyle karşılaştırılamaz).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | folluk | teslim | |
| 2 | ahır bölmesi | teslim | |
| 3 | at bölmesi | teslim | stil UYARI (palete uzak) |
| 4 | yemlik | teslim | |
| 5 | su yalağı | TESLİM EDİLMEDİ | stil KALDI: doygunluk x0.36 (soluk), palet dışı renkler; `fit` düzeltmedi (x1.00 ama %85 palet dışı kaldı). Yeni tarif gerek |
| 6 | saman balyası | teslim (doygunluk fit) | ham x1.80 KALDI idi; 60 renk > 48 (yalnızca raporlandı) |
| 7 | tuz yalağı | teslim | UYARI x1.29 |
| 8 | ahır feneri | teslim (doygunluk fit) | ham x1.80 KALDI idi; küçük nesne (14x26 px), ayak izini doldurmuyor |
| 9 | eyer sehpası | teslim | |
| 10 | hayvan çanı | teslim (doygunluk fit) | ham x1.62 KALDI idi; 55 renk > 48 (raporlandı) |
| 11 | toplama sandığı | teslim | |
| 12 | yem oluğu | teslim | UYARI x1.48 |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_nest_box_se.png | 80x64 | 40 | 24 |
| iso_item_stall_1x2_se.png | 112x96 | 56 | 32 |
| iso_item_horse_stall_1x2_se.png | 112x112 | 56 | 32 |
| iso_item_feed_trough_2x1_se.png | 112x80 | 56 | 32 |
| iso_item_hay_bale_se.png | 80x64 | 40 | 24 |
| iso_item_salt_lick_se.png | 64x64 | 32 | 24 |
| iso_item_barn_lantern_se.png | 64x80 | 32 | 24 |
| iso_item_saddle_rack_se.png | 80x72 | 40 | 24 |
| iso_item_animal_bell_se.png | 64x80 | 32 | 24 |
| iso_item_collection_box_se.png | 80x72 | 40 | 24 |
| iso_item_feed_chute_2x1_se.png | 112x96 | 56 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Renk | Yarı saydam | Maske dışı px | En alt opak satır | Sonuç |
|---|---|---|---|---|---|
| folluk | 38 | 0 | 0 | 56 | geçti |
| ahır bölmesi | 19 | 0 | 0 | 88 | geçti |
| at bölmesi | 23 | 0 | 0 | 104 | geçti |
| yemlik | 19 | 0 | 0 | 72 | geçti |
| su yalağı (teslim değil) | 35 | 0 | 0 | 72 | geçti (stil KALDI) |
| saman balyası | 60 | 0 | 0 | 56 | renk sınırı aşıldı, raporlandı |
| tuz yalağı | 27 | 0 | 0 | 55 | geçti |
| ahır feneri | 25 | 0 | 0 | 44 | geçti (alt boşluk 35 satır, tabana değmiyor) |
| eyer sehpası | 34 | 0 | 0 | 63 | geçti |
| hayvan çanı | 55 | 0 | 0 | 62 | renk sınırı aşıldı, raporlandı (alt boşluk 17) |
| toplama sandığı | 31 | 0 | 0 | 64 | geçti |
| yem oluğu | 22 | 0 | 0 | 88 | geçti |

## Stil denetimi (`style.py check --family esya`, teslim edilenler)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| folluk | %89 | 8.8 | x0.95 | GEÇTİ |
| ahır bölmesi | %100 | 6.8 | x0.72 | GEÇTİ |
| at bölmesi | %100 | 11.4 | x0.71 | UYARI |
| yemlik | %100 | 4.4 | x0.79 | GEÇTİ |
| tuz yalağı | %94 | 9.7 | x1.29 | UYARI |
| eyer sehpası | %100 | 8.0 | x1.02 | GEÇTİ |
| toplama sandığı | %100 | 5.5 | x0.92 | GEÇTİ |
| yem oluğu | %92 | 7.7 | x1.48 | UYARI |
| saman balyası (fit) | %92 | 11.1 | x1.15 | UYARI |
| ahır feneri (fit) | %88 | 9.7 | x1.17 | GEÇTİ |
| hayvan çanı (fit) | %100 | 7.0 | x1.26 | UYARI |
| su yalağı (teslim değil) | %85 | 16.2 | x0.36 | KALDI |

## Ek ölçümler
- Maske dışı piksel yok. Zemin parçası olmadığı için döşeme testi yapılmadı. Gölge pişirilmemiş.
- Sol/sağ ortalama parlaklık: ahır bölmesi 69/75, yemlik 34/37, eyer sehpası 51/51, at bölmesi 54/54, sandık 26/33, yem oluğu 66/68 (ayrışmıyor, düz); folluk 57/69, tuz 131/187, çan 49/84 (sağ aydınlık, ışık sağdan ⚠); saman 182/140 ve fener 68/57 (sol aydınlık, doğru).
- Fener ve çan küçük ve tabana değmiyor; kullanıcı yan yana sayfasından karar versin.
- Ham (fit öncesi) saman, fener, çan ve su yalağı `338-red/` altında.

## Sorunlar
- Su yalağı teslim edilmedi; yeni tarif ("grey-brown stone trough with dark water") gerek.
- Base64 aktarımında kılavuz ve maske kopyaları birkaç kez bozuldu (sunucu reddetti, ücret yok). Öneri: kılavuzu `upload:` ile yükleyen bir araç.
- Hesap Tier 2 görünüyor (5000 gen); `art/budget.md` Tier 1 diyor, kullanıcı güncelleyebilir.
