unity-import: pending

# 487 Kaz, ada tavşanı, güvercin: teslim edildi (3 aday, 24 PNG)

Sonuç: `status: review`. Masaüstü kopyası: yerel (bulut modu).
Yan yana sayfası (goose, rabbit, pigeon SE; referans koyun/domuz/tavuk): `art/results/487-yanyana.png`

## Araç ve ayarlar (375 yol B)
- `create_image_pixflux` 32x32 (en küçük alan), `view=high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading. Palet zorlaması `iso_animal_sheep_1_cand_se.png`. Açıklama ekine `desaturated` eklendi. Üç çağrı paralel.
- Güney kontrolü: üçü de önden bakıyor, zemin lekesi yok; hepsi döndürüldü.
- v3 döndürme: `create_character mode=v3` + referans, `high top-down`. Kaz ve güvercin `humanoid` (kuş yolu B'), tavşan `quadruped` şablon `cat` (oturan poz, 375 kedisi gibi).
- `style.py fit --family hayvan` her yöne uygulandı (SE: x1.00, ham zaten aile altı).

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x3 | 3 |
| create_character v3 ref x3 | 3 |
| Toplam | 6 |

`get_balance` sonrası: 2798,9 (paralel şeritlerin harcaması dahil).

## Dosyalar (`Assets/Art/Iso/_aday/iso_animal_<tür>_cand_{s,se,sw,w,nw,n,ne,e}.png`)
Hepsi 32x32, yarı saydam piksel 0. SE değerleri; pivot y alttan, pivot x bbox ortası.
| Tür | SE boy x en | Pivot x (SE) | Pivot y (alttan) | Renk (SE) | Stil denetimi (8 yön) |
|---|---|---|---|---|---|
| goose | 30 x 19 | 15,5 | 0 (yönlere göre 0-1) | 16 | 8 UYARI (soluk, x0,31-0,56), KALDI yok |
| rabbit | 28 x 18 | 15,0 | 2 (hepsinde 2) | 11 | 8 UYARI (soluk, x0,21-0,23), KALDI yok |
| pigeon | 30 x 23 | 13,5 | 0 (0-1) | 15 | 8 UYARI (soluk, x0,27-0,30), KALDI yok |

`validate_iso.py --kind object` SE: üçü de geçti. Hayvan ailesinin kilidi kurulmadı; palet uyumu yalnızca uyarı. Gölge pişirilmemiş, ışık sol-üstten, 8 yön tutarlı.

## Sorunlar
- **Güvercin çok iri:** 30 px, kazla aynı boyda ve tavuktan (~24) büyük; iş dosyasındaki "güvercin tavuktan küçük" ölçütü tutmadı. Pixflux'ın en küçük tuvali 32x32, yeniden örnekleme yapılmadı. Öneri: pixen 20-24 tuvalle yeniden dene ya da oyunda ölçekle küçült.
- Kaz 30 px (hedef ~30) tamam; ördekten iri.
- Tavşan oturan poz (28 px boy), kedi gibi; ada tavşanı için boy iri olabilir, görsel onayda bak.
- Üç tür de aileden soluk (UYARI), kullanıcı yan yana sayfasına bakmalı.
