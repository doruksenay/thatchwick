unity-import: pending

# 358 Zemin süsleri (bulut şeridi 0)

- Araç: `create_map_object`, stil eşleme + özel maske, 8 çağrı paralel, çağrı başına 1 gen (toplam 8; `get_balance` düşüşü 9,0: bakiye 4694,90 → 4685,90, paralel şeritlerin küçük adımları dahil olabilir).
- Kalan (`get_balance`): 4685,9.
- Sapma: araç en az 32 px kenar istiyor (24 yok). 32x32 saydam tuval kullanıldı, maske alt 24 satır (y 8-31); sonuç yalnızca maske bölgesine kırpıldı → 32x24. Maske dışı boştu, içerik yeniden boyanmadı.
- Referans: tuval saydam olduğundan (tarif gereği) stil referansı yalnızca açıklamadaki "grass tile" ifadesi; aileden palet uyumu `fit` ile sağlandı.
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x24, pivot x 16, y 8 ALTTAN)
| Dosya | Not |
|---|---|
| iso_decal_clover.png | doygunluk x2,53 → x0,45 (`fit`), ham hâli ilk denetimde KALDI |
| iso_decal_grass_tuft.png | doygunluk x3,10 → x0,37 (`fit`) |
| iso_decal_leaves_autumn.png | doygunluk x2,41 → x0,48 (`fit`), GEÇTİ |
| iso_decal_fern.png | UYARI (palet) |
| iso_decal_pebbles.png | UYARI (palete uzak, soluk) |

## Teslim edilmeyenler (`art/358-red/`)
- buttercups, molehill, daisies: model altına çimen karosu/plaka pişirdi ("no ground tile" ihlali), çimen üstüne serpilince kare görünür. Buttercups ve molehill ayrıca KALDI (doygunluk x2,09; molehill x1,37 uyarı). Plaka şekil sorunu, `fit` çözmez. Tek deneme kuralı gereği yeniden denenmedi.

## Doğrulama
Yan yana sayfası: `art/results/358-yanyana.png`

| Dosya | Palete yakın | Doygunluk/aile | Sonuç |
|---|---|---|---|
| clover | %6 | x1,14 | UYARI |
| fern | %69 | x1,14 | UYARI |
| grass_tuft | %86 | x1,14 | UYARI |
| leaves_autumn | %100 | x1,15 | GEÇTİ |
| pebbles | %86 | x0,29 | UYARI |

`validate_iso.py`: 5/5 geçti (32x24, yarı saydam 0, en çok 17 px yükseklik; ot öbeği 12 px, eğrelti 17 px, yaprak 15 px). Döşeme testi: zemin karosu değil, uygulanmadı; süs döşemesi sayfası yapılmadı (3 süs eksik, kullanıcı yan yana sayfasına bakar). 'bitki' ailesinin kilidi kurulmadı, palet uyumu uyarı.

## Sorunlar / öneri
- 8 süsten 3'ü plakalı çıktı. Öneri: ikinci deneme için tuvale çimen rengi bağlam verilip maske sıkılaştırılabilir, ya da `create_1_direction_object` küçük boyutla; kullanıcı kararı.
- Araç alt sınırı 32 px; `tools/art` değişmedi.
