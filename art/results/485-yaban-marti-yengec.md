unity-import: pending

# 485 Yaban martı ve yengeç: 1/2 teslim (martı), yengeç zemin lekesi yüzünden teslim edilmedi, review

Not: `art/queue/` altında iki dosya `485` numarası taşıyor: bu iş (`485-kiyi-yaban-marti-yengec.md`, sahiplenilmiş) ve `485-kedi-govdeleri.md` (pending, dokunulmadı). Numara çakışması kod oturumunca düzeltilmeli.
Yan yana sayfası (martı SE, S, NE; koyun 1 ve domuz 2 referans): [485-yanyana.png](485-yanyana.png). Teslim edilmeyen: `art/results/485-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
375 ile aynı: `create_image_pixflux`, `high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading, palet zorlaması `iso_animal_sheep_1_cand_se.png` (10 renk şerit), açıklama eki `earthy muted medieval palette, desaturated` + 375'teki "no ground disc, no floor shadow" vurgusu. Tuval: ikisi 32 (yengeç için 24 istenmişti, pixflux alanı en az 32x32). Döndürme (yalnızca martı): `create_character` v3 referanslı, `body_type=humanoid` (iki bacaklı kuş; B' kuş kuralı, quadruped iskelet ayakta duran martıya uymaz), view high top-down. `style.py fit --family hayvan` her yönde x1.00.

## Tek kare kontrolü
| Tür | Tuval | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|
| wild.gull | 32 | önden, dik | yok | döndürüldü, teslim |
| wild.crab | 32 | önden | VAR (açık gri disk), ayrıca renk koyu mor-gri (yeşil değil) | döndürülmedi |

Öneri (yengeç): palet zorlamasız ya da 3/4 yan bakışlı deneme; "green shell, orange claws" vurgusu; gri disk koyun paletindeki açık gri tondan geliyor (375/414'te de aynı).

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x2 | 2 |
| create_character v3 ref (martı) | 1 |
| Toplam | 3 (cost_max 8) |

`get_balance` sonrası: 2815,9 (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/iso_wild_gull_cand_<yön>.png`, 32x32)
| Yön | Opak kutu | Renk | Yarı saydam | Pivot x | Pivot y (alttan) |
|---|---|---|---|---|---|
| s | x8-23, y1-31 | 10 | 0 | 15,5 | 0 |
| se | x3-24, y1-31 | 15 | 0 | 16,2 | 0 |
| sw | x7-28, y1-31 | 15 | 0 | 14,8 | 0 |
| w | x3-29, y0-31 | 14 | 0 | 13,7 | 0 |
| nw | x6-27, y0-30 | 15 | 0 | 13,6 | 1 |
| n | x7-23, y0-30 | 11 | 0 | 15,0 | 1 |
| ne | x3-25, y0-30 | 15 | 0 | 17,1 | 1 |
| e | x2-28, y0-31 | 13 | 0 | 17,4 | 0 |

Pivot y yönler arası 0-1 px oynuyor. Renk 32'nin altı, azaltma gerekmedi. Tuval tam dolu (boy ~31-32 px / tuval 32): martı 32 px tuvale sığmak için büyük kaldı, hedef ölçeğe göre (kedi ~21, karga/kuşlar daha küçük) fazla büyük olabilir; kullanıcı kararı.

## Doğrulama
`validate_iso.py --kind object` SE: geçti (15 renk, yarı saydam 0). Alt boşluk 0 satır (ayaklar tuval tabanında). Gölge pişirilmemiş, zemin lekesi yok, ışık sol-üstten, 8 yön tutarlı (aynı martı; humanoid iskelet kuşu insanlaştırmadı).

### Stil denetimi (`check --family hayvan`)
Hayvan ailesi kilidi kurulmadı; palet uyumu uyarı. 8 yönün hepsi UYARI (ailesinden soluk, x0,37-0,60; beyaz-gri tüy doğal olarak düşük doygunluk), KALDI yok; palete yakın %100.

## Sorunlar
- Yengeç teslim edilmedi (zemin diski, yeşil değil); yeni iş gerekir.
- Martıda sarı gaga seçilmiyor (soluk); gaga düz açık tonda.
- İş numarası çakışması (iki `485`).
