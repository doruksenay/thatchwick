unity-import: pending

# 364 İşleme yapıları (rapor)

Yan yana sayfası: [364-yanyana.png](364-yanyana.png). Teslim edilmeyenler: `art/results/364-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py --w 144 --h 112 --n 2 --m 2 --axis x`; tarifteki `--axis both` yok, `--m 2` kullanıldı), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Yükseklikler tarifte yok, tahmin: kömür yığını 36, kavurma 20, çiçek ocağı 48, kireç ocağı 48 px. 4 çağrı paralel, 4 generation (cost_max 16). İlk kılavuz base64 aktarımı bozuldu (üretime girmedi, ücret yok), 1 bit PNG ile yeniden kodlandı. `get_balance` iş sonu: 4557,90 (paralel şeritler de harcıyor; bu işin payı 4). `--light` seçeneği kılavuz aracında yok, kullanılmadı.

## Sonuç özeti
| # | Yapı | Durum | Not |
|---|---|---|---|
| 1 | kömür yığını | TESLİM EDİLMEDİ | şekil tanınıyor (kırmızımsı kütükler, çim kubbe) ama stil KALDI: palete yakın %70, `fit` sonrası %74 |
| 2 | kavurma yığını | TESLİM EDİLMEDİ | küçük kamp ateşi çıktı, cevher taşı ve taş taban yok, ayak izini doldurmuyor (x40-98, y21-74) |
| 3 | çiçek ocağı | teslim (UYARI) | kil ocak gövdesi + yanında körük yerine ayaklı direk; ayak izini doldurmuyor (x46-107); palete uzak uyarısı |
| 4 | kireç ocağı | teslim (`fit`, doygunluk x0.71 yerine aileye indirildi) | yuvarlak taş fırın, kemerli ağız, kum/kireç zemin; kırmızımsı tuğla tonu |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_bloomery_2x2_se.png | 144x112 | 72 | 40 |
| iso_item_lime_kiln_2x2_se.png | 144x112 | 72 | 40 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| çiçek ocağı | x46-107, y10-79 | 39 | 0 | geçti |
| kireç ocağı | x19-128, y15-96 | 48 | 0 | geçti |
| kömür yığını (red) | x8-135, y9-104 | 47 | 0 | geçti |
| kavurma yığını (red) | x40-98, y21-74 | 39 | 0 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| çiçek ocağı | %99 | 10.7 | x0.84 | UYARI |
| kireç ocağı (fit sonrası) | %100 | 9.1 | x1.16 | GEÇTİ (ham: %86, x1.62 KALDI; ham `364-red/`de) |
| kömür yığını | %70 (fit: %74) | 16.8 | x1.25 | KALDI |
| kavurma yığını | %67 | 20.8 | x1.04 | KALDI |

## İstenmeyen ayrıntı
Pişmiş gölge yok; kireç ocağında tarifin istediği kum/kireç zemini var. Işık yönü ölçülmedi, görsele bakılmalı. Çiçek ocağında körük tanınmıyor (⚠).

## Sorunlar
- Kavurma yığını ve çiçek ocağı tarifi tek denemede tutmadı; tek deneme kuralı gereği tekrar edilmedi.
- Öneri: `footprint_guide.py`'ye `--light` ve `--axis both` yok; tarif `--m 2` diyecek şekilde güncellenebilir.
