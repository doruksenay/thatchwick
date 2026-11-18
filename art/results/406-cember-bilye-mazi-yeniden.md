unity-import: pending

# 406 Çember, bilye, mazı yeniden (durum: review)

Yan yana sayfası: [406-yanyana.png](406-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 3 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (401 ile aynı yöntem), tarifteki açıklamalar + "simple still-life object, no face..." eki. Paralel gönderildi, hız sınırı olmadı. 3 çağrı = **3 gen** (cost_est 3, cost_max 6).
- `get_balance` sonrası: **4154,9** (paralel şeritler dahil).
- `style.py fit` gerekmedi (doygunluk UYARI, şekil tanınıyor).

## Dosyalar (pivot x, pivot y ALTTAN)
| Yol (`Assets/Art/Iso/_aday/`) | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| iso_item_hoop_se_v3.png | 32x32 | 16 | 10 |
| iso_item_marbles_se_v3.png | 32x32 | 16 | 7 |
| icon_oak_gall_v2.png | 32x32 | 16 | 0 (ikon) |

## Doğrulama (validate_iso --kind object --max-colors 48: hepsi geçti) ve stil denetimi
| Dosya | Renk | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|
| hoop_v3 | 15 | %100 | x1.27 | UYARI (ailesinden doygun) |
| marbles_v3 | 17 | %100 | x1.25 | UYARI (ailesinden doygun) |
| oak_gall_v2 | 25 | %100 | x0.65 | GEÇTİ |

Gözle (32x32 küçük önizleme): çember tek halka + sopa okunuyor; kese kahve, bilyeler kese ağzında ve yanında; mazı üç yuvarlak koyu kahve top, dalda, pembe değil. Kullanıcı yan yana sayfasına bakmalı (çember inceliği, bilyelerin kese içi/dışı).

## Sorunlar
- Oyuncaklar yuva/zemin kılavuzsuz üretildi; yuva uyumu doğrulanmadı.
- Dosya kopyası `cp` yerine Python ile yapıldı (var olan dosya üzerine yazılmadı).
