unity-import: pending

# 375 Köpek ve kedi yeniden: kısmen teslim (cat_1, cat_2)

Sonuç: `status: review`. 8 pixflux tek karesi üretildi; ayak altında zemin/gölge lekesi ya da güney dışı duruş yüzünden yalnızca cat_1 ve cat_2 döndürüldü ve teslim edildi. Köpeklerin hiçbiri teslim edilemedi. Masaüstü kopyası: yerel (bulut modu).
Yan yana sayfası (cat_1, cat_2 SE; koyun 1 ve domuz 2 referans): `art/results/375-yanyana.png`

## Araç ve ayarlar
- 106 ile aynı: pixflux, `view=high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading. Palet zorlaması: `iso_animal_sheep_1_cand_se.png`'nin 10 rengi (şerit PNG). Açıklama ekine `, desaturated` eklendi. Tuvaller: dog_1 42, dog_2 38, dog_3 34, dog_4 42, kedi 32.
- v3 döndürme: `create_character mode=v3`, quadruped, `template=cat`, `view=high top-down`, 1 gen/aday.
- cat_4 ilk çağrıda hız sınırı (10/10 jobs) verdi, bir kez yeniden denendi, geçti.
- `style.py fit --family hayvan` her yön için çalıştırıldı (SE'de "x1.00"); çıktı ham ile pratikte aynı (ham zaten düşük doygunluktaydı).

## Tek kare kontrolü
| Aday | Boyut | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|
| dog_1 | 42 | önden, oturan | var (gri leke) | döndürülmedi |
| dog_2 | 38 | önden | var (gri leke) | döndürülmedi |
| dog_3 | 34 | önden | var (gri leke) | döndürülmedi |
| dog_4 | 42 | yandan (güney değil) | yok | döndürülmedi |
| cat_1 | 32 | önden, oturan | yok | döndürüldü |
| cat_2 | 32 | önden, oturan | yok | döndürüldü |
| cat_3 | 32 | önden | var (gri leke) | döndürülmedi |
| cat_4 | 32 | önden | var (gri leke) | döndürülmedi |

Öneri: köpek için "no ground disc, no floor shadow" vurgulu bir tarif ya da palet zorlamasız deneme (leke koyun paletindeki griden gelmiş olabilir).

## Teslim edilen dosyalar (`Assets/Art/Iso/_aday/`, 16 PNG)
`iso_animal_cat_1_cand_{s,se,sw,w,nw,n,ne,e}.png`, `iso_animal_cat_2_cand_{...}.png`. Hepsi 32x32, ham renk 13 (azaltma gerekmedi), yarı saydam 0, boy 27-29 px (hedef 21; oturan poz, 106'daki gibi), pivot y alttan 1-3, pivot x (SE) cat_1 14,0; cat_2 15,4.

| Aday | SE boy x en | Doluluk | Stil denetimi (8 yön) |
|---|---|---|---|
| cat_1 | 28 x 19 | 0,88 | 8 UYARI (ailesinden soluk, x0,25-0,30), KALDI yok |
| cat_2 | 28 x 20 | 0,88 | 8 UYARI (ailesinden soluk, x0,45-0,52), KALDI yok |

Hayvan ailesinin kilidi kurulmadı; palet uyumu yalnızca uyarı. `validate_iso.py --kind object` SE: geçti (ikisi). 8 yön tutarlı, aynı kedi; ışık sol-üstten, gölge yok. 106'daki aşırı doygunluk (x1,4-1,9) giderildi; kediler artık fazla soluk.

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x8 | 8 |
| create_character v3 ref x2 | 2 |
| Toplam | 10 |

`get_balance` sonrası: 4641,9 (paralel şeritlerin harcaması dahil).

## Sorunlar
- 6/8 aday teslim edilmedi (zemin lekesi 5, güney dışı 1). Köpek için yeni iş gerekir.
