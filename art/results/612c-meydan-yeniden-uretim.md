unity-import: pending

# 612c Köy meydanı yeniden üretim (rapor)

Sonuç: **KISMİ**. 5 üretimden 2 teslim edildi (köprü, küçük iskele). Çark, ocak ve büyük iskele teslim edilmedi. Tek deneme kuralı gereği yeniden denenmedi.
Yan yana sayfası: [612c-yanyana.png](612c-yanyana.png) (3 referans + 2 yeni). Teslim edilmeyenler: `art/results/612c-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_image_pixflux` x5 (paralel), isometric, `high top-down`, south-east, `single color outline`, `basic shading`, `low detail`, `no_background`; tuval tarifteki gibi. Her çağrı 1 generation, toplam 5. `get_balance` sonrası: 830,60 (paralel şeritler de harcıyor).
Kılavuz/stil referansı (`init_image`, `--ref`) kullanılmadı: bulutta görsel yükleme yolu yok. Bu yüzden ayak izi elması ölçülmedi.
Büyük iskele (192x256 > 40000 px) düz beyaz arka planla geldi; kenardan ve içeriden (RGB >= 240) şeffaflaştırıldı, içerik yeniden boyanmadı.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_square_bridge_v2_se.png | 192x128 | 96 | 4 |
| iso_square_scaffold_small_v2_se.png | 128x192 | 64 | 19 |

Pivotlar tahmindir (opak alt kenar boşluğu, yatay orta).

## Doğrulama (`validate_iso --kind object --max-colors 48`) ve stil
| Dosya | Renk | Opak oran | Stil (yapi) | Sonuç |
|---|---|---|---|---|
| köprü | 26 | %39,9 | %100 palete yakın, x0.32 | UYARI (soluk); teslim |
| iskele küçük | 47 | %34,0 | %100, x1.06 | GEÇTİ; teslim |
| iskele büyük (red) | 60 | %34,3 | %100, x0.82 | BAŞARISIZ (renk 60 > 48) |
| çark (red) | - | - | - | görsel ret |
| ocak (red) | - | - | - | görsel ret |

## Sorunlar
- **Çark:** kendi çim/toprak tabanı ve kova/basamak taşıyor (tarif: kendi zemini yok) ve oluk duvarı okunmuyor. Teslim edilmedi.
- **Ocak:** kazanda alev ve kırmızı/turuncu pikseller var (tarif: sönük), altında kahverengi toprak tabanı var, etrafında oturak yerine kütük sırası çizilmiş. Teslim edilmedi.
- **Büyük iskele:** 60 renk (sınır 48). Şekil tarife uygun (opak %34); ham hâli `612c-red/`de. `style.py fit` yalnızca doygunluğu indirir, renk sayısını değil; kullanıcı kabul ederse `reduce_colors` ile 48'e indirilebilir.
- **Köprü:** tek parça dolu taş yığını gibi okunuyor, kemer yalnızca bir yüzde; üstü boş. Altı saydam sayılabilir ama kemer açıklığı arkası tam geçirgen değil olabilir. Kullanıcı sayfaya bakmalı.
- Beklenen "iskele, kabul edilen yapıların önüne bindirilmiş" yan yana görseli üretilmedi (bindirme betiği yok); sayfada yalnızca referanslar ve yeni dosyalar var.
- Öneri: ocak ve çark için kılavuz (`footprint_guide.py`) + `get_upload_url` yüklemesi gereken yerel bir tur.
