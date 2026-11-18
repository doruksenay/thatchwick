unity-import: pending

# 354 Su gücüyle çalışan düzenekler (rapor)

Yan yana sayfası: [354-yanyana.png](354-yanyana.png). Teslim edilmeyenler: `art/results/354-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`footprint_guide.py`; hızar `--n 2 --m 2 --height 64`, 144x128; dink ve çekiç `--n 2 --axis x --height 56`, 112x104), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 3 çağrı x 1 gen = 3 generation (cost_max 10), tek deneme.
`get_balance`: iş başı 4968, iş sonu 4955 (paralel şeritler de harcıyor; bu işin payı 3).
Hız sınırı hatası yok. Hızar için iki çağrı base64 bozulmasıyla üretime girmeden reddedildi (ücret yok); kodlama değiştirilip gönderildi.

## ⚠ Hızar kılavuz hatası
Hızarın üretim çağrısına, işçi hatasıyla, `background_image` olarak kılavuz yerine **maskenin 1 bit sürümü** (siyah-beyaz elmas) gitti; `inpainting` maskesi doğruydu. Çıktı ayak izi içinde ve düzgün, ama kılavuz bağlamı olmadan üretildi. Tek deneme kuralı nedeniyle yeniden denenmedi. Kullanıcı yan yana sayfasına bakıp karar versin.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | su hızarı 2x2 | teslim (⚠ inceleme) | ahşap platform, iki direk, üstte yatay kiriş, sol üstte testere/çark benzeri parça, yanda kütük. Dişli ve testere bıçağı belirsiz |
| 2 | dink 2x1 | TESLİM EDİLMEDİ | ayak izinin üst yarısına sıkışmış (en alt opak y=71, taban elması y 48-95), tabana değmiyor; tek ayaklı masa gibi, tekne yok. Doygunluk x1.38 (UYARI) |
| 3 | su çekici 2x1 | TESLİM EDİLMEDİ | ince uzun sırık ve küçük kaide, en alt opak y=71, tabana değmiyor; çekiç başı ve kam mili tanınmıyor |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_sawmill_2x2_se.png | 144x128 | 72 | 40 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| hızar | x23-135, y11-109 | 32 | 0 | yok | geçti |
| dink (teslim değil) | x25-102, y10-71 | 27 | 0 | yok | geçti (şekil tutmadı) |
| çekiç (teslim değil) | x8-103, y27-71 | 21 | 0 | yok | geçti (şekil tutmadı) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| hızar | %100 | 9.9 | x0.80 | GEÇTİ |
| dink | %100 | 6.3 | x1.38 | UYARI (doygun) |
| çekiç | %100 | 5.0 | x0.84 | GEÇTİ |

## Sorunlar
- Dink ve su çekici baştan gerekir; daha basit, tabana oturan tarif önerilir (ör. "wooden trough on the ground with two hammers above it"). Yeni iş dosyası yazılmadı.
- Hızarda dişli dili ve su çarkı bağlantısı zayıf; 339 su çarkıyla ahşap tonu yakın.
- Zemin değil: döşeme testi yok. Işık sol-üstten, gölge pişirilmemiş. Ayak izi kaplaması ölçülmedi.
