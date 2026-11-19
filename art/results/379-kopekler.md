unity-import: pending

# 379 Köpekler: 3/4 teslim (dog_1, dog_2, dog_3); dog_4 teslim edilmedi

Sonuç: `status: review`. Masaüstü kopyası: yerel (bulut modu).
Yan yana sayfası (köpek 1-3, kedi 1-2, koyun 1, domuz 2): `art/results/379-yanyana.png`

## Araç ve ayarlar
- 375 tarifi, iki farkla: palet zorlaması yok (`color_image` verilmedi), açıklama ekine "no ground disc, no floor shadow, no grey patch under the paws" eklendi.
- Tek kare: `create_image_pixflux`, `view=high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading. Tuvaller dog_1 42, dog_2 38, dog_3 34, dog_4 42. 4 çağrı paralel.
- Döndürme: `create_character mode=v3`, quadruped, `template=dog`, `view=high top-down`, referans = pixflux çıktısının URL'si (1 gen/aday). Çıktı tuvali 44/40/36 (araç 2 px dolgu ekledi).
- Renk: `style.py fit --family hayvan` her yön için. Ham renk 26-39, `reduce_colors` gerekmedi (azaltma yapılmadı).
- Hız sınırı hatası olmadı.

## Tek kare kontrolü
| Aday | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|
| dog_1 | önden, oturan | yok | döndürüldü |
| dog_2 | önden, oturan | yok | döndürüldü |
| dog_3 | önden, oturan | yok | döndürüldü |
| dog_4 | yandan, sola bakıyor (güney değil, "front view" vurgusuna rağmen) | yok | döndürülmedi, aday raporlandı |

## Teslim edilen dosyalar (`Assets/Art/Iso/_aday/`, 24 PNG)
`iso_animal_dog_{1,2,3}_cand_{s,se,sw,w,nw,n,ne,e}.png`. Yarı saydam piksel 0. Pivot y alttan: 4 (dog_1, dog_2), 3 (dog_3) (SE). Pivot x (SE bbox merkezi) dog_1 19,0; dog_2 19,0; dog_3 14,5.

| Aday | Tuval | SE boy x en | Doluluk | SE renk | Stil denetimi (8 yön) |
|---|---|---|---|---|---|
| dog_1 | 44 | 37 x 31 | 0,88 | 27 | 8 GEÇTİ (x1,13-1,16) |
| dog_2 | 40 | 32 x 27 | 0,80 | 26 | 8 GEÇTİ (x0,61-0,73) |
| dog_3 | 36 | 28 x 24 | 0,78 | 39 | 5 UYARI (palete uzak, ort. uzaklık 9,5-11,3), 3 GEÇTİ |

Hayvan ailesinin kilidi kurulmadı (kabul referansı < 3); palet uyumu yalnızca uyarı. KALDI yok. `validate_iso.py --kind object` SE: üçü de geçti. Hedef boy ~30 px: dog_1 37 (+23 %), dog_2 32 (+7 %), dog_3 28 (-7 %).
Gölge pişirilmemiş, ışık sol-üstten, arka plan saydam. Yan yana sayfasında ayak altında gri leke yok. Köpek 1-3 oturan poz.

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x4 | 4 |
| create_character v3 ref x3 | 3 |
| Toplam | 7 |

`get_balance` sonrası: 3705,5 (paralel şeritlerin harcaması dahil).

## Sorunlar
- dog_4 (tazı) yan duruşla geldi, güney kontrolünden geçmedi. Ham tek kare `art/tmp379/dog_4_tek.png` yalnızca yerelde (commit edilmez). Gerekirse ayrı iş: pixflux yerine `create_character mode=pro` ile döndürme (10 gen) ya da tazı için başka tarif.
- dog_2 SE'de gözler sarı; ham çıktıda böyleydi.
