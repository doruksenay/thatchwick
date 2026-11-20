unity-import: pending

# 479 Sera eşyaları: fidan rafı ve saksı tezgâhı (rapor)

Yan yana sayfası: [479-yanyana.png](479-yanyana.png) (fidan rafı ve reddedilen tezgâh). Teslim edilmeyen: `art/results/479-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Çağrılar paralel. 2 generation (cost_max 6 içinde). `get_balance` iş sonu: 2489,9 (paralel şeritler de harcıyor).

## Sonuç
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | fidan rafı | teslim | ham çıktı çok doygundu (x1,87, KALDI); `style.py fit` ile doygunluk indirildi (x1,04), GEÇTİ. Ham hâl `479-red/` içinde |
| 2 | saksı tezgâhı | TESLİM EDİLMEDİ | 51 renk (>48); `fit` sonrası da 51. İkinci deneme koşulu (ayak izi/tanınma) yok, tekrar denenmedi |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_seedling_rack_se.png | 80x88 | 40 | 24 |

## Doğrulama
| Dosya | Opak sınır | Renk | Yarı saydam | Alta değme | validate_iso | style check |
|---|---|---|---|---|---|---|
| rack (fit) | x10-66, y8-80 | 36 | 0 | yok | geçti | GEÇTİ (%92, 6,7, x1,04) |
| rack (ham) | aynı | 36 | 0 | yok | geçti | KALDI (%85, 11,3, x1,87) |
| bench (red) | x10-101, y14-80 | 51 | 0 | yok | BAŞARISIZ (renk) | GEÇTİ (%97, 9,5, x1,22) |

## Sorunlar
- Tezgâh yalnızca renk sayısında kaldı (51/48); şekil olarak tanınıyor. Öneri: 479b ile yeniden deneme ya da `reduce_colors` izni (kullanıcı kararı).
- Ölçülmedi: ışık sırası, ayna uygunluğu. Kullanıcı yan yana sayfasına bakmalı.
