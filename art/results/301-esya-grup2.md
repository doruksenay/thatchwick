unity-import: pending

# 301 Ev eşyaları grup 2 (rapor)

Yan yana sayfası: [301-yanyana.png](301-yanyana.png). Teslim edilmeyen denemeler: `art/results/301-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek. Kılavuzlar referanssız (tabure yok). Araçta `seed` parametresi yok; ikinci denemeler aynı kılavuz ve maskeyle yeniden gönderildi.

Çağrılar (her biri 1 gen): 7 ilk deneme (sandık ve tezgâhın ilk iki çağrısı base64 aktarım hatasıyla üretime girmeden reddedildi, ücret yok; kılavuz 1 bit palet PNG ile yeniden kodlandı), 3 ikinci deneme (kazan, bardak, sandık) = 10 generation. Cost_max 15 içinde.
`get_balance`: iş başı 1068.70, iş sonu 1028.70 (paralel şeritler de harcıyor; bu işin payı 10).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | raf (2x1) | teslim (UYARI) | doygunluk aileden soluk (x0.38); ayak izi sol %75 / sağ %81 |
| 2 | kazan | TESLİM EDİLMEDİ | iki deneme de tutmadı |
| 3 | sandık | teslim (2. deneme) | 1. deneme ton 330° (pembe/kırmızı kayma) idi; 2. denemede ton 3°, kırmızı kayma %0 |
| 4 | bardak | TESLİM EDİLMEDİ | iki denemede de tabana değmedi |
| 5 | oymalı sandalye | teslim | gerçek yön SE (asimetri %56, yüz hattı ve gölgeleme SE ile uyumlu) |
| 6 | yarma kütüğü | teslim | üstte küçük koyu dikdörtgen ayrıntı var (balta değil, kesik/çentik gibi); ⚠ kullanıcı bakmalı |
| 7 | pazar tezgâhı (2x1) | teslim | |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_shelf_wood_2x1_se_nw_v2.png | 112x96 | 56 | 32 |
| iso_item_chest_wood_se.png | 80x72 | 40 | 24 |
| iso_item_chair_carved_se.png | 80x88 | 40 | 24 |
| iso_item_chopping_block_se.png | 80x64 | 40 | 24 |
| iso_item_market_stall_2x1_se_nw.png | 112x136 | 56 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Boy | Renk | Yarı saydam | Alta değme | Maske dışı | Kılavuz kalıntısı | Ayak izi sol/sağ |
|---|---|---|---|---|---|---|---|---|
| raf | x9-102, y7-88 | 82 | 12 | 0 | yok | 0 | 0 | %75 / %81 |
| sandık | x9-70, y5-64 | 60 | 37 | 0 | yok | 0 | 0 | %100 / %100 |
| sandalye | x18-61, y8-80 | 73 | 21 | 0 | yok | 0 | 0 | %74 / %74 |
| kütük | x12-67, y6-56 | 51 | 21 | 0 | yok | 0 | 0 | %98 / %98 |
| tezgâh | x8-103, y3-128 | 126 | 24 | 0 | yok | 0 | 0 | %100 / %100 |

Not: sandalyenin ayak izi kaplaması %74 ama sandalye zaten dar tabanlı (4 ayak); ikinci deneme ölçütü olan "her iki yanda %80 altı" teknik olarak sağlandı, ama sandalye tanınıyor ve ayakları tabana oturuyor, bu yüzden yeniden denenmedi (kullanıcı kararı gerektirir). Eşya yükseklikleri tarif yüksekliğinden büyük (raf 82, sandalye 73, tezgâh 126 px; hedef taban + yükseklik toplamı 32+32, 24+44, 32+76 civarı), yani tuval sınırı içinde.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| raf | %100 | 4.3 | x0.38 | UYARI (soluk) |
| sandık | %97 | 4.8 | x1.10 | GEÇTİ |
| sandalye | %100 | 5.2 | x0.68 | GEÇTİ |
| kütük | %100 | 6.0 | x0.92 | GEÇTİ |
| tezgâh | %100 | 9.8 | x0.80 | GEÇTİ |
| kazan (teslim edilmedi) | %100 | 8.1 | x0.43 | UYARI |

## Palet (masa ve tabure ile)
Ortalama ton (derece, V>0.25): masa 16, tabure 20. Raf 24 (kayma yok, kırmızı piksel %0), sandık 3 (kırmızı piksel %0), sandalye 8 (kırmızı piksel %64, hafif koyu bordo; ⚠ masadan biraz kırmızı), kütük 23, tezgâh 348 (kumaş tenti kırmızımsı, "soluk kumaş" ile uyumlu, ahşap tezgâh kahve).

## Işık (ortalama parlaklık)
Üst en açık hepsinde (raf üst %35: 58, sandık 50, kütük 106, tezgâh 138). Sol/sağ yüz: raf sol 49 > sağ 45 (beklenen), kütük sol 79 / sağ 89 (sağ daha açık, ⚠ ışık ters), sandalye sol 47 / sağ 57 (ters), tezgâh sol 99 / sağ 94. Sandıkta sol = sağ (38/38), yüzler ayrışmıyor (⚠ düz).

## İstenmeyen ayrıntı
Zemin lekesi ve pişmiş gölge yok. Kazan denemelerinde alev ya da buhar yok. Kütükte üstte küçük bir ek parça (yukarıda not). Başka ekstra nesne yok.

## Ayna uygunluğu (sol/sağ şekil simetrisi)
Sandık %100 simetrik (4 yönün 4'ü tek sprite ya da ayna ile çıkar, ama sol/sağ parlaklık farkı yok), kütük %100 (tek sprite her yöne), sandalye %56 (ayna ile SE↔SW; aynada ışık sağ-üstten gelir), raf %57 ve tezgâh %67 (2x1: SE-NW ve ayna SW-NE; aynada ışık sağ-üstten). Ayna dosyası Assets'e konmadı.

## Raf yüzey yuvaları
Üst yüzeyin y aralığı (alttan) için ölçüm yapılmadı (otomatik betik yok); önizleme işaretlemesi eksik. Kullanıcı yan yana sayfasında bakmalı.

## Teslim edilmeyenler
- **Kazan:** 1. deneme (80x68, 59 renk > 48, ayak izi %76/%76): kazan ahşap sehpa üzerinde, üç bacak yok; 2. deneme (10 renk, %70/%70): kare tekne/tepsi, kazan olarak tanınmıyor, düz gölgeleme. Tarif tutmadı.
- **Bardak:** 1. deneme en alt opak satır y=20, 2. deneme y=21; taban elmasının alt yarısı y 25-28 olmalıydı, ikisi de tabana değmiyor.
- Dosyalar `art/results/301-red/` içinde (masaüstü `_red\` bulutta yok).

## Sorunlar
- PixelLab araçlarında `seed` yok; "seed değişir" kuralı uygulanamadı.
- Raf yüzey yuvası ölçümü yapılmadı.
- Sandalye, kütük ve sandıkta ışık yönü ölçümü beklenenle uyuşmuyor, kullanıcı görsele bakmalı.
