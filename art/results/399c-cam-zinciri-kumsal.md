unity-import: pending

# 399c Cam zinciri yapıları ve kumsal (rapor)

Yan yana sayfası: [399c-yanyana.png](399c-yanyana.png).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`; ocak `--n 2 --m 2 --axis x`, tezgâh `--n 2 --axis x`, kalbur ve kumsal `--n 1`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki (kumsalda "dark brown wood", "no ground", "no shadow" çıkarıldı). Tahmini yükseklikler: ocak 52, tezgâh 36, kalbur 26, kumsal 8 px. 4 çağrı, çağrı başına 1 generation (3 paralel + 1), tek deneme, yeniden deneme yok. `--light` kılavuz aracında yok, kullanılmadı. `get_balance` iş sonu: 9665 (paralel şeritler de harcıyor; bu işin payı 4).

## Sonuç özeti
| # | Yapı | Durum | Not |
|---|---|---|---|
| 1 | cam ocağı | teslim | kubbeli kahverengi ocak, üstte parlak ağız, yanda boru; kömür yığını ve duman yok; ayak izini doldurmuyor (x45-99 / 128 px) |
| 2 | camcı tezgâhı | teslim | tezgâh üstünde şişeler, boru/maşa benzeri çubuklar ve yeşil kaplar; tanınıyor |
| 3 | kalbur | teslim | yuvarlak ahşap kalbur, ağ dokulu; alttaki kum yığını görünmüyor |
| 4 | kumsal | teslim (UYARI) | kum yaması, çakıl, kamışlar; doygunluk x1.31 |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_glass_furnace_2x2_se.png | 144x112 | 72 | 40 |
| iso_item_glass_bench_2x1_se.png | 112x96 | 56 | 32 |
| iso_item_sieve_se.png | 80x64 | 40 | 24 |
| iso_item_forage_sand_se.png | 80x48 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| cam ocağı | x45-99, y26-84 | 26 | 0 | geçti |
| camcı tezgâhı | x10-101, y22-84 | 45 | 0 | geçti |
| kalbur | x14-65, y7-56 | 5 | 0 | geçti |
| kumsal | x14-66, y4-38 | 9 | 0 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| cam ocağı | %93 | 7.7 | x1.05 | GEÇTİ |
| camcı tezgâhı | %94 | 6.3 | x0.88 | GEÇTİ |
| kalbur | %100 | 4.5 | x0.66 | GEÇTİ |
| kumsal | %100 | 9.4 | x1.31 | UYARI (ailesinden doygun) |

## Gözle bakılacaklar
Pişmiş gölge yok. Işık yönü ölçülmedi. Cam ocağı ayak izini doldurmuyor ve kömür yığını yok; kalburda kum yığını yok; kumsal alt kenarda tabana tam oturmuyor (en alt opak satır y=38, taban 39). Kullanıcı yan yana sayfaya bakmalı.

## Sorunlar
Tek deneme kuralı gereği ocak, kalbur ve kumsal eksik ayrıntılar için yeniden üretilmedi. Öneri: `footprint_guide.py`'ye `--light` ekle.
