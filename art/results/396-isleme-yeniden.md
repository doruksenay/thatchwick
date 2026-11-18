unity-import: pending

# 396 İşleme yeniden: kömür yığını, çiçek ocağı (rapor)

Yan yana sayfası: [396-yanyana.png](396-yanyana.png). Teslim edilmeyen: `art/results/396-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py --w 144 --h 112 --n 2 --m 2 --axis x`; yükseklik kömür 36, ocak 48), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki, tarifteki açıklamalar. 2 çağrı, 2 generation (cost_max 10). Çiçek ocağı çağrısının ilk iki denemesi istemci tarafı base64 doğrulama hatasıyla üretime girmedi (ücret yok); maske yeniden kodlandı. `get_balance` iş sonu: 4394,90 (paralel şeritlerin harcaması dahil; bu işin payı 2).

## Sonuç özeti
| # | Yapı | Durum | Not |
|---|---|---|---|
| 1 | kömür yığını | TESLİM EDİLMEDİ | şekil tanınıyor (koyu toprak/çim kubbe, ağız, duman yerine bir alet), ama stil KALDI: palete yakın %68, `fit` doygunluğu x0.99 zaten aileye yakın, düzeltmedi. Ham `396-red/`de |
| 2 | çiçek ocağı | teslim (UYARI) | körük tanınmıyor: solda yatan namlu/varil benzeri silindir var, ocak gövdesi bel boyu değil iki katlı ahşap-kil yapı; zemin dahil taban dolu |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_bloomery_b_2x2_se.png | 144x112 | 72 | 40 |

## Doğrulama
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Ayak izi kaplaması | Sonuç |
|---|---|---|---|---|---|
| çiçek ocağı | x8-135, y18-104 | 18 | 0 | %99,5 (eşik %70) | geçti |
| kömür yığını (red) | x8-135, y8-104 | 38 | 0 | tam | geçti (şekil), stil KALDI |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| çiçek ocağı | %100 | 6.2 | x0.80 | GEÇTİ |
| kömür yığını | %68 | 12.8 | x0.99 | KALDI |

## İstenmeyen ayrıntı
Pişmiş gölge yok; her iki görselde zemin (çim/toprak elması) üretilmiş: ayak izini dolduruyor ama "no ground" isteğine uymuyor. Işık yönü ölçülmedi. Kömür yığınında kömür/kütük yerine tepede alet benzeri bir şey var. Çiçek ocağında körük ⚠ (tanınmıyor), yükseklik beklenenden fazla.

## Sorunlar
- Tek deneme kuralı gereği kömür yığını tekrar edilmedi.
- Öneri: kömür yığını için palet dışı renkleri azaltan yeni bir tarif (`footprint_guide` ile `--height` düşürülmüş) ya da elle düzeltme.
