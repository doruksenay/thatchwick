unity-import: pending

# 362 Kazı destekleri, kuyu inişi, dren çukuru (rapor)

Yan yana sayfası: [362-yanyana.png](362-yanyana.png). Teslim edilmeyen: `art/results/362-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py --n 1 --axis x`; destek 80x96 yükseklik 60, çukur 80x56 yükseklik 10, merdiven 80x96 yükseklik 44), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 5 çağrı (paralel) x 1 gen = 5 generation. Tek deneme (`max_attempts: 1`). `get_balance` iş sonu: 4561,9 (paralel şeritler de harcıyor; bu işin payı 5).

## Sonuç özeti
| Dosya | Durum | Not |
|---|---|---|
| mine_support_wood | teslim (⚠) | iki dikme ve üst kiriş yerine A çatkılı kesme tezgâhı gibi; tonu kırmızımsı-mor, "palete uzak" uyarısı. Tanınır ama ton sorunlu, kullanıcı gözüyle karar versin |
| mine_support_stone | teslim (⚠) | taş kemerli kapı/niş; soluk (x0.41) |
| mine_support_rotten | teslim | sarkan kiriş okunuyor; koyu, hafif mor-kırmızı ton |
| shaft_ladder | teslim (⚠) | merdiven yatık duruyor, kuyu deliği/zemin yok (yalnızca merdiven); doygun (x1.41) |
| drain_pit | TESLİM EDİLMEDİ | çukur yerine duvarlı, yüksek kenarlı kutu/oda çıktı; çakıl çukuru okunmuyor (`362-red/`) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_mine_support_wood_se.png | 80x96 | 40 | 24 |
| iso_item_mine_support_stone_se.png | 80x96 | 40 | 24 |
| iso_item_mine_support_rotten_se.png | 80x96 | 40 | 24 |
| iso_item_shaft_ladder_se.png | 80x96 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`, ayak izi 8,56,64,32; çukurda 8,16,64,32)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| wood | x15-65, y11-88 | 28 | 0 | yok | geçti |
| stone | x10-69, y8-88 | 24 | 0 | yok | geçti |
| rotten | x8-71, y15-77 | 25 | 0 | yok | geçti |
| ladder | x22-52, y36-75 | 38 | 0 | yok | geçti |
| pit (teslim değil) | x10-69, y5-48 | 14 | 0 | yok | geçti (şekil kabul edilmedi) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| wood | %95 | 14.4 | x1.04 | UYARI (palete uzak) |
| stone | %100 | 8.3 | x0.41 | UYARI (soluk) |
| rotten | %96 | 7.9 | x1.12 | GEÇTİ |
| ladder | %96 | 7.1 | x1.41 | UYARI (doygun) |
| pit | %99 | 7.9 | x0.74 | GEÇTİ |

## Sorunlar
- Çukur ve merdiven "zemin üstü nesne" olarak değil kendi başına çıktı; kuyu deliği ve çakıl dolgu istenen biçimde okunmuyor. Yeni tarif önerisi: çukur için zemin karosu yaklaşımı (`create_isometric_tile`, koyu delik + çakıl), merdiven için "square dark hole" ayrı bir karo, merdiven üstüne bindirme.
- Ahşap destek kırmızımsı; ton ipucu ("warm orange-brown pine") ile yeniden denenebilir. Tek deneme kuralı gereği tekrar edilmedi.
- Gölge pişirilmemiş; ışık büyük ölçüde sol-üstten. Zemin parçası olmadığı için döşeme testi yapılmadı.
