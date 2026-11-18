unity-import: pending

# 401 Oyuncak (4) ve ürün ikonu (5) yeniden (durum: review)

Yan yana sayfası: [401-yanyana.png](401-yanyana.png) (yalnızca oyuncaklar; ikonlar için esya referansı yok). Ham (düzeltilmemiş) hâller: `art/results/401-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 9 x `create_map_object` **basic mod**, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline`; her açıklamanın sonuna ders ekindeki "no face, no eyes..." eki. 9 çağrı = **9 gen** (cost_est 9, cost_max 12), paralel gönderildi, hız sınırı olmadı. `get_balance` sonrası: **4274,9** (paralel şeritler dahil).
- Gerekçe: bulutta oyuncaklar için kılavuz+maske (399'daki `art/tmp399`) yoktu; basic mod kullanıldı, yuva/zemin tabanı kılavuzu uygulanmadı (kullanıcı yuva uyumuna bakmalı).
- Doygunluk düzeltmesi (`style.py fit`): icon_oak_gall (x1,26 → fit sonrası x1,15 denetimde, KALDI → UYARI) ve icon_malt (x1,56 → x1,15, KALDI → UYARI). Şekil tanınıyor, bu yüzden fit hâli teslim edildi; ham hâller `401-red/*-ham.png`.

## Dosyalar (pivot x, pivot y ALTTAN)
| Yol (`Assets/Art/Iso/_aday/`) | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| iso_item_hoop_se_v2.png | 32x32 | 16 | 10 |
| iso_item_marbles_se_v2.png | 32x32 | 16 | 7 |
| iso_item_whittling_knife_se.png | 32x32 | 16 | 7 |
| iso_item_juggling_balls_se.png | 32x32 | 16 | 7 |
| icon_tallow.png | 32x32 | 16 | 0 (ikon) |
| icon_oak_gall.png | 32x32 | 16 | 0 (ikon) |
| icon_ink.png | 32x32 | 16 | 0 (ikon) |
| icon_malt.png | 32x32 | 16 | 0 (ikon) |
| icon_blank_book_v2.png | 32x32 | 16 | 0 (ikon) |

## Doğrulama (validate_iso --kind object --max-colors 48: hepsi geçti) ve stil denetimi
| Dosya | Renk | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|
| hoop_v2 | 17 | %100 | x0.71 | GEÇTİ |
| marbles_v2 | 9 | %100 | x0.56 | UYARI (soluk) |
| whittling_knife | 31 | %100 | x1.04 | GEÇTİ |
| juggling_balls | 17 | %83 | x1.31 | UYARI (palete uzak, doygun) |
| icon_tallow | 15 | %100 | x1.24 | GEÇTİ |
| icon_oak_gall (fit) | 15 | %91 | x1.15 | UYARI |
| icon_ink | 14 | %98 | x0.67 | UYARI |
| icon_malt (fit) | 35 | %86 | x1.15 | UYARI |
| icon_blank_book_v2 | 14 | %100 | x0.69 | GEÇTİ |

Gözle: yüz/göz/figür yok; çember+sopa, kese+5 bilye, bıçak+yontulmuş çubuk, üç top, yağ kasesi, mazı dalı, hokka+kaz tüyü, arpa yığını, deri kitap hepsi tanınıyor.

## Sorunlar
- Fit sonrası oak_gall yarı saydam olmayan alanı tuvali dolduruyor (alt boşluk 0), malt 35 renk (limit 48 içinde). Mazılar sarımsı-pembe, açık kahveye yakın değil: kullanıcı bakmalı.
- Oyuncaklar yuva/zemin tabanına göre doğrulanmadı (kılavuzsuz). Döşeme testi gerekmedi.
