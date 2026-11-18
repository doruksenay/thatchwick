unity-import: pending

# 339 Tarım düzenekleri ve saraç (rapor)

Yan yana sayfası: [339-yanyana.png](339-yanyana.png). Teslim edilmeyen: `art/results/339-red/` (kanal). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`, `--axis x`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 5 çağrı x 1 gen = 5 generation, hepsi paralel, tek deneme (iş dosyası `max_attempts: 1`). `get_balance` iş sonu: 560,1 (paralel şeritler de harcıyor; bu işin payı 5).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | kanal | TESLİM EDİLMEDİ | Su yok; taş kenarların üstünde kırmızı-kahverengi çalı/dal gibi dikenler var. Kanal olarak tanınmıyor (iki kopyayla ekleşme testi de anlamsız). Tek deneme hakkı kullanıldı |
| 2 | su çarkı 2x2 | teslim | ahşap dikey çark, taş taban, iki direk; renk sayısı 51 (>48, yalnızca raporlandı) |
| 3 | değirmen taşı 2x2 | teslim | yatay taş, ahşap iskelet, huni; sol altta ekstra zincir/halka benzeri ayrıntı var |
| 4 | tabak germesi 2x1 | teslim | ahşap çerçeve, gerili deri |
| 5 | saraç tezgâhı 2x1 | teslim | tezgâh üstünde kırmızı eyer; renk sayısı 57 (>48, yalnızca raporlandı) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_water_wheel_2x2_se.png | 144x128 | 72 | 40 |
| iso_item_millstone_2x2_se.png | 144x128 | 72 | 40 |
| iso_item_tanning_rack_2x1_se.png | 112x104 | 56 | 32 |
| iso_item_saddler_bench_2x1_se.png | 112x104 | 56 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| kanal (teslim değil) | x10-69, y8-40 | 33 | 0 | yok | geçti (şekil tutmadı) |
| su çarkı | x20-123, y27-120 | 51 | 0 | yok | renk sayısı 48 üstü (iş kuralı: yalnızca raporla) |
| değirmen taşı | x21-116, y13-115 | 39 | 0 | yok | geçti |
| tabak germesi | x13-98, y6-96 | 33 | 0 | yok | geçti |
| saraç tezgâhı | x8-103, y3-94 | 57 | 0 | yok | renk sayısı 48 üstü (iş kuralı: yalnızca raporla) |

Kılavuz rengi (#786046) kalıntısı: hepsinde 0. Ayak izi kaplama yüzdesi ölçülmedi (komut kısıtı nedeniyle); yan yana sayfasında gözle: tabak germesi ve tezgâh ayak izini doldurur.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| kanal (teslim değil) | %100 | 8.1 | x0.43 | UYARI (soluk) |
| su çarkı | %97 | 7.7 | x0.77 | GEÇTİ |
| değirmen taşı | %100 | 8.2 | x0.79 | GEÇTİ |
| tabak germesi | %100 | 7.7 | x0.95 | GEÇTİ |
| saraç tezgâhı | %100 | 5.8 | x0.68 | GEÇTİ |

## Sorunlar
- Kanal baştan: "kanal" yerine açık bir tarif gerekir (ör. "flat stone trough filled with blue water, no plants"). Kullanıcı kararı; yeni iş dosyası yazılmadı.
- Değirmen taşında sol altta istenmeyen zincir benzeri ayrıntı.
- Zemin döşeme testi gerekmedi (zemin değil). Işık sol-üstten; gölge pişirilmemiş.
