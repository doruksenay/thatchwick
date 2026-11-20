unity-import: pending

# 493 Yerleşim: kutu çizilen eşyalar (rapor): 3/10 teslim, review

Yan yana sayfası: [493-yanyana.png](493-yanyana.png). Teslim edilmeyenler: `art/results/493-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme (halılar, hendek, masa üstü, şişe, duvar eşyası) ve temel mod (tavan otları, kılavuzsuz). Kılavuz ve maske Python ile (`art/tmp/mk493.py`, commit edilmez): zemin için taban elması, duvar için "/" eğimli band. `high top-down` (zemin, masa üstü), `side` (duvar, tavan), `low detail`, `basic shading`, `single color outline`, 337/301 ortak eki + "earthy muted medieval palette, desaturated". 10 çağrı x 1 gen = 10 generation (cost_max 24). Araç alt sınırı 32 px olduğundan kitap ve dama tahtası 32x32 üretildi, 32x24'e kırpıldı (üst 8 satır boştu); şişe 32x40 üretildi. Hız sınırı (429): goblen ve otlar bir kez yeniden gönderildi, tuttu; kitap ve dama tahtası ilk gönderimde tuval alt sınırı ve base64 bozulması yüzünden doğrulama hatası aldı (üretim başlamamıştı), yeniden gönderildi. Doygunluk düzeltmesi (`style.py fit`) denendi. `get_balance` sonrası 2689,9 (paralel şeritler de harcıyor; bu işin payı 10).

## Sonuç
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | sulama hendeği | TESLİM EDİLMEDİ | ağaçlı, zeminli minyatür arazi çıktı; şekil yanlış |
| 2 | rush hasır | TESLİM EDİLMEDİ | ham KALDI (palet %28, x2,03); fit x0,57 ile stil GEÇTİ ama validate_iso: 53 renk > 48 ve elmas içinde 96 boş piksel |
| 3 | küçük yün halı 2x1 | teslim (UYARI, fit x0,80) | ham KALDI (x1,43); kırmızı-ocak şeritli; fit sonrası palete uzak %89 |
| 4 | yün halı 2x2 | TESLİM EDİLMEDİ | desen iyi ama palet dışı (%62-65), fit çözmedi |
| 5 | dama tahtası | teslim (GEÇTİ, fit x0,87) | küçük ve belirsiz (kütük gibi), kullanıcı bakmalı |
| 6 | kitap | teslim (GEÇTİ) | tanınıyor, deri kahve |
| 7 | cam şişe | TESLİM EDİLMEDİ | yeşil palet dışı (%37-38), fit çözmedi |
| 8 | goblen | TESLİM EDİLMEDİ | düz dikdörtgen çıktı, "/" duvar eğimi yok; palet %9 (fit sonrası UYARI) |
| 9 | keski rafı | TESLİM EDİLMEDİ | fit sonrası stil GEÇTİ ama validate_iso 53 renk > 48 |
| 10 | tavan otları | TESLİM EDİLMEDİ | şekil iyi (kirişten sarkan demetler) ama palet dışı (%80), soluk (x0,76) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_rug_wool_small_2x1_se.png | 96x48 | 48 | 24 |
| iso_item_morris_board_se.png | 32x24 | 16 | 8 |
| iso_item_book_se.png | 32x24 | 16 | 8 |

## Doğrulama (validate_iso)
| Dosya | Renk | Yarı saydam | Sonuç |
|---|---|---|---|
| halı 2x1 (`--footprint 0,0,96,48`) | 18 | 0 | geçti |
| dama tahtası | 7 | 0 | geçti (alt boşluk 9) |
| kitap | 7 | 0 | geçti (alt boşluk 9) |

Döşeme testi yapılmadı (halılar zemin karosu değil, üst üste konan nesne).

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| halı 2x1 (fit) | %89 | 13,8 | x1,15 | UYARI |
| dama tahtası (fit) | %100 | 5,7 | x1,15 | GEÇTİ |
| kitap | %97 | 5,1 | x1,24 | GEÇTİ |

## Sorunlar
- 7 eşya teslim edilemedi; yeniden deneme hakkı yok (max_attempts). Öneri: hendek için `create_map_object` yerine 321 yöntemi ya da elle; şişe ve otlar için `style.py` paletine yeşil ton eklenmesi kullanıcı kararı; goblen için duvar kılavuzunu `tools/art/wall_guide.py` olarak kalıcı yapmak (bulutta araç yazılmadı).
