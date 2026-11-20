unity-import: pending

# 621 Maden hazır mekân eşyaları (rapor)

Yan yana sayfası: [621-yanyana.png](621-yanyana.png). Teslim edilmeyen: `art/results/621-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py --axis x`; yükseklikler: prop 60, sandık 26, el arabası 36, kristal 50, küçük kristal 30, havuz taşı 14, niş 56), `high top-down`, `low detail`, `basic shading`, `single color outline`, 362 yöntemi; tarif eki: mine gallery + sol-üst lamba. 7 çağrı (paralel) x 1 gen = 7 generation. Tek deneme. 3 çağrının base64 argümanı iletimde bozulduğu için sunucu reddetti (üretim ve ücret yok), aynı kılavuzla yeniden gönderildi; bu yeniden deneme değil, ilk geçerli çağrıdır. `get_balance` iş sonu: 891,6 (paralel şeritler de harcıyor; bu işin payı 7).

## Sonuç özeti
| Dosya | Durum | Not |
|---|---|---|
| old_mine_chest | teslim (⚠) | tanınır demir bantlı sandık, ayak izinin yalnızca ~30 px'ini dolduruyor; soluk (x0.57) |
| old_lamp_niche | teslim | kayada kovuk + yağ kandili okunuyor, palet iyi |
| rotten_prop | TESLİM EDİLMEDİ | destek yerine merdiven + platform çıktı; stil KALDI (doygun x1.35, ışık payı %37) |
| broken_barrow_2x1 | TESLİM EDİLMEDİ | el arabası okunmuyor, zemin elması ve uzun çizgi artığı; validate_iso renk 51 > 48 |
| crystal_cluster | TESLİM EDİLMEDİ | kırmızı kaya + mavi kristal, palet dışı (%76), doygun x1.38 KALDI |
| crystal_cluster_small | TESLİM EDİLMEDİ | nesne yerine duvarlı, pencereli küçük oda çıktı |
| pool_edge_stones | TESLİM EDİLMEDİ | taş yok, yalnızca turkuaz zemin karosu; palet KALDI (%58) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_old_mine_chest_se.png | 80x72 | 40 | 24 |
| iso_item_old_lamp_niche_se.png | 80x96 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| chest | x25-55, y30-57 | 31 | 0 | yok | geçti |
| niche | x21-60, y31-70 | 38 | 0 | yok | geçti |
| rotten_prop (red) | x10-67, y19-104 | 43 | 0 | yok | geçti (şekil kabul edilmedi) |
| barrow (red) | x9-100, y7-65 | 51 | 0 | yok | BAŞARISIZ (renk) |
| crystal (red) | x17-63, y24-73 | 27 | 0 | yok | geçti (şekil/palet kabul edilmedi) |
| crystal_s (red) | x10-69, y7-56 | 13 | 0 | yok | geçti (şekil kabul edilmedi) |
| pool (red) | x10-69, y9-48 | 39 | 0 | yok | geçti (palet kabul edilmedi) |

## Stil denetimi (`style.py check --family esya --light`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| chest | %100 | 6.0 | x0.57 | UYARI (soluk) |
| niche | %100 | 6.6 | x0.73 | GEÇTİ |
| rotten_prop | %95 | 9.6 | x1.35 | KALDI |
| barrow | %97 | 6.0 | x0.93 | GEÇTİ (şekil kabul edilmedi) |
| crystal | %76 | 13.2 | x1.38 | KALDI |
| crystal_s | %95 | 8.5 | x0.65 | GEÇTİ (şekil kabul edilmedi) |
| pool | %58 | 16.5 | x1.20 | KALDI |

## Sorunlar
- 7 işin 5'i teslim edilemedi. Stil eşleme "mine gallery" ifadesiyle sahne/oda üretmeye eğilimli (küçük kristal, niş). Tek deneme kuralı gereği tekrar edilmedi. Öneri: "inside a dark mine gallery" ekini kaldırıp 301 ortak ekine dönmek; havuz taşı ve el arabası için ayrı iş (kısa tarif, tek nesne). Kristal için `fit` doygunluğu düzeltir ama palet dışı kırmızı kayayı ve şekli düzeltmez.
- Gölge pişirilmemiş; ışık sol-üstten. Zemin parçası olmadığı için döşeme testi yapılmadı (havuz taşı zemin benzeri çıktı ama teslim edilmedi).
