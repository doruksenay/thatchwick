unity-import: pending

# 101 Hayvanlar 1: tavuk 3/4/5 teslim, horoz adayları elendi

Yan yana sayfası: `art/results/101-yanyana.png` (referanslar + 3 tavuk SE). Azaltılmış (fit'siz) hâller ve horoz tek kareleri: `art/results/101-red/`. Masaüstü kopyası: yerel.

## Çağrılar
| Çağrı | Gen |
|---|---|
| (önceki tur, şerit 0) pixen x4 + v3 döndürme x3 | 7 |
| pixen 32 rooster_1..4 (bu tur) | 4 |
| reduce_colors x3 (32 renk, 8 kare birlikte) | 0,3 |
| `style.py fit` (yerel) | 0 |

Toplam ≈ 11,3 (cost_max 20). `get_balance`: öncesi 4928,6, sonrası 4914,3 (paralel şeritlerin harcaması karışık). chicken_3/4/5 karakterleri kayıtlı kimliklerle indirildi, yeniden üretilmedi. Yükleme: `get_upload_url` + Python urllib.

## Tavuk adayları (32x32 tuval, 8 yön)
| Aday | Yol | Güney kontrolü | SE boy / uzunluk | Hedef 24'e sapma | Renk ham→azalt | Zemin lekesi | Not |
|---|---|---|---|---|---|---|---|
| chicken_3 | pixen 28 + v3 | önden | 25 / 17 | +4% | 35→25 | yok | temiz |
| chicken_4 | pixen 28 + v3 | önden | 24 / 20 | 0% | 57→29 | yok | temiz |
| chicken_5 | pixen 32 + v3 | önden, tuvale dayalı | 30 / 22 | +25% | 48→31 | yok | ⚠ insansı: kanatlar kol gibi açık, yeşil giysi benzeri alt gövde; hedeften büyük |
| chicken_6 | pixen 32 | 3/4 dönük | - | - | - | - | döndürülmedi (önceki tur) |

Azaltma bozdu mu: SE ham/azaltılmış 8x karşılaştırmasında göz, gaga, ibik ve dış hat bozulmadı. Üçü de "ailesinden çok doygun" (x2.18 / x2.04 / x1.67) yüzünden KALDI'ydı; şekil tanınıyor, bu yüzden `style.py fit --family hayvan` uygulandı (x0.53-0.78 aralığı, kare başına). Fit sonrası SE: UYARI (x1.46 / x1.31 / x1.32 "ailesinden doygun"), KALDI yok; hayvan ailesinin kilidi kurulmadı. Assets'e fit'li hâl gitti; yönler arası doygunluk hafif tutarsız olabilir.

## Doğrulama (SE; pivot ALTTAN)
| Dosya | Boyut | boy x uzunluk | Renk ham/azaltılmış | Yarı saydam | Pivot x | Pivot y (yön aralığı) |
|---|---|---|---|---|---|---|
| chicken_3 se | 32x32 | 25x17 | 35/25 | 0 | 16,0 | 3 (3-4) |
| chicken_4 se | 32x32 | 24x20 | 57/29 | 0 | 17,0 | 4 (4-5) |
| chicken_5 se | 32x32 | 30x22 | 48/31 | 0 | 17,0 | 1 (1-2) |

Diğer yönler: boy 24-25 (c3, c4) ve 30-31 (c5); pivot y oynaması ≤1 px; yarı saydam 0; `validate_iso.py --kind object` SE'lerde geçti. Gölge pişirilmemiş, ışık sol-üstten.

## Doluluk oranı
SE boy / 32: chicken_3 0,78, chicken_4 0,75, chicken_5 0,94. Tek kare (28 tuval) ölçümü: 0,89 / 0,86 / 0,94; ortalama r ≈ 0,90. **Kural R:** 28 / 0,90 = 31,1 → en yakın 4'ün katı **32** (horoz tuvali). Öneri (tahmin): 102/103 için r ≈ 0,86-0,90; 32 tuvalde gövde ~30 çıkıyor, küçük kuşlar için 28 ve altı.

## Horozlar: döndürülmedi
Dördü de pixen 32'de yandan/3/4 çıktı (güney değil); kural gereği döndürme yok. Tek kareler: `art/results/101-red/rooster_N_tek.png`, `roosters_tek_x8.png`.
| Aday | SE boy | Sorun |
|---|---|---|
| rooster_1 | 30 | yandan, güney değil |
| rooster_2 | 32 | yandan, tuvale dayalı, ayak altı gölge |
| rooster_3 | 28 | yandan, ayak altı toprak lekesi |
| rooster_4 | 32 | 3/4, tuvale dayalı, ayak altı mavi gölge |

Öneri: horozu "symmetrical front view" vurgusuyla ya da tavuk gibi 28 tuvalde yeniden dene (tahmin).

## Sorunlar
- chicken_6 ve 4 horoz teslim edilmedi; iş eksik, yeni iş gerekir.
- chicken_5 insansı; tercih notu.
- Bulutta `echo >` yönlendirmesi onay istediği için geçici betikler Write ile yazıldı (`art/tmp101`, commit edilmez).

## Dosyalar
`Assets/Art/Iso/_aday/iso_animal_chicken_{3,4,5}_cand_{se,sw,nw,ne,s,w,n,e}.png` (24 dosya, 32x32).
